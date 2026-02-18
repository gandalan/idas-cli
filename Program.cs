using Cocona;

var exeDir = AppContext.BaseDirectory;
var workDir = Directory.GetCurrentDirectory();
var dotenvLocal = Path.Combine(workDir, ".env");
var dotenvExe = Path.Combine(exeDir, ".env");
var dotenvSample = Path.Combine(exeDir, ".env.sample");

// First-Run Setup: if no .env exists but sample does, ask user for config
if (!File.Exists(dotenvLocal) && !File.Exists(dotenvExe) && File.Exists(dotenvSample))
{
    Console.WriteLine("Ersteinrichtung - keine .env Konfiguration gefunden.");
    Console.WriteLine();
    
    Console.Write("IDAS App Token (Guid von Gandalan): ");
    var appToken = Console.ReadLine()?.Trim();
    
    if (string.IsNullOrWhiteSpace(appToken) || !Guid.TryParse(appToken, out _))
    {
        Console.WriteLine("Fehler: Kein gültiger App Token angegeben.");
        Environment.Exit(1);
        return;
    }
    
    Console.Write("Environment (dev/staging/produktiv) [dev]: ");
    var env = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(env)) env = "dev";
    
    File.WriteAllText(dotenvExe, $"IDAS_APP_TOKEN={appToken}\nIDAS_ENV={env}\n");
    Console.WriteLine();
    Console.WriteLine("✓ Konfiguration gespeichert in .env");
    Console.WriteLine();
}

// Load .env files - local takes precedence over exe directory
DotEnv.Load(dotenvLocal);
if (!string.Equals(dotenvLocal, dotenvExe, StringComparison.OrdinalIgnoreCase))
{
    DotEnv.Load(dotenvExe);
}

// If called without arguments and no token file exists, auto-start login
string[] effectiveArgs = args;
if (args.Length == 0 && !File.Exists("token"))
{
    Console.WriteLine("Starte SSO-Login...");
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

