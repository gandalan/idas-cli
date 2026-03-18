using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class VarianteListCommand : AsyncCommand<GlobalSettings>
{
    public async Task GetList(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task GetVariante(
        CommonParameters commonParams,
        Guid guid,
        bool includeKonfigs,
        bool includeUIDefs)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid, includeUIDefs, includeKonfigs));
    }

    public async Task PutVariante(
        CommonParameters commonParams,
        string file)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        var variante = JsonSerializer.Deserialize<VarianteDTO>(await File.ReadAllTextAsync(file));
        await client.SaveVarianteAsync(variante);
    }

    public async Task GetAllGuids(CommonParameters commonParams)
    {
        var settings = await getSettings();
        VariantenWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllGuidsAsync());
    }
}
