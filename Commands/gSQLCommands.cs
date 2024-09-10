using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;

public class gSQLCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList()
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        Console.WriteLine(JsonSerializer.Serialize(await client.LadeBestellungenAsync()));
    }

    [Command("get")]
    public async Task GetBeleg(
        [Argument("beleg", Description = "Beleg-GUID")]
        Guid beleg)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        Console.WriteLine(JsonSerializer.Serialize(await client.GetgSQLBelegAsync(beleg)));
    }

    [Command("reset")]
    public async Task Reset(
        [Argument("since", Description = "Reset since")]
        DateTime since)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await client.ResetBestellungenAsync(since);
    }
}