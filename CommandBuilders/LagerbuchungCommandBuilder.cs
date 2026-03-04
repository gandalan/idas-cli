using System.CommandLine;

public static class LagerbuchungCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("lagerbuchung", "Manage Lagerbuchungen");

        // list subcommand
        var listCmd = new Command("list", "Get the booking list");
        var fromOption = new Option<DateTime>("--from", "Start date of booking list");
        var tillOption = new Option<DateTime>("--till", "End date for booking list");

        listCmd.AddOption(fromOption);
        listCmd.AddOption(tillOption);

        listCmd.SetHandler(async (format, filename, from, till) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new LagerbuchungCommands();
            await handler.GetList(commonParams, from, till);
        }, GlobalOptions.Format, GlobalOptions.FileName, fromOption, tillOption);

        cmd.AddCommand(listCmd);

        // put subcommand
        var putCmd = new Command("put", "Book inventory");
        var fileArgument = new Argument<string>("file", "JSON file with LagerbuchungDTO data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new LagerbuchungCommands();
            await handler.PutBuchung(commonParams, file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample LagerbuchungDTO");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new LagerbuchungCommands();
            await handler.CreateSample(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        return cmd;
    }
}
