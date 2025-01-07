using System;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class RollenCommands : CommandsBase
{
    [Command]
    public async Task ListRollen(CommonParameters commonParams)
    {
        var settings = await getSettings();
        RollenWebRoutinen client = new(settings);
        var rollen = await client.GetAllAsync();
        foreach (var rolle in rollen)
        {
            Console.WriteLine($"Rolle: {rolle.Name}");
        }
    }
}
