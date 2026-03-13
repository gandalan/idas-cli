using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class BerechtigungCommands : CommandsBase
{
    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        BerechtigungWebRoutinen client = new(settings);
        var berechtigungen = await client.GetAllAsync();
        await dumpOutput(commonParams, berechtigungen);
    }
}
