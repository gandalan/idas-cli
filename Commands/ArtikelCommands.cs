using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class ArtikelCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList()
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        Console.WriteLine(JsonSerializer.Serialize(await client.GetAllAsync()));
    }

    /*[Command("get")]
    public async Task GetArtikel(
        [Argument("kontakt", Description = "Kontakt-GUID")]
        Guid kontakt)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        Console.WriteLine(JsonSerializer.Serialize(await client.(kontakt)));
    }*/

    [Command("put")]
    public async Task PutArtikel(
        [Argument("file", Description = "JSON file with Artikel data")]
        string file)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        var artikel = JsonSerializer.Deserialize<KatalogArtikelDTO>(await File.ReadAllTextAsync(file));
        Console.WriteLine(JsonSerializer.Serialize(await client.SaveArtikelAsync(artikel)));
    }
}