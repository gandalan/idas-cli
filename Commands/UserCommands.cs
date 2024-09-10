using System.Text.Json;
using Cocona;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;

public class UserCommands 
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
}