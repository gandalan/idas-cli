using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class KonfigSatzCommands : CommandsBase
{
    [CliCommand("list", Description = "List all KonfigSätze")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [CliCommand("put", Description = "Update a KonfigSatz from JSON file")]
    public async Task PutKonfigSatz(
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        KonfigSatzInfoWebRoutinen client = new(settings);
        var konfigSatz = JsonSerializer.Deserialize<KonfigSatzInfoDTO>(await File.ReadAllTextAsync(file));
        await client.SaveKonfigSatzInfoAsync(konfigSatz);
    }
}
