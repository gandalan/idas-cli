using System.Diagnostics;
using System.Text.Json;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;
using IDAS.Cli.SSO;

public class BenutzerCommands : CommandsBase
{
    public async Task Login(int timeoutSeconds)
    {
        var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "prod";
        var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APPGUID") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide appGuid via IDAS_APPGUID environment variable or --appguid flag.");
            return;
        }

        try
        {
            // Initialize WebApiConfigurations
            await WebApiConfigurations.InitializeAsync(appGuid);
            var settings = WebApiConfigurations.ByName(env);
            if (settings == null)
            {
                Console.WriteLine($"Environment '{env}' not found.");
                return;
            }

            // Use the library's SSO login service
            var ssoService = new Gandalan.IDAS.WebApi.Client.SSO.SsoLoginService(settings, timeoutSeconds);

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
                    SafeLog($"Could not open browser automatically: {ex.Message}");
                    SafeLog("Please open the SSO URL manually to continue the login.");
                    SafeLog($"SSO URL: {url}");
                    return false;
                }
            }

            var result = await ssoService.LoginAsync(appGuid, msg => SafeLog(msg), OpenBrowser);

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

    public async Task PasswordReset(string email)
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

    public async Task ChangePassword(string username, string oldPassword, string newPassword)
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
