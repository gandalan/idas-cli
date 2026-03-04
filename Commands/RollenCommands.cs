using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class RollenCommands : CommandsBase
{
    [CliCommand("list", Description = "List all Rollen")]
    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        await dumpOutput(commonParams, rollen);
    }

    [CliCommand("get", Description = "Get a Rolle by GUID")]
    public async Task Get(
        CommonParameters commonParams,
        [CliArgument(Description = "Rolle GUID")] Guid rolleGuid)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        var rolle = rollen.FirstOrDefault(r => r.RolleGuid == rolleGuid);
        await dumpOutput(commonParams, rolle);
    }

    [CliCommand("sample", Description = "Create a sample Rolle JSON")]
    public async Task Sample(CommonParameters commonParams)
    {
        var sample = new RolleDTO
        {
            RolleGuid = Guid.NewGuid(),
            Name = "BeispielRolle",
            Beschreibung = "Beschreibung der Rolle",
            Berechtigungen = new List<BerechtigungDTO>
            {
                new BerechtigungDTO 
                { 
                    Code = "Vorgang.Lesen",
                    ErklaerungsText = "Vorgänge lesen",
                    Level = "Mandant"
                }
            }.ToArray()
        };
        await dumpOutput(commonParams, sample);
    }
}
