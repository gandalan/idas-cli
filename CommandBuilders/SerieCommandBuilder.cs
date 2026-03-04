using System.CommandLine;

public static class SerieCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("serie", "Manage Serien");

        // list subcommand
        var listCmd = new Command("list", "List all Serien");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new SerieCommands();
            await handler.GetSerienList(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // get subcommand
        var getCmd = new Command("get", "Get a Serie by GUID");
        var serieArgument = new Argument<Guid>("serie", "Serie-GUID");
        getCmd.AddArgument(serieArgument);

        getCmd.SetHandler(async (format, filename, serie) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new SerieCommands();
            await handler.GetSerie(commonParams, serie);
        }, GlobalOptions.Format, GlobalOptions.FileName, serieArgument);

        cmd.AddCommand(getCmd);

        // put subcommand
        var putCmd = new Command("put", "Put a Serie from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Serie data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (format, filename, file) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new SerieCommands();
            await handler.PutSerie(file);
        }, GlobalOptions.Format, GlobalOptions.FileName, fileArgument);

        cmd.AddCommand(putCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample SerieDTO");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new SerieCommands();
            await handler.CreateSampleSerie(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        return cmd;
    }
}
