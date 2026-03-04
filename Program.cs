using System.CommandLine;

var exeDir = AppContext.BaseDirectory;
var workDir = Directory.GetCurrentDirectory();

// Resolve configuration using FirstRunManager
var configManager = new FirstRunManager(args);
if (!configManager.TryResolveConfiguration(out var appGuid, out var env))
{
    Environment.Exit(1);
    return;
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

var rootCommand = new RootCommand("IDAS CLI - Command line interface for IDAS/i3 ERP system");

// Add global options
rootCommand.AddGlobalOption(GlobalOptions.Format);
rootCommand.AddGlobalOption(GlobalOptions.FileName);

// Add all command builders
rootCommand.AddCommand(VorgangCommandBuilder.Build());
rootCommand.AddCommand(GSQLCommandBuilder.Build());
rootCommand.AddCommand(KontaktCommandBuilder.Build());
rootCommand.AddCommand(ArtikelCommandBuilder.Build());
rootCommand.AddCommand(AVCommandBuilder.Build());
rootCommand.AddCommand(LagerbestandCommandBuilder.Build());
rootCommand.AddCommand(LagerbuchungCommandBuilder.Build());
rootCommand.AddCommand(WarengruppeCommandBuilder.Build());
rootCommand.AddCommand(BenutzerCommandBuilder.Build());
rootCommand.AddCommand(SerieCommandBuilder.Build());
rootCommand.AddCommand(RollenCommandBuilder.Build());
rootCommand.AddCommand(VarianteCommandBuilder.Build());
rootCommand.AddCommand(UIDefinitionCommandBuilder.Build());
rootCommand.AddCommand(KonfigSatzCommandBuilder.Build());
rootCommand.AddCommand(WertelisteCommandBuilder.Build());
rootCommand.AddCommand(McpServerCommandBuilder.Build());
rootCommand.AddCommand(BelegCommandBuilder.Build());

return await rootCommand.InvokeAsync(effectiveArgs);

public record CommonParameters(
    string Format = "json",
    string? FileName = null
);
