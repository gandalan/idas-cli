using System;
using System.IO;
using System.Linq;

/// <summary>
/// Manages first-run configuration resolution including:
/// - Parsing command line flags
/// - Reading environment variables and .env files
/// - Interactive prompting when configuration is missing
/// - Persisting configuration to .env files
/// </summary>
public class FirstRunManager
{
    private readonly string _workDir;
    private readonly string _exeDir;
    private readonly string _dotenvLocal;
    private readonly string _dotenvExe;
    private readonly string[] _args;
    private readonly string[] _validEnvs = { "dev", "stg", "prod" };
    private const string DefaultEnv = "prod";

    public FirstRunManager(string[] args)
    {
        _args = args;
        _exeDir = AppContext.BaseDirectory;
        _workDir = Directory.GetCurrentDirectory();
        _dotenvLocal = Path.Combine(_workDir, ".env");
        _dotenvExe = Path.Combine(_exeDir, ".env");
    }

    /// <summary>
    /// Resolves AppGuid and Environment configuration.
    /// Returns true if successful, false if failed (non-interactive and missing config).
    /// </summary>
    public bool TryResolveConfiguration(out string appGuid, out string env)
    {
        appGuid = ResolveAppGuid()!;
        env = ResolveEnv()!;

        var needsAppGuid = string.IsNullOrWhiteSpace(appGuid) || !Guid.TryParse(appGuid, out _);
        var needsEnv = string.IsNullOrWhiteSpace(env);

        if (!needsAppGuid && !needsEnv)
        {
            // All values present from flags/env/.env - no interactive needed
            return true;
        }

        // Configuration is missing - need to prompt
        if (!IsInteractive())
        {
            ShowNonInteractiveError();
            return false;
        }

        return PromptForConfiguration(ref appGuid, ref env);
    }

    private string? ResolveAppGuid()
    {
        // 1. Check flags
        var flag = ParseFlag("--appguid");
        if (!string.IsNullOrEmpty(flag))
            return flag;

        // 2. Check environment variables
        var envVar = Environment.GetEnvironmentVariable("IDAS_APPGUID");
        if (!string.IsNullOrEmpty(envVar))
            return envVar;

        // 3. Check .env files
        return ReadEnvValue(_dotenvLocal, "IDAS_APPGUID") 
            ?? ReadEnvValue(_dotenvExe, "IDAS_APPGUID");
    }

    private string? ResolveEnv()
    {
        // 1. Check flags
        var flag = ParseFlag("--env");
        if (!string.IsNullOrEmpty(flag))
            return flag;

        // 2. Check environment variables
        var envVar = Environment.GetEnvironmentVariable("IDAS_ENV");
        if (!string.IsNullOrEmpty(envVar))
            return envVar;

        // 3. Check .env files
        return ReadEnvValue(_dotenvLocal, "IDAS_ENV") 
            ?? ReadEnvValue(_dotenvExe, "IDAS_ENV");
    }

    private string? ParseFlag(string flagName)
    {
        for (int i = 0; i < _args.Length; i++)
        {
            var arg = _args[i];
            
            // Check for --flag=value format
            var prefix = flagName + "=";
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return arg.Substring(prefix.Length);
            }
            
            // Check for --flag value format
            if (arg.Equals(flagName, StringComparison.OrdinalIgnoreCase) && i + 1 < _args.Length)
            {
                return _args[i + 1];
            }
        }

        return null;
    }

    private static string? ReadEnvValue(string filePath, string key)
    {
        if (!File.Exists(filePath))
            return null;

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0] == key)
            {
                return parts[1];
            }
        }

        return null;
    }

    private static bool IsInteractive()
    {
        return !Console.IsInputRedirected && !Console.IsOutputRedirected;
    }

    private static void ShowNonInteractiveError()
    {
        Console.Error.WriteLine("Error: Missing required settings.");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Please either:");
        Console.Error.WriteLine("  - Set IDAS_APPGUID and IDAS_ENV environment variables");
        Console.Error.WriteLine("  - Use --appguid and --env flags");
        Console.Error.WriteLine("  - Run interactively to configure and save to .env");
    }

    private bool PromptForConfiguration(ref string appGuid, ref string env)
    {
        Console.WriteLine("Configuration required - some settings are missing.");
        Console.WriteLine();

        // Prompt for AppGuid if missing
        var needsAppGuid = string.IsNullOrWhiteSpace(appGuid) || !Guid.TryParse(appGuid, out _);
        if (needsAppGuid)
        {
            Console.Write("AppGuid (provided by Gandalan): ");
            var input = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(input) || !Guid.TryParse(input, out _))
            {
                Console.WriteLine("Error: Invalid or missing AppGuid.");
                return false;
            }
            
            appGuid = input;
        }

        // Always prompt for environment (per requirements) - defaults to prod
        Console.Write($"Environment ({string.Join("/", _validEnvs)}) [{DefaultEnv}]: ");
        var envInput = Console.ReadLine()?.Trim();
        env = string.IsNullOrWhiteSpace(envInput) ? DefaultEnv : envInput;

        // Validate environment value
        if (!_validEnvs.Contains(env.ToLowerInvariant()))
        {
            Console.WriteLine($"Warning: Unknown environment '{env}'. Expected: {string.Join(", ", _validEnvs)}");
        }

        // Save to .env file
        SaveConfiguration(appGuid, env);
        Console.WriteLine();

        return true;
    }

    private void SaveConfiguration(string appGuid, string env)
    {
        var targetEnvFile = File.Exists(_dotenvLocal) ? _dotenvLocal : _dotenvExe;
        
        if (File.Exists(targetEnvFile))
        {
            Console.WriteLine();
            Console.Write($"Overwrite existing .env file at {targetEnvFile}? (y/N): ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Configuration not saved. Using values for this session only.");
                return;
            }
        }

        SaveEnvFile(targetEnvFile, appGuid, env);
        Console.WriteLine($"✓ Configuration saved to {targetEnvFile}");
    }

    private static void SaveEnvFile(string filePath, string appGuid, string env)
    {
        var content = $"IDAS_APPGUID={appGuid}\nIDAS_ENV={env}\n";
        File.WriteAllText(filePath, content);
    }
}
