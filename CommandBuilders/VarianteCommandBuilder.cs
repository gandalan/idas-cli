using System.CommandLine;

public static class VarianteCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("variante", "Manage Varianten");

        // list subcommand
        var listCmd = new Command("list", "Get all variants");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VarianteCommands();
            await handler.GetList(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Variante by GUID");
        var guidArgument = new Argument<Guid>("guid", "Variante GUID");
        var includeKonfigsOption = new Option<bool>("--include-konfigs", () => true, "Include configurations");
        var includeUIDefsOption = new Option<bool>("--include-uidefs", () => true, "Include UI definitions");

        getCmd.AddArgument(guidArgument);
        getCmd.AddOption(includeKonfigsOption);
        getCmd.AddOption(includeUIDefsOption);

        getCmd.SetHandler(async (format, filename, guid, includeKonfigs, includeUIDefs) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VarianteCommands();
            await handler.GetVariante(commonParams, guid, includeKonfigs, includeUIDefs);
        }, GlobalOptions.Format, GlobalOptions.FileName, guidArgument, includeKonfigsOption, includeUIDefsOption);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a Variante from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Variante data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VarianteCommands();
            await handler.PutVariante(commonParams, file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        // guids subcommand
        var guidsCmd = new Command("guids", "Get all Variante GUIDs");

        guidsCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new VarianteCommands();
            await handler.GetAllGuids(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(guidsCmd);

        return cmd;
    }
}
