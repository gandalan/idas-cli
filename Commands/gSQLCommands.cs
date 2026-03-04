using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using static IdasCli.CliAttributes;

public class gSQLCommands : CommandsBase
{
    [CliCommand("list", Description = "List all gSQL Bestellungen")]
    public async Task GetList(CommonParameters commonParams)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await dumpOutput(commonParams, await client.LadeBestellungenAsync());
    }

    [CliCommand("get", Description = "Get gSQL data for a Beleg")]
    public async Task GetBeleg(
        CommonParameters commonParams,
        [CliArgument(Description = "Beleg GUID")] Guid beleg)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        var paramsOverride = new CommonParameters { Format = "gsql", FileName = commonParams.FileName };
        await dumpOutput(paramsOverride, await client.GetgSQLBelegAsync(beleg));
    }

    [CliCommand("reset", Description = "Reset gSQL Bestellungen since date")]
    public async Task Reset(
        [CliArgument(Description = "Reset since date")] DateTime since)
    {
        var settings = await getSettings();
        IBOS1ImportRoutinen client = new(settings);
        await client.ResetBestellungenAsync(since);
    }
}