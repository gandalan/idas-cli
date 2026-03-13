using System.CommandLine;

public static class BelegCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("beleg", "Manage Belege");

        // list subcommand
        var listCmd = new Command("list", "List all Belege");
        var jahrOption = new Option<int>("--jahr", () => 0, "Jahr filtern (0 = alle Jahre)");
        var formatOption = new Option<string>("--format", () => "json", "Output format: json oder csv");
        var separatorOption = new Option<string>("--separator", () => ";", "CSV-Trennzeichen");
        var belegartOption = new Option<string?>("--belegart", "Belegart filtern (z.B. AB, Angebot, Rechnung)");
        var filenameOption = new Option<string?>("--filename", "Output in Datei speichern");
        var includeArchiveOption = new Option<bool>("--includeArchive", () => true, "Archivierte Vorgänge inkludieren");

        listCmd.AddOption(jahrOption);
        listCmd.AddOption(formatOption);
        listCmd.AddOption(separatorOption);
        listCmd.AddOption(belegartOption);
        listCmd.AddOption(filenameOption);
        listCmd.AddOption(includeArchiveOption);

        listCmd.SetHandler(async (jahr, format, separator, belegart, filename, includeArchive) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var handler = new BelegCommands();
                await handler.List(jahr, format, separator, belegart, filename, includeArchive);
            });
        }, jahrOption, formatOption, separatorOption, belegartOption, filenameOption, includeArchiveOption);

        cmd.AddCommand(listCmd);

        return cmd;
    }
}
