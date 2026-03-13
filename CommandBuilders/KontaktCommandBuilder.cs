using System.CommandLine;

public static class KontaktCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("kontakt", "Manage contacts");

        // list subcommand
        var listCmd = new Command("list", "List all contacts");

        listCmd.SetHandler(async (format, filename) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KontaktCommands();
                await handler.GetList(commonParams);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a contact by GUID");
        var kontaktArgument = new Argument<Guid>("kontakt", "Kontakt-GUID");
        getCmd.AddArgument(kontaktArgument);

        getCmd.SetHandler(async (format, filename, kontakt) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KontaktCommands();
                await handler.GetKontakt(commonParams, kontakt);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, kontaktArgument);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a contact from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Kontakt data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KontaktCommands();
                await handler.PutKontakt(commonParams, file);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample KontaktDTO");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            await CommandsBase.ExecuteWithErrorHandling(async () =>
            {
                var commonParams = new CommonParameters(format, filename);
                var handler = new KontaktCommands();
                await handler.CreateSample(commonParams);
            });
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        return cmd;
    }
}
