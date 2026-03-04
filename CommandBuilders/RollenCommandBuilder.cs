using System.CommandLine;

public static class RollenCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("rollen", "Manage Rollen");

        // listrollen subcommand
        var listCmd = new Command("listrollen", "List all Rollen");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new RollenCommands();
            await handler.ListRollen(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        return cmd;
    }
}
