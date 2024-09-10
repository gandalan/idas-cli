using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class AVCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(
        CommonParameters commonParams,
        [Option("since", Description = "Reset since")] DateTime? since)
    {
        var settings = await getSettings();
        AVWebRoutinen client = new(settings);
        var data = await client.GetAllBelegPositionenAVAsync(since ?? DateTime.Now.AddMonths(-1));
        var reduced = data.Select(avpo => new { avpo.Pcode, avpo.BelegPositionAVGuid, avpo.BelegGuid, avpo.BelegPositionGuid });
        Console.WriteLine(JsonSerializer.Serialize(reduced));
    }

    [Command("get")]
    public async Task GetPos(
        [Argument("pos", Description = "AVPos-GUID or PCode")]
        string? pos)
    {
        Guid.TryParse(pos, out var guid);
        
        var settings = await getSettings();
        AVWebRoutinen client = new(settings);
        if (pos == null && guid == Guid.Empty)
        {
            throw new InvalidOperationException("Please provide pos or pcode");
        }
        var result = guid != Guid.Empty ? 
            await client.GetBelegPositionAVByIdAsync(guid) : 
            (await client.GetBelegPositionAVByPCodeAsync(pos!)).FirstOrDefault();
        Console.WriteLine(JsonSerializer.Serialize(result));
    }
}