using System.CommandLine;

public static class BerechtigungCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("berechtigung", "Manage Berechtigungen (permissions)");

        // list subcommand
        var listCmd = new Command("list", "List all available Berechtigungen");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new BerechtigungCommands();
            await handler.List(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        return cmd;
    }
}
