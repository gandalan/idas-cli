using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class RollenListCommand : AsyncCommand<GlobalSettings>
{
    public async Task List(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
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
