using System.CommandLine;

public static class ArtikelCommandBuilder
{
    public static Command Build()
    {
        var cmd = new Command("artikel", "Manage Artikel");

        // list subcommand
        var listCmd = new Command("list", "List all Artikel");

        listCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new ArtikelCommands();
            await handler.GetList(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(listCmd);

        // put subcommand
        var putCmd = new Command("put", "Put an Artikel from JSON file");
        var fileArgument = new Argument<string>("file", "JSON file with Artikel data");
        putCmd.AddArgument(fileArgument);

        putCmd.SetHandler(async (file) =>
        {
            var handler = new ArtikelCommands();
            await handler.PutArtikel(file);
        }, fileArgument);

        cmd.AddCommand(putCmd);

        // sample subcommand
        var sampleCmd = new Command("sample", "Create a sample KatalogArtikelDTO");

        sampleCmd.SetHandler(async (format, filename) =>
        {
            var commonParams = new CommonParameters(format, filename);
            var handler = new ArtikelCommands();
            await handler.CreateSample(commonParams);
        }, GlobalOptions.Format, GlobalOptions.FileName);

        cmd.AddCommand(sampleCmd);

        return cmd;
    }
}
