using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class UIDefinitionListCommand : AsyncCommand<GlobalSettings>
{
    public async Task GetList(CommonParameters commonParams)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public async Task GetUIDefinition(
        CommonParameters commonParams,
        Guid guid)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAsync(guid));
    }

    public async Task PutUIDefinition(
        CommonParameters commonParams,
        string file)
    {
        var settings = await getSettings();
        UIWebRoutinen client = new(settings);
        var uiDefinition = JsonSerializer.Deserialize<UIDefinitionDTO>(await File.ReadAllTextAsync(file));
        await client.SaveUIDefinitionAsync(uiDefinition);
    }
}
