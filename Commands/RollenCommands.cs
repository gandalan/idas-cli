using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using static IdasCli.CliAttributes;

public class RollenCommands : CommandsBase
{
    [CliCommand("listrollen", Description = "List all Rollen")]
    public async Task ListRollen(CommonParameters commonParams)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        await dumpOutput(commonParams, rollen);
    }
}
