using System.CommandLine;

public static class KonfigSatzCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("konfigsatz", "Manage KonfigSatz configuration sets");

        // list subcommand
        var listCmd = new Command("list", "Get all configuration sets");

        listCmd.SetHandler(async (format, filename) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KonfigSatzCommands();
                await handler.GetList(commonParams);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a KonfigSatz from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with KonfigSatzInfo data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KonfigSatzCommands();
                await handler.PutKonfigSatz(file);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        return cmd;
    }
}
