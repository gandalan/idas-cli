using System.CommandLine;

public static class AVCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("av", "Manage AV (BelegPositionAV) data");

        // list subcommand
        var listCmd = new Command("list", "List all BelegPositionen AV");
        var sinceOption = new Option<DateTime?>("--since", "Reset since date");
        sinceOption.SetDefaultValue(null);
        listCmd.AddOption(sinceOption);

        listCmd.SetHandler(async (format, filename, since) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new AVCommands();
                await handler.GetList(commonParams, since);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, sinceOption);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a BelegPosition AV by GUID or PCode");
        var posArgument = new Argument<string>("pos", "AVPos-GUID or PCode");
        getCmd.AddArgument(posArgument);

        getCmd.SetHandler(async (format, filename, pos) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new AVCommands();
                await handler.GetPos(commonParams, pos);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, posArgument);

        cmd.AddCommand(getCmd);

        return cmd;
    }
}
