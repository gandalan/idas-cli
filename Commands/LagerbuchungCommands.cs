using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class LagerbuchungCommands : CommandsBase
{
    [Command("list", Description = "Get the booking list")]
    public async Task GetList(CommonParameters commonParams, 
        [Option("from", Description = "Start date of booking list")] DateTime from,
        [Option("till", Description = "End date for booking list")] DateTime till)
    {
        var settings = await getSettings();
        LagerbestandWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetLagerhistorieAsync(from, till));
    }

    [Command("put", Description = "Book inventory")]
    public async Task PutBuchung(
        CommonParameters commonParams, 
        [Argument("file", Description = "JSON file with LagerbuchungDTO data")]
        string file)
    {
        var settings = await getSettings();
        
        LagerbestandWebRoutinen client = new(settings);
        var data = JsonSerializer.Deserialize<LagerbuchungDTO>(await File.ReadAllTextAsync(file));
        await dumpOutput(commonParams, await client.LagerbuchungAsync(data));
    }

    [Command("sample", Description = "Create a sample LagerbuchungDTO")]
    public async Task CreateSample(CommonParameters commonParams)
    {
        await dumpOutput(commonParams, new LagerbuchungDTO());
    }
}