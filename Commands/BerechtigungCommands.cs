using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class BerechtigungCommands : CommandsBase
{
    [CliCommand("list", Description = "List all available Berechtigungen (permissions)")]
    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        BerechtigungWebRoutinen client = new(settings);
        var berechtigungen = await client.GetAllAsync();
        await dumpOutput(commonParams, berechtigungen);
    }
}
