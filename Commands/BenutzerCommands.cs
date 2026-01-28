using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;

public class BenutzerCommands : CommandsBase
{
    [Command("login")]
    public async Task Login(
        [Option("user", Description = "User name")] string? user,
        [Option("password", Description = "Password")] string? password, 
        [Option("appguid", Description = "AppGuid (provided by Gandalan)")] Guid? appGuid,
        [Option("env", Description = "Environment (dev, staging, produktiv)")] string? env)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());
        user = user ?? Environment.GetEnvironmentVariable("IDAS_USER");
        password = password ?? Environment.GetEnvironmentVariable("IDAS_PASSWORD");

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) || appGuid == Guid.Empty)
        {
            Console.WriteLine("Please provide user, password and appGuid either as command line parameters or in the .env file.");
            return;
        }

        await WebApiConfigurations.InitializeAsync(appGuid.Value);
        var settings = WebApiConfigurations.ByName(env);
        settings.UserName = user;
        settings.Passwort = password;

        var client = new WebRoutinenBase(settings);
        if (await client.LoginAsync())
        {
            settings.AuthToken = client.AuthToken; 
            Console.WriteLine($"Login successful: User={settings.UserName} Mandant={client.AuthToken.Mandant.Name}, Environment={settings.FriendlyName}");
            JsonSerializerOptions options = new() 
            { 
                WriteIndented = true
            };
            await File.WriteAllTextAsync("token", JsonSerializer.Serialize(client.AuthToken, options));
            //await File.WriteAllTextAsync(dump, JsonSerializer.Serialize(client.AuthToken, options));
            //Process.Start("notepad.exe", dump);
        } 
        else 
        {
            Console.WriteLine("Login failed");
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