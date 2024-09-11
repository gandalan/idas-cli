using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;

public class ArtikelCommands : CommandsBase
{
    [Command("list")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [Command("put")]
    public async Task PutArtikel(
        [Argument("file", Description = "JSON file with Artikel data")]
        string file)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        var artikel = JsonSerializer.Deserialize<KatalogArtikelDTO>(await File.ReadAllTextAsync(file));
        await client.SaveArtikelAsync(artikel);
    }

    [Command("sample", Description = "Create a sample KatalogArtikelDTO")]
    public async Task CreateSample(CommonParameters commonParams)
    {
        await dumpOutput(commonParams, new KatalogArtikelDTO() {
            Art = "KatalogArtikel",
            Bezeichnung = "Testartikel",
            Einheit = "Stk.",
            Preis = 1.99m,
            KatalogArtikelGuid = Guid.NewGuid(),
            KatalogNummer = "99099",
            Freigabe_IBOS = true,
            GueltigAb = DateTime.Now,
            GueltigBis = DateTime.Now.AddYears(1)
        });
    }
}