using System.CommandLine;

public static class VorgangCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("vorgang", "Manage Vorgänge");

        // list subcommand
        var listCmd = new Command("list", "List all Vorgänge");
        var jahrOption = new Option<int?>("--jahr", "Year to list (0 = all years)");
        var includeArchiveOption = new Option<bool>("--include-archive", () => true, "Include archived Vorgänge");
        var includeOthersDataOption = new Option<bool>("--include-others-data", () => true, "Include data from other users");
        var includeAspOption = new Option<bool>("--include-asp", () => true, "Include application specific properties");
        var includeAdditionalPropertiesOption = new Option<bool>("--include-additional-properties", () => true, "Include additional properties");

        listCmd.AddOption(jahrOption);
        listCmd.AddOption(includeArchiveOption);
        listCmd.AddOption(includeOthersDataOption);
        listCmd.AddOption(includeAspOption);
        listCmd.AddOption(includeAdditionalPropertiesOption);

        listCmd.SetHandler(async (format, filename, jahr, includeArchive, includeOthersData, includeAsp, includeAdditionalProperties) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.GetList(commonParams, jahr, includeArchive, includeOthersData, includeAsp, includeAdditionalProperties);
        }, GlobalOptions.Format, GlobalOptions.FileName, jahrOption, includeArchiveOption, includeOthersDataOption, includeAspOption, includeAdditionalPropertiesOption);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Vorgang by GUID");
        var vorgangArgument = new Argument<Guid>("vorgang", "Vorgang-GUID");
        getCmd.AddArgument(vorgangArgument);

        getCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.GetVorgang(commonParams, vorgang);
        }, GlobalOptions.Format, GlobalOptions.FileName, vorgangArgument);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a Vorgang from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Vorgang data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.PutVorgang(commonParams, file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample VorgangDTO");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.CreateSample(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        // archive subcommand
        var archiveCmd = new Command("archive", "Archiviert einen einzelnen Vorgang");
        var archiveVorgangArgument = new Argument<Guid>("vorgang", "Vorgang-GUID");
        archiveCmd.AddArgument(archiveVorgangArgument);

        archiveCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ArchiveVorgang(commonParams, vorgang);
        }, GlobalOptions.Format, GlobalOptions.FileName, archiveVorgangArgument);

        cmd.AddCommand(archiveCmd);

        // archive-bulk subcommand
        var archiveBulkCmd = new Command("archive-bulk", "Archiviert mehrere Vorgänge gleichzeitig");
        var vorgaengeArgument = new Argument<string>("vorgaenge", "Kommagetrennte Liste von Vorgang-GUIDs");
        archiveBulkCmd.AddArgument(vorgaengeArgument);

        archiveBulkCmd.SetHandler(async (format, filename, vorgaenge) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ArchiveVorgangBulk(commonParams, vorgaenge);
        }, GlobalOptions.Format, GlobalOptions.FileName, vorgaengeArgument);

        cmd.AddCommand(archiveBulkCmd);

        // activate subcommand
        var activateCmd = new Command("activate", "Activate an archived Vorgang");
        var activateVorgangArgument = new Argument<Guid>("vorgang", "Vorgang-GUID");
        activateCmd.AddArgument(activateVorgangArgument);

        activateCmd.SetHandler(async (format, filename, vorgang) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VorgangCommands();
            await handler.ActivateVorgang(commonParams, vorgang);
        }, GlobalOptions.Format, GlobalOptions.FileName, activateVorgangArgument);

        cmd.AddCommand(activateCmd);

        return cmd;
    }
}
