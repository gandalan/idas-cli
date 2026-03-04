using System.CommandLine;

public static class RollenCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("rollen", "Manage Rollen");

        // list subcommand
        var listCmd = new Command("list", "List all Rollen");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new RollenCommands();
            await handler.List(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Rolle by GUID");
        var rolleGuidArg = new Argument<Guid>("rolleGuid", "Rolle GUID");
        getCmd.AddArgument(rolleGuidArg);

        getCmd.SetHandler(async (format, filename, rolleGuid) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new RollenCommands();
            await handler.Get(commonParams, rolleGuid);
        }, GlobalOptions.Format, GlobalOptions.FileName, rolleGuidArg);

        cmd.AddCommand(getCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample Rolle JSON");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new RollenCommands();
            await handler.Sample(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        return cmd;
    }
}
