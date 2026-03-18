using System.Diagnostics;
using System.Text.Json;
using IdasCli.Services;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.BusinessRoutinen;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;

namespace IdasCli.Commands;

public class BenutzerLoginCommand : AsyncCommand<BenutzerLoginCommand.Settings>
{
    public async Task Login(
        int timeoutSeconds)
    {
        _logger = logger;
    }

    public class Settings : GlobalSettings
    {
        [CommandOption("--timeout")]
        public int Timeout { get; set; } = 60;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
            var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

            if (appGuid == Guid.Empty)
            {
                Console.WriteLine("Please provide appGuid via IDAS_APPGUID environment variable or --appguid flag.");
                return 1;
            }

            // Initialize WebApiConfigurations
            await WebApiConfigurations.InitializeAsync(appGuid);
            var authSettings = WebApiConfigurations.ByName(env);
            if (authSettings == null)
            {
                Console.WriteLine($"Environment '{env}' not found.");
                return 1;
            }

            // Use the library's SSO login service
            var ssoService = new Gandalan.IDAS.WebApi.Client.SSO.SsoLoginService(authSettings, settings.Timeout);

            Console.WriteLine("Starting SSO login flow...");

            bool OpenBrowser(string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Could not open browser automatically: {ex.Message}");
                    _logger.LogInformation("Please open the SSO URL manually to continue the login.");
                    _logger.LogInformation($"SSO URL: {url}");
                    return false;
                }
            }

            var result = await ssoService.LoginAsync(appGuid, msg => _logger.LogInformation(msg), OpenBrowser);

            if (result.Success)
            {
                authSettings.AuthToken = result.AuthToken;
                WebApiConfigurations.Save(authSettings);

                // Save token to file for CLI persistence
                await SaveTokenToFileAsync(result.AuthToken);

                Console.WriteLine($"SSO Login successful: User={result.UserName} Mandant={result.MandantName}");
                return 0;
            }
            else
            {
                Console.WriteLine($"SSO Login failed: {result.ErrorMessage}");
                return 1;
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    public async Task Logout()
    {
        try
        {
            await LogoutAsync();
        }
        catch (InvalidOperationException ex)
        {
            SafeLog($"Logout failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            SafeLog($"Logout failed: {ex.Message}");
        }
    }

    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);
        var data = await client.GetBenutzerListeAsync(settings.AuthToken.MandantGuid);
        await dumpOutput(commonParams, data);
    }

