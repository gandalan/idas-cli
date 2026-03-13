using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class RollenCommands : CommandsBase
{
    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        await dumpOutput(commonParams, rollen);
    }

    public async Task Get(
        CommonParameters commonParams,
        Guid rolleGuid)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        var rolle = rollen.FirstOrDefault(r => r.RolleGuid == rolleGuid);
        await dumpOutput(commonParams, rolle);
    }

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
                    Code = "Vorgang.Lesen"
                }
            }.ToArray()
        };
        await dumpOutput(commonParams, sample);
    }

    public async Task Put(
        string file)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rolle = JsonSerializer.Deserialize<RolleDTO>(await File.ReadAllTextAsync(file));
        await client.SaveAsync(rolle);
        Console.WriteLine($"Rolle '{rolle.Name}' wurde gespeichert.");
    }
}
