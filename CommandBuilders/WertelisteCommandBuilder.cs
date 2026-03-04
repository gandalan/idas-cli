using System.CommandLine;

public static class WertelisteCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("werteliste", "Manage Werteliste value lists");

        // list subcommand
        var listCmd = new Command("list", "Get all value lists");
        var includeAutoListOption = new Option<bool>("--include-auto", () => true, "Include auto-generated value lists");
        listCmd.AddOption(includeAutoListOption);

        listCmd.SetHandler(async (format, filename, includeAuto) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new WertelisteCommands();
            await handler.GetList(commonParams, includeAuto);
        }, GlobalOptions.Format, GlobalOptions.FileName, includeAutoListOption);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Werteliste by GUID");
        var guidArgument = new Argument<Guid>("guid", "Werteliste GUID");
        var includeAutoGetOption = new Option<bool>("--include-auto", () => true, "Include auto-generated value lists");
        getCmd.AddArgument(guidArgument);
        getCmd.AddOption(includeAutoGetOption);

        getCmd.SetHandler(async (format, filename, guid, includeAuto) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new WertelisteCommands();
            await handler.GetWerteliste(guid, commonParams, includeAuto);
        }, GlobalOptions.Format, GlobalOptions.FileName, guidArgument, includeAutoGetOption);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a Werteliste from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Werteliste data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new WertelisteCommands();
            await handler.PutWerteliste(file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        return cmd;
    }
}