    public async Task Get(
        CommonParameters commonParams,
        Guid benutzerGuid)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);
        var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);
        await dumpOutput(commonParams, benutzer);
    }

    public async Task AddRole(
        Guid benutzerGuid,
        Guid rolleGuid)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);

        var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);

        if (benutzer.Rollen == null)
            benutzer.Rollen = new List<RolleDTO>();

        if (!benutzer.Rollen.Any(r => r.RolleGuid == rolleGuid))
        {
            var rollenClient = new RollenWebRoutinen(settings);
            var alleRollen = await rollenClient.GetAllAsync();
            var rolle = alleRollen.FirstOrDefault(r => r.RolleGuid == rolleGuid);

            if (rolle != null)
            {
                benutzer.Rollen.Add(rolle);
                await client.SaveBenutzerAsync(benutzer);
                Console.WriteLine($"Rolle '{rolle.Name}' wurde hinzugefügt.");
            }
            else
            {
                Console.WriteLine($"Rolle mit GUID {rolleGuid} nicht gefunden.");
            }
        }
        else
        {
            Console.WriteLine("Benutzer hat diese Rolle bereits.");
        }
    }

    public async Task RemoveRole(
        Guid benutzerGuid,
        Guid rolleGuid)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);

        var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);

        if (benutzer.Rollen != null && benutzer.Rollen.Any(r => r.RolleGuid == rolleGuid))
        {
            benutzer.Rollen = benutzer.Rollen.Where(r => r.RolleGuid != rolleGuid).ToList();
            await client.SaveBenutzerAsync(benutzer);
            Console.WriteLine("Rolle wurde entfernt.");
        }
        else
        {
            Console.WriteLine("Benutzer hat diese Rolle nicht.");
        }
    }

    public async Task SetRollen(
        Guid benutzerGuid,
        string file)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);

        var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);
        var rollen = JsonSerializer.Deserialize<List<RolleDTO>>(await File.ReadAllTextAsync(file));

        benutzer.Rollen = rollen;
        await client.SaveBenutzerAsync(benutzer);
        Console.WriteLine($"{rollen.Count} Rollen wurden zugewiesen.");
    }

    public async Task PasswordReset(
        string email)
    {
        // Get minimal settings without requiring full authentication
        var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide appGuid via IDAS_APPGUID environment variable.");
            return;
        }

        await WebApiConfigurations.InitializeAsync(appGuid);
        var settings = WebApiConfigurations.ByName(env);

        var client = new BenutzerWebRoutinen(settings);
        await client.PasswortResetAsync(email);
        Console.WriteLine($"Password reset email has been sent to {email}");
    }

    public async Task ChangePassword(
        string username,
        string oldPassword,
        string newPassword)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);

        var passwortAendernData = new PasswortAendernDTO
        {
            Benutzername = username,
            AltesPasswort = oldPassword,
            NeuesPasswort = newPassword
        };

        await client.PasswortAendernAsync(passwortAendernData);
        Console.WriteLine($"Password successfully changed for user {username}");
    }

    private async Task SaveTokenToFileAsync(UserAuthTokenDTO? authToken)
    {
        if (authToken == null)
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        await File.WriteAllTextAsync("token", JsonSerializer.Serialize(authToken, options));
    }
}

