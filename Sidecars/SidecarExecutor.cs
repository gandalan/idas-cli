using System.Diagnostics;
using System.Text.Json;

namespace IdasCli.Sidecars;

public static class SidecarExecutor
{
    public static async Task<int> ExecuteAsync(SidecarDescriptor sidecar, IReadOnlyList<string> arguments)
    {
        var context = BuildInvocationContext();
        var startInfo = new ProcessStartInfo
        {
            FileName = sidecar.ExecutablePath,
            UseShellExecute = false,
            WorkingDirectory = Directory.GetCurrentDirectory(),
            RedirectStandardInput = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        startInfo.Environment["IDAS_SIDECAR_CONTEXT_STDIN"] = "1";

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        if (process == null)
        {
            Console.Error.WriteLine($"Error: Sidecar '{sidecar.DisplayName}' konnte nicht gestartet werden.");
            return 1;
        }

        await process.StandardInput.WriteAsync(JsonSerializer.Serialize(context));
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    private static SidecarInvocationContext BuildInvocationContext()
    {
        return new SidecarInvocationContext(
            AppGuid: Environment.GetEnvironmentVariable("IDAS_APPGUID"),
            Environment: Environment.GetEnvironmentVariable("IDAS_ENV"),
            TokenJson: TryReadTokenJson(),
            WorkingDirectory: Directory.GetCurrentDirectory(),
            TimestampUtc: DateTime.UtcNow);
    }

    private static string? TryReadTokenJson()
    {
        var tokenPath = Path.Combine(Directory.GetCurrentDirectory(), "token");
        return File.Exists(tokenPath)
            ? File.ReadAllText(tokenPath)
            : null;
    }

    private sealed record SidecarInvocationContext(
        string? AppGuid,
        string? Environment,
        string? TokenJson,
        string? WorkingDirectory,
        DateTime TimestampUtc);
}
