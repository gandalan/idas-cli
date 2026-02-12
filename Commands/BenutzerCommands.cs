using System.Diagnostics;
using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;
using IDAS.Cli.SSO;

public class BenutzerCommands : CommandsBase
{
    [Command("login")]
    public async Task Login(
        [Option("appguid", Description = "AppGuid (provided by Gandalan)")] Guid? appGuid,
        [Option("env", Description = "Environment (dev, staging, produktiv)")] string? env)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());

        if (appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide appGuid either as command line parameter or via IDAS_APP_TOKEN environment variable.");
            return;
        }

        await WebApiConfigurations.InitializeAsync(appGuid.Value);
        var settings = WebApiConfigurations.ByName(env);

        using var server = new SsoCallbackServer();
        await server.StartAsync();

        var redirectUrl = Uri.EscapeDataString(server.CallbackUrl + "?token=%token%");
        var idasUri = new Uri(settings.IDASUrl.TrimEnd('/'));
        var baseUrl = $"{idasUri.Scheme}://{idasUri.Host}";
        if (!idasUri.IsDefaultPort)
            baseUrl += $":{idasUri.Port}";
        var ssoUrl = $"{baseUrl}/SSO?a={appGuid}&r={redirectUrl}";
        
        Process.Start(new ProcessStartInfo(ssoUrl) { UseShellExecute = true });
        Console.WriteLine("Browser geöffnet. Bitte im Browser einloggen...");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        string token;
        try
        {
            token = await server.WaitForTokenAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Login timeout - no token received within 120 seconds.");
            return;
        }
        
        // Create minimal auth token with the received token
        settings.AuthToken = new UserAuthTokenDTO 
        { 
            Token = Guid.Parse(token),
            AppToken = appGuid.Value
        };

        var client = new WebRoutinenBase(settings);
        client.IgnoreOnErrorOccured = true;
        
        try
        {
            // Use RefreshTokenAsync to get the full UserAuthTokenDTO from the server
            var refreshedToken = await client.RefreshTokenAsync(settings.AuthToken.Token);
            
            if (refreshedToken != null)
            {
                settings.AuthToken = refreshedToken;
                Console.WriteLine($"Login successful: User={settings.AuthToken.Benutzer?.Benutzername} Mandant={settings.AuthToken.Mandant?.Name}, Environment={settings.FriendlyName}");
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true
                };
                await File.WriteAllTextAsync("token", JsonSerializer.Serialize(settings.AuthToken, options));
            }
            else
            {
                Console.WriteLine("Login failed - could not refresh token");
                Console.WriteLine("The SSO token might not be compatible with the API login.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"Inner: {ex.InnerException.Message}");
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
        var env = Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        var appGuid = Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());
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
}
