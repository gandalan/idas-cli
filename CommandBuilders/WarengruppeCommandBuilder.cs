using System.CommandLine;

public static class WarengruppeCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("warengruppe", "Get the list of product groups, including all their products");

        // list subcommand
        var listCmd = new Command("list", "Get the list of product groups, including all their products");

        listCmd.SetHandler(async (format, filename) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new WarengruppeCommands();
                await handler.GetList(commonParams);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        return cmd;
    }
}
