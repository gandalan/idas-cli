using System.CommandLine;

public static class GSQLCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("gsql", "Manage gSQL (IBOS1/Bestellungen) data");

        // list subcommand
        var listCmd = new Command("list", "List all Bestellungen");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new gSQLCommands();
            await handler.GetList(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Beleg by GUID");
        var belegArgument = new Argument<Guid>("beleg", "Beleg-GUID");
        getCmd.AddArgument(belegArgument);

        getCmd.SetHandler(async (format, filename, beleg) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new gSQLCommands();
            await handler.GetBeleg(commonParams, beleg);
        }, GlobalOptions.Format, GlobalOptions.FileName, belegArgument);

        cmd.AddCommand(getCmd);

        // reset subcommand
        var resetCmd = new Command("reset", "Reset Bestellungen since a given date");
        var sinceArgument = new Argument<DateTime>("since", "Reset since date");
        resetCmd.AddArgument(sinceArgument);

        resetCmd.SetHandler(async (since) =>
        {
            var handler = new gSQLCommands();
            await handler.Reset(since);
        }, sinceArgument);

        cmd.AddCommand(resetCmd);

        return cmd;
    }
}
