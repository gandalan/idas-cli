using System;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class RollenCommands
{
    private readonly RollenWebRoutinen _rollenWebRoutinen;

    public RollenCommands(RollenWebRoutinen rollenWebRoutinen)
    {
        _rollenWebRoutinen = rollenWebRoutinen;
    }

    public void ListRollen()
    {
        var rollen = _rollenWebRoutinen.GetAllAsync();
        foreach (var rolle in rollen)
        {
            Console.WriteLine($"Rolle: {rolle.Name}");
        }
    }
}
