using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

namespace IdasCli.Commands;

public static class SpectreCommandAppFactory
{
    public static CommandApp CreateApp(IServiceCollection services)
    {
        using var registrar = new DependencyInjectionRegistrar(services);
        var app = new CommandApp(registrar);
        
        app.Configure(config =>
        {
            config.SetApplicationName("idas");
            config.SetApplicationVersion("0.0.1");

            config.AddBranch<GlobalSettings>("benutzer", benutzer =>
            {
                benutzer.AddCommand<BenutzerLoginCommand>("login");
                benutzer.AddCommand<BenutzerLogoutCommand>("logout");
                benutzer.AddCommand<BenutzerListCommand>("list");
                benutzer.AddCommand<BenutzerGetCommand>("get");
                benutzer.AddCommand<BenutzerPasswordResetCommand>("password-reset");
                benutzer.AddCommand<BenutzerChangePasswordCommand>("change-password");
                benutzer.AddCommand<BenutzerAddRoleCommand>("add-role");
                benutzer.AddCommand<BenutzerRemoveRoleCommand>("remove-role");
                benutzer.AddCommand<BenutzerSetRollenCommand>("set-rollen");
            });

            config.AddBranch<GlobalSettings>("vorgang", vorgang =>
            {
                vorgang.AddCommand<VorgangListCommand>("list");
                vorgang.AddCommand<VorgangGetCommand>("get");
                vorgang.AddCommand<VorgangPutCommand>("put");
                vorgang.AddCommand<VorgangSampleCommand>("sample");
                vorgang.AddCommand<VorgangArchiveCommand>("archive");
                vorgang.AddCommand<VorgangArchiveBulkCommand>("archive-bulk");
                vorgang.AddCommand<VorgangActivateCommand>("activate");
            });

            config.AddBranch<GlobalSettings>("artikel", artikel =>
            {
                artikel.AddCommand<ArtikelListCommand>("list");
                artikel.AddCommand<ArtikelPutCommand>("put");
                artikel.AddCommand<ArtikelSampleCommand>("sample");
            });

            config.AddBranch<GlobalSettings>("av", av =>
            {
                av.AddCommand<AVListCommand>("list");
                av.AddCommand<AVPosCommand>("pos");
            });

            config.AddBranch<GlobalSettings>("kontakt", kontakt =>
            {
                kontakt.AddCommand<KontaktListCommand>("list");
                kontakt.AddCommand<KontaktGetCommand>("get");
                kontakt.AddCommand<KontaktPutCommand>("put");
                kontakt.AddCommand<KontaktSampleCommand>("sample");
            });

            config.AddBranch<GlobalSettings>("serie", serie =>
            {
                serie.AddCommand<SerieListCommand>("list");
                serie.AddCommand<SerieGetCommand>("get");
                serie.AddCommand<SeriePutCommand>("put");
                serie.AddCommand<SerieSampleCommand>("sample");
            });

            config.AddBranch<GlobalSettings>("variante", variante =>
            {
                variante.AddCommand<VarianteListCommand>("list");
                variante.AddCommand<VarianteGetCommand>("get");
                variante.AddCommand<VariantePutCommand>("put");
                variante.AddCommand<VarianteGuidsCommand>("guids");
            });

            config.AddBranch<GlobalSettings>("rolle", rolle =>
            {
                rolle.AddCommand<RollenListCommand>("list");
                rolle.AddCommand<RollenGetCommand>("get");
                rolle.AddCommand<RollenPutCommand>("put");
                rolle.AddCommand<RollenSampleCommand>("sample");
            });

            config.AddBranch<GlobalSettings>("konfigsatz", konfigsatz =>
            {
                konfigsatz.AddCommand<KonfigSatzListCommand>("list");
                konfigsatz.AddCommand<KonfigSatzPutCommand>("put");
            });

            config.AddBranch<GlobalSettings>("uidefinition", uidefinition =>
            {
                uidefinition.AddCommand<UIDefinitionListCommand>("list");
                uidefinition.AddCommand<UIDefinitionGetCommand>("get");
                uidefinition.AddCommand<UIDefinitionPutCommand>("put");
            });

            config.AddBranch<GlobalSettings>("werteliste", werteliste =>
            {
                werteliste.AddCommand<WertelisteListCommand>("list");
                werteliste.AddCommand<WertelisteGetCommand>("get");
                werteliste.AddCommand<WertelistePutCommand>("put");
            });

            config.AddBranch<GlobalSettings>("lagerbestand", lagerbestand =>
            {
                lagerbestand.AddCommand<LagerbestandListCommand>("list");
            });

            config.AddBranch<GlobalSettings>("lagerbuchung", lagerbuchung =>
            {
                lagerbuchung.AddCommand<LagerbuchungListCommand>("list");
                lagerbuchung.AddCommand<LagerbuchungPutCommand>("put");
                lagerbuchung.AddCommand<LagerbuchungSampleCommand>("sample");
            });

            config.AddBranch<GlobalSettings>("gsql", gsql =>
            {
                gsql.AddCommand<GSQLListCommand>("list");
                gsql.AddCommand<GSQLGetCommand>("get");
                gsql.AddCommand<GSQLResetCommand>("reset");
            });

            config.AddBranch<GlobalSettings>("berechtigung", berechtigung =>
            {
                berechtigung.AddCommand<BerechtigungListCommand>("list");
            });

            config.AddBranch<GlobalSettings>("warengruppe", warengruppe =>
            {
                warengruppe.AddCommand<WarengruppeListCommand>("list");
            });

            config.AddBranch<GlobalSettings>("beleg", beleg =>
            {
                beleg.AddCommand<BelegListCommand>("list");
            });

            config.AddCommand<McpServerCommand>("mcp");
        });
        
        return app;
    }
}
