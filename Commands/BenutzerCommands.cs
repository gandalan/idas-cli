using System.Diagnostics;
using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;
using IDAS.Cli.SSO;

public class BenutzerCommands : CommandsBase
{
    [Command("login", Description = "Login using Single Sign-On (SSO)")]
    public async Task Login(
        [Option("appguid", Description = "AppGuid (provided by Gandalan or from .env as IDAS_APP_TOKEN)")] Guid? appGuid,
        [Option("env", Description = "Environment (dev, staging, produktiv) or from .env as IDAS_ENV")] string? env,
        [Option("timeout", Description = "Timeout in seconds for SSO callback")] int timeoutSeconds = 60)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide appGuid either as command line parameter or in the .env file (IDAS_APP_TOKEN).");
            return;
        }

        try
        {
            // Initialize WebApiConfigurations
            await WebApiConfigurations.InitializeAsync(appGuid.Value);
            var settings = WebApiConfigurations.ByName(env);

            // Use the library's SSO login service
            var ssoService = new Gandalan.IDAS.WebApi.Client.SSO.SsoLoginService(settings, timeoutSeconds);

            Console.WriteLine("Starting SSO login flow...");
            Console.WriteLine($"SSO URL: {ssoService.BuildSsoUrl(appGuid.Value)}");

            // Perform SSO login
            var result = await ssoService.LoginAsync(appGuid.Value, msg => SafeLog(msg));

            if (result.Success)
            {
                settings.AuthToken = result.AuthToken;
                WebApiConfigurations.Save(settings);

                // Save token to file for CLI persistence
                await SaveTokenToFileAsync(result.AuthToken);

                Console.WriteLine($"SSO Login successful: User={result.UserName} Mandant={result.MandantName}");
            }
            else
            {
                Console.WriteLine($"SSO Login failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SSO Login failed: {ex.Message}");
        }
    }

    [Command("logout", Description = "Logout and revoke the current session token")]
    public async Task Logout(
        [Option("appguid", Description = "AppGuid (provided by Gandalan or from .env as IDAS_APP_TOKEN)")] Guid? appGuid,
        [Option("env", Description = "Environment (dev, staging, produktiv) or from .env as IDAS_ENV")] string? env)
    {
        try
        {
            await LogoutAsync(env, appGuid);
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

    [Command("list", Description = "Get the list of own users")]
    public async Task List(CommonParameters commonParams)
    {
        var settings = await getSettings();
        var client = new BenutzerWebRoutinen(settings);
        var data = await client.GetBenutzerListeAsync(settings.AuthToken.MandantGuid);
        await dumpOutput(commonParams, data);
    }

    [Command("password-reset", Description = "Reset password for a user by email")]
    public async Task PasswordReset(
        [Argument("email", Description = "User's email address")]
        string email)
    {
        // Get minimal settings without requiring full authentication
        var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide appGuid via IDAS_APP_TOKEN environment variable.");
            return;
        }

        await WebApiConfigurations.InitializeAsync(appGuid);
        var settings = WebApiConfigurations.ByName(env);

        var client = new BenutzerWebRoutinen(settings);
        await client.PasswortResetAsync(email);
        Console.WriteLine($"Password reset email has been sent to {email}");
    }

    [Command("change-password", Description = "Change password for the current user")]
    public async Task ChangePassword(
        [Argument("username", Description = "Username or email")]
        string username,
        [Argument("old-password", Description = "Current password")]
        string oldPassword,
        [Argument("new-password", Description = "New password")]
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