public class BenutzerLogoutCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly ILogger<BenutzerLogoutCommand> _logger;

    public BenutzerLogoutCommand(IIdasAuthService authService, ILogger<BenutzerLogoutCommand> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutAsync();
            return 0;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogInformation($"Logout failed: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerListCommand : AsyncCommand<GlobalSettings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public BenutzerListCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GlobalSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);
            var data = await client.GetBenutzerListeAsync(authSettings.AuthToken.MandantGuid);
            await _outputService.DumpOutputAsync(data);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerGetCommand : AsyncCommand<BenutzerGetCommand.Settings>
{
    private readonly IIdasAuthService _authService;
    private readonly IOutputService _outputService;

    public BenutzerGetCommand(IIdasAuthService authService, IOutputService outputService)
    {
        _authService = authService;
        _outputService = outputService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<BENUTZERGUID>")]
        public string BenutzerGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.BenutzerGuid, out var benutzerGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);
            var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);
            await _outputService.DumpOutputAsync(benutzer);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerPasswordResetCommand : AsyncCommand<BenutzerPasswordResetCommand.Settings>
{
    public BenutzerPasswordResetCommand()
    {
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<EMAIL>")]
        public string Email { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            // Get minimal settings without requiring full authentication
            var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
            var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

            if (appGuid == Guid.Empty)
            {
                Console.WriteLine("Please provide appGuid via IDAS_APPGUID environment variable.");
                return 1;
            }

            await WebApiConfigurations.InitializeAsync(appGuid);
            var authSettings = WebApiConfigurations.ByName(env);

            var client = new BenutzerWebRoutinen(authSettings);
            await client.PasswortResetAsync(settings.Email);
            Console.WriteLine($"Password reset email has been sent to {settings.Email}");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerChangePasswordCommand : AsyncCommand<BenutzerChangePasswordCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public BenutzerChangePasswordCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<USERNAME>")]
        public string Username { get; set; } = string.Empty;

        [CommandArgument(1, "<OLDPASSWORD>")]
        public string OldPassword { get; set; } = string.Empty;

        [CommandArgument(2, "<NEWPASSWORD>")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);

            var passwortAendernData = new PasswortAendernDTO
            {
                Benutzername = settings.Username,
                AltesPasswort = settings.OldPassword,
                NeuesPasswort = settings.NewPassword
            };

            await client.PasswortAendernAsync(passwortAendernData);
            Console.WriteLine($"Password successfully changed for user {settings.Username}");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerAddRoleCommand : AsyncCommand<BenutzerAddRoleCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public BenutzerAddRoleCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<BENUTZERGUID>")]
        public string BenutzerGuid { get; set; } = string.Empty;

        [CommandArgument(1, "<ROLLEGUID>")]
        public string RolleGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.BenutzerGuid, out var benutzerGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid Benutzer GUID format[/]");
                return 1;
            }

            if (!Guid.TryParse(settings.RolleGuid, out var rolleGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid Rolle GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);

            var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);

            if (benutzer.Rollen == null)
                benutzer.Rollen = new List<RolleDTO>();

            if (!benutzer.Rollen.Any(r => r.RolleGuid == rolleGuid))
            {
                var rollenClient = new RollenWebRoutinen(authSettings);
                var alleRollen = await rollenClient.GetAllAsync();
                var rolle = alleRollen.FirstOrDefault(r => r.RolleGuid == rolleGuid);

                if (rolle != null)
                {
                    benutzer.Rollen.Add(rolle);
                    await client.SaveBenutzerAsync(benutzer);
                    Console.WriteLine($"Rolle '{rolle.Name}' wurde hinzugefügt.");
                }
                else
                {
                    Console.WriteLine($"Rolle mit GUID {rolleGuid} nicht gefunden.");
                    return 1;
                }
            }
            else
            {
                Console.WriteLine("Benutzer hat diese Rolle bereits.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerRemoveRoleCommand : AsyncCommand<BenutzerRemoveRoleCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public BenutzerRemoveRoleCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<BENUTZERGUID>")]
        public string BenutzerGuid { get; set; } = string.Empty;

        [CommandArgument(1, "<ROLLEGUID>")]
        public string RolleGuid { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.BenutzerGuid, out var benutzerGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid Benutzer GUID format[/]");
                return 1;
            }

            if (!Guid.TryParse(settings.RolleGuid, out var rolleGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid Rolle GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);

            var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);

            if (benutzer.Rollen != null && benutzer.Rollen.Any(r => r.RolleGuid == rolleGuid))
            {
                benutzer.Rollen = benutzer.Rollen.Where(r => r.RolleGuid != rolleGuid).ToList();
                await client.SaveBenutzerAsync(benutzer);
                Console.WriteLine("Rolle wurde entfernt.");
            }
            else
            {
                Console.WriteLine("Benutzer hat diese Rolle nicht.");
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}

public class BenutzerSetRollenCommand : AsyncCommand<BenutzerSetRollenCommand.Settings>
{
    private readonly IIdasAuthService _authService;

    public BenutzerSetRollenCommand(IIdasAuthService authService)
    {
        _authService = authService;
    }

    public class Settings : GlobalSettings
    {
        [CommandArgument(0, "<BENUTZERGUID>")]
        public string BenutzerGuid { get; set; } = string.Empty;

        [CommandArgument(1, "<FILE>")]
        public string File { get; set; } = string.Empty;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!Guid.TryParse(settings.BenutzerGuid, out var benutzerGuid))
            {
                AnsiConsole.MarkupLine("[red]Error: Invalid Benutzer GUID format[/]");
                return 1;
            }

            var authSettings = await _authService.GetSettingsAsync();
            var client = new BenutzerWebRoutinen(authSettings);

            var benutzer = await client.GetBenutzerAsync(benutzerGuid, mitRollenUndRechten: true);
            if (benutzer == null)
            {
                Console.WriteLine($"Benutzer mit GUID {benutzerGuid} nicht gefunden.");
                return 1;
            }

            var rollen = JsonSerializer.Deserialize<List<RolleDTO>>(await File.ReadAllTextAsync(settings.File));
            if (rollen == null)
            {
                Console.WriteLine("Ungültige Rollendatei.");
                return 1;
            }

            benutzer.Rollen = rollen;
            await client.SaveBenutzerAsync(benutzer);
            Console.WriteLine($"{rollen.Count} Rollen wurden zugewiesen.");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }
}
