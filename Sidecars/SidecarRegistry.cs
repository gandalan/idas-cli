using System.Diagnostics;
using System.Text.Json;

namespace IdasCli.Sidecars;

public static class SidecarRegistry
{
    private static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;
    private static IReadOnlyList<SidecarDescriptor>? _cache;
    private static string? _cacheKey;

    public static IReadOnlyList<SidecarDescriptor> Discover(IReadOnlyCollection<string>? builtInCommandNames = null)
    {
        var cacheKey = BuildCacheKey();
        if (_cache != null && string.Equals(_cacheKey, cacheKey, StringComparison.Ordinal))
        {
            return _cache;
        }

        var builtIns = new HashSet<string>(builtInCommandNames ?? Array.Empty<string>(), NameComparer);
        var result = new Dictionary<string, SidecarDescriptor>(NameComparer);

        foreach (var directory in GetSearchDirectories())
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var descriptor in DiscoverInDirectory(directory, builtIns))
            {
                if (!result.ContainsKey(descriptor.CommandName))
                {
                    result[descriptor.CommandName] = descriptor;
                }
            }
        }

        _cacheKey = cacheKey;
        _cache = result.Values.OrderBy(value => value.CommandName, NameComparer).ToArray();
        return _cache;
    }

    public static SidecarDescriptor? Find(string commandName, IReadOnlyCollection<string>? builtInCommandNames = null)
    {
        return Discover(builtInCommandNames)
            .FirstOrDefault(sidecar => NameComparer.Equals(sidecar.CommandName, commandName));
    }

    public static IEnumerable<SidecarDescriptor> GetAll(IReadOnlyCollection<string>? builtInCommandNames = null)
    {
        return Discover(builtInCommandNames);
    }

    private static IEnumerable<SidecarDescriptor> DiscoverInDirectory(string directory, HashSet<string> builtIns)
    {
        foreach (var candidate in GetCandidateFileNames(directory))
        {
            var commandName = TryGetCommandName(candidate);
            if (commandName == null || builtIns.Contains(commandName))
            {
                continue;
            }

            if (!File.Exists(candidate) || !IsExecutable(candidate))
            {
                continue;
            }

            yield return new SidecarDescriptor(
                CommandName: commandName,
                ExecutablePath: candidate,
                DisplayName: Path.GetFileName(candidate),
                Description: ReadDescription(candidate, commandName));
        }
    }

    private static IEnumerable<string> GetCandidateFileNames(string directory)
    {
        return Directory.EnumerateFiles(directory, "idas-*", SearchOption.TopDirectoryOnly)
            .OrderBy(path => GetPlatformPriority(path))
            .ThenBy(path => path, NameComparer);
    }

    private static int GetPlatformPriority(string path)
    {
        var extension = Path.GetExtension(path);
        if (OperatingSystem.IsWindows())
        {
            return extension.ToLowerInvariant() switch
            {
                ".exe" => 0,
                ".cmd" => 1,
                ".bat" => 2,
                _ => 3
            };
        }

        return string.IsNullOrEmpty(extension) ? 0 : 1;
    }

    private static string? TryGetCommandName(string path)
    {
        var fileName = Path.GetFileName(path);
        var extension = Path.GetExtension(fileName);
        var baseName = string.IsNullOrEmpty(extension)
            ? fileName
            : Path.GetFileNameWithoutExtension(fileName);

        if (!baseName.StartsWith("idas-", StringComparison.OrdinalIgnoreCase) || baseName.Length <= 5)
        {
            return null;
        }

        var commandName = baseName[5..];
        return commandName.All(ch => char.IsLetterOrDigit(ch))
            ? commandName
            : null;
    }

    private static bool IsExecutable(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            var extension = Path.GetExtension(path);
            return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".bat", StringComparison.OrdinalIgnoreCase);
        }

        try
        {
            var mode = File.GetUnixFileMode(path);
            return mode.HasFlag(UnixFileMode.UserExecute)
                || mode.HasFlag(UnixFileMode.GroupExecute)
                || mode.HasFlag(UnixFileMode.OtherExecute);
        }
        catch
        {
            return false;
        }
    }

    private static string ReadDescription(string executablePath, string commandName)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            };

            startInfo.Environment["IDAS_SIDECAR_DESCRIBE"] = "1";

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return FallbackDescription(commandName, executablePath);
            }

            if (!process.WaitForExit(2000))
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }

                return FallbackDescription(commandName, executablePath);
            }

            var stdout = process.StandardOutput.ReadToEnd().Trim();
            if (process.ExitCode != 0 || string.IsNullOrWhiteSpace(stdout))
            {
                return FallbackDescription(commandName, executablePath);
            }

            var payload = JsonSerializer.Deserialize<SidecarDescriptionPayload>(stdout);
            if (!string.IsNullOrWhiteSpace(payload?.Description))
            {
                return payload.Description;
            }
        }
        catch
        {
        }

        return FallbackDescription(commandName, executablePath);
    }

    private static string FallbackDescription(string commandName, string executablePath)
    {
        return $"Externes Kommando '{commandName}' ({Path.GetFileName(executablePath)}).";
    }

    private static IEnumerable<string> GetSearchDirectories()
    {
        var directories = new List<string>();
        var seen = new HashSet<string>(NameComparer);

        void AddDirectory(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            var fullPath = Path.GetFullPath(path);
            if (seen.Add(fullPath))
            {
                directories.Add(fullPath);
            }
        }

        AddDirectory(AppContext.BaseDirectory);
        AddDirectory(Directory.GetCurrentDirectory());

        var envPath = Environment.GetEnvironmentVariable("IDAS_SIDECAR_PATH");
        if (!string.IsNullOrWhiteSpace(envPath))
        {
            foreach (var entry in envPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                AddDirectory(entry);
            }
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(home))
        {
            AddDirectory(Path.Combine(home, ".idas", "commands"));
        }

        return directories;
    }

    private static string BuildCacheKey()
    {
        return string.Join("|", GetSearchDirectories().Select(path => $"{path}:{Directory.Exists(path)}"));
    }

    private sealed record SidecarDescriptionPayload(string? Description);
}
