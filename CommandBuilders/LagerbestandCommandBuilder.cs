using System.CommandLine;

public static class LagerbestandCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("lagerbestand", "Get inventory list");

        // list subcommand
        var listCmd = new Command("list", "Get the inventory list");
        var sinceOption = new Option<DateTime?>("--since", "Reset since");

        listCmd.AddOption(sinceOption);

        listCmd.SetHandler(async (format, filename, since) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new LagerbestandCommands();
            await handler.GetList(commonParams, since);
        }, GlobalOptions.Format, GlobalOptions.FileName, sinceOption);

        cmd.AddCommand(listCmd);

        return cmd;
    }
}
