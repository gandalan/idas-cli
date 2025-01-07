using System;
using Cocona;

public class RollenCommands
{
    private readonly RollenWebRoutinen _rollenWebRoutinen;

    public RollenCommands(RollenWebRoutinen rollenWebRoutinen)
    {
        _rollenWebRoutinen = rollenWebRoutinen;
    }

    public void ListRollen()
    {
        var rollen = _rollenWebRoutinen.GetRollen();
        foreach (var rolle in rollen)
        {
            Console.WriteLine($"Rolle: {rolle.Name}");
        }
    }
}
