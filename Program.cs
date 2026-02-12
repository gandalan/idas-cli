using Cocona;

var exeDir = AppContext.BaseDirectory;
var dotenv = Path.Combine(exeDir, ".env");
DotEnv.Load(dotenv);

var builder = CoconaApp.CreateBuilder();

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

