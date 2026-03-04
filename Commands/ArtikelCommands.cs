using System.Text.Json;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.DTO;
using static IdasCli.CliAttributes;

public class ArtikelCommands : CommandsBase
{
    [CliCommand("list", Description = "List all articles")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.GetAllAsync());
    }

    [CliCommand("put", Description = "Update an article from JSON file")]
    public async Task PutArtikel(
        [CliArgument(Description = "Path to JSON file")] string file)
    {
        var settings = await getSettings();
        ArtikelWebRoutinen client = new(settings);
        var artikel = JsonSerializer.Deserialize<KatalogArtikelDTO>(await File.ReadAllTextAsync(file));
        await client.SaveArtikelAsync(artikel);
    }

    [CliCommand("sample", Description = "Create a sample article JSON")]
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