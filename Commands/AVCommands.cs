using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class AVCommands : CommandsBase
{
    public async Task GetList(
        CommonParameters commonParams,
        DateTime? since)
    {
        var settings = await getSettings();
        AVWebRoutinen client = new(settings);
        var data = await client.GetAllBelegPositionenAVAsync(since ?? DateTime.Now.AddMonths(-1));
        var reduced = data.Select(avpo => new { avpo.Pcode, avpo.BelegPositionAVGuid, avpo.BelegGuid, avpo.BelegPositionGuid });
        await dumpOutput(commonParams, reduced);
    }

    public async Task GetPos(
        CommonParameters commonParams,
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
            (await client.GetBelegPositionAVByPCodeAsync(pos!)).First();

        await dumpOutput(commonParams, result);
    }
}
