using System.CommandLine;

public static class CliRootCommandFactory
{
    private static readonly (string Name, Func<Command> Build)[] TopLevelCommandBuilders =
    {
        ("vorgang", VorgangCommandBuilder.Build),
        ("gsql", GSQLCommandBuilder.Build),
        ("kontakt", KontaktCommandBuilder.Build),
        ("artikel", ArtikelCommandBuilder.Build),
        ("av", AVCommandBuilder.Build),
        ("lagerbestand", LagerbestandCommandBuilder.Build),
        ("lagerbuchung", LagerbuchungCommandBuilder.Build),
        ("warengruppe", WarengruppeCommandBuilder.Build),
        ("benutzer", BenutzerCommandBuilder.Build),
        ("serie", SerieCommandBuilder.Build),
        ("rollen", RollenCommandBuilder.Build),
        ("variante", VarianteCommandBuilder.Build),
        ("uidefinition", UIDefinitionCommandBuilder.Build),
        ("konfigsatz", KonfigSatzCommandBuilder.Build),
        ("werteliste", WertelisteCommandBuilder.Build),
        ("berechtigung", BerechtigungCommandBuilder.Build),
        ("mcp", McpServerCommandBuilder.Build),
        ("beleg", BelegCommandBuilder.Build)
    };

    public static RootCommand BuildRootCommand(string description, bool includeMcpServer = true)
    {
        var rootCommand = new RootCommand(description);

        rootCommand.AddGlobalOption(GlobalOptions.Format);
        rootCommand.AddGlobalOption(GlobalOptions.FileName);

        foreach (var command in CreateTopLevelCommands(includeMcpServer))
        {
            rootCommand.AddCommand(command);
        }

        return rootCommand;
    }

    public static IReadOnlyList<Command> CreateTopLevelCommands(bool includeMcpServer = true)
    {
        return TopLevelCommandBuilders
            .Where(builder => includeMcpServer || !builder.Name.Equals("mcp", StringComparison.OrdinalIgnoreCase))
            .Select(builder => builder.Build())
            .ToArray();
    }
}