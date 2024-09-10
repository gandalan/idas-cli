using Cocona;

var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, ".env");
DotEnv.Load(dotenv);

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();
app.AddCommands<UserCommands>();
app.AddSubCommand("vorgang", x => x.AddCommands<VorgangCommands>());
app.AddSubCommand("gsql", x => x.AddCommands<gSQLCommands>());
app.AddSubCommand("kontakt", x => x.AddCommands<KontaktCommands>());
app.AddSubCommand("artikel", x => x.AddCommands<ArtikelCommands>());
app.AddSubCommand("av", x => x.AddCommands<AVCommands>());

app.Run();

public record CommonParameters(
    [Option("format", Description = "Output format")] string Format = "json",
    [Option("filename", Description = "Dump output to file")] string? FileName = null
) : ICommandParameterSet;

