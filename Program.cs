using System.CommandLine;

var exeDir = AppContext.BaseDirectory;
var workDir = Directory.GetCurrentDirectory();

// Resolve configuration using FirstRunManager
var configManager = new FirstRunManager(args);
if (!configManager.TryResolveConfiguration(out var appGuid, out var env))
{
    Environment.Exit(1);
    return 1;
}

// Set environment variables for this process so CommandsBase can read them
Environment.SetEnvironmentVariable("IDAS_APPGUID", appGuid);
Environment.SetEnvironmentVariable("IDAS_ENV", env);

// If called without arguments and no token file exists, auto-start login
string[] effectiveArgs = args;
if (args.Length == 0 && !File.Exists("token"))
{
    Console.WriteLine("Starting SSO-Login...");
    Console.WriteLine();
    effectiveArgs = new[] { "benutzer", "login" };
}

var rootCommand = CliRootCommandFactory.BuildRootCommand(
    "IDAS CLI - Command line interface for IDAS/i3 ERP system",
    includeMcpServer: true);

return await rootCommand.InvokeAsync(effectiveArgs);

public record CommonParameters(
    string Format = "json",
    string? FileName = null
);
