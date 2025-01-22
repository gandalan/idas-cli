using System.Runtime.CompilerServices;
using System.Text.Json;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;

public class CommandsBase
{
    protected async Task<IWebApiConfig> getSettings(string? user = null, string? password = null, Guid? appGuid = null, string? env = null)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());
        
        await WebApiConfigurations.InitializeAsync(appGuid.Value);
        var settings = WebApiConfigurations.ByName(env);
        
        if (File.Exists("token"))
        {
            var token = await File.ReadAllTextAsync("token");
            settings.AuthToken = JsonSerializer.Deserialize<UserAuthTokenDTO>(token);
            if (settings.AuthToken != null && await new WebRoutinenBase(settings).LoginAsync())
            {
                Console.WriteLine($"Login from stored token successful: User={settings.UserName} Mandant={settings.AuthToken.Mandant.Name}, Environment={settings.FriendlyName}");
                settings.UserName = user;
                settings.Passwort = password;
                settings.AppToken = appGuid.Value;
                return settings;
            }
        }

        user = user ?? Environment.GetEnvironmentVariable("IDAS_USER");
        password = password ?? Environment.GetEnvironmentVariable("IDAS_PASSWORD");

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException("Please provide user and password");
        }

        settings.UserName = user;
        settings.Passwort = password;

        var client = new WebRoutinenBase(settings);
        if (await client.LoginAsync())
        {
            settings.AuthToken = client.AuthToken; 
            return settings;
        }

        throw new InvalidOperationException("Login failed");
    }

    protected async Task dumpOutput(CommonParameters commonParameters, object data)
    {
        switch (commonParameters.Format) 
        {
            case "json":
                JsonSerializerOptions options = new() 
                { 
                    WriteIndented = true
                };
                var output = JsonSerializer.Serialize(data, options);                
                if (!string.IsNullOrEmpty(commonParameters.FileName))
                {
                    await File.WriteAllTextAsync(commonParameters.FileName, output);
                } else {
                    Console.WriteLine(output);
                }
                break;
    
            default:
                Console.WriteLine(data.ToString());
                break;
        }
    }
}