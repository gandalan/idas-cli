using Cocona;

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

var builder = CoconaApp.CreateBuilder(effectiveArgs);

var app = builder.Build();
app.AddSubCommand("vorgang", x => x.AddCommands<VorgangCommands>());
app.AddSubCommand("gsql", x => x.AddCommands<gSQLCommands>());
app.AddSubCommand("kontakt", x => x.AddCommands<KontaktCommands>());
app.AddSubCommand("artikel", x => x.AddCommands<ArtikelCommands>());
app.AddSubCommand("av", x => x.AddCommands<AVCommands>());
app.AddSubCommand("lagerbestand", x => x.AddCommands<LagerbestandCommands>());
app.AddSubCommand("lagerbuchung", x => x.AddCommands<LagerbuchungCommands>());
app.AddSubCommand("warengruppe", x => x.AddCommands<WarengruppeCommands>());
app.AddSubCommand("benutzer", x => x.AddCommands<BenutzerCommands>());
app.AddSubCommand("serie", x => x.AddCommands<SerieCommands>());
app.AddSubCommand("rollen", x => x.AddCommands<RollenCommands>());
app.AddSubCommand("variante", x => x.AddCommands<VarianteCommands>());
app.AddSubCommand("uidefinition", x => x.AddCommands<UIDefinitionCommands>());
app.AddSubCommand("konfigsatz", x => x.AddCommands<KonfigSatzCommands>());
app.AddSubCommand("werteliste", x => x.AddCommands<WertelisteCommands>());
app.AddSubCommand("mcp", x => x.AddCommands<McpServerCommand>());
app.AddSubCommand("beleg", x => x.AddCommands<BelegCommands>());
app.Run();

public record CommonParameters(
    [Option("format", Description = "Output format")] string Format = "json",
    [Option("filename", Description = "Dump output to file")] string? FileName = null
) : ICommandParameterSet;
