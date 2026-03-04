using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class LagerbuchungCommands : CommandsBase
{
    [CliCommand("list", Description = "List Lagerbuchungen in date range")]
    public async Task GetList(
        CommonParameters commonParams,
        [CliOption(Description = "Start date")] DateTime from,
        [CliOption(Description = "End date")] DateTime till)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetLagerhistorieAsync(from, till));
    }

    [CliCommand("put", Description = "Create a Lagerbuchung from JSON file")]
    public async Task PutBuchung(
        CommonParameters commonParams,
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        
        LagerbestandWebRoutinen client = new(settings);
        var data = JsonSerializer.Deserialize<LagerbuchungDTO>(await File.ReadAllTextAsync(file));
        await dumpOutput(commonParams, await client.LagerbuchungAsync(data));
    }

    [CliCommand("sample", Description = "Create a sample Lagerbuchung JSON")]
    public async Task CreateSample(CommonParameters commonParams)
    {
        await dumpOutput(commonParams, new LagerbuchungDTO());
    }
}