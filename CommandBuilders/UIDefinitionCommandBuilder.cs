using System.CommandLine;

public static class UIDefinitionCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("uidefinition", "Manage UI definitions");

        // list subcommand
        var listCmd = new Command("list", "Get all UI definitions");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new UIDefinitionCommands();
            await handler.GetList(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a UIDefinition by GUID");
        var guidArgument = new Argument<Guid>("guid", "UIDefinition GUID");
        getCmd.AddArgument(guidArgument);

        getCmd.SetHandler(async (format, filename, guid) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new UIDefinitionCommands();
            await handler.GetUIDefinition(commonParams, guid);
        }, GlobalOptions.Format, GlobalOptions.FileName, guidArgument);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a UIDefinition from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with UIDefinition data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new UIDefinitionCommands();
            await handler.PutUIDefinition(commonParams, file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        return cmd;
    }
}
