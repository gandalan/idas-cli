using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class LagerbuchungCommands : CommandsBase
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime from,
        DateTime till)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetLagerhistorieAsync(from, till));
    }

    public async Task PutBuchung(
        CommonParameters commonParams,
        string file)
    {
        var settings = await getSettings();
        
        LagerbestandWebRoutinen client = new(settings);
        var data = JsonSerializer.Deserialize<LagerbuchungDTO>(await File.ReadAllTextAsync(file));
        await dumpOutput(commonParams, await client.LagerbuchungAsync(data));
    }

    public async Task CreateSample(CommonParameters commonParams)
    {
        await dumpOutput(commonParams, new LagerbuchungDTO());
    }
}