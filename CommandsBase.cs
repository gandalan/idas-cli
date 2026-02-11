using System.Runtime.CompilerServices;
using System.Text.Json;
using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.WebApi.Client;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;

public class CommandsBase
{
    private static readonly HashSet<Guid> _initializedAppTokens = new();
    private static readonly object _initLock = new();
    
    /// <summary>
    /// When true, suppresses all console output (used in MCP server mode)
    /// </summary>
    public static bool IsSilentMode { get; set; } = false;

    protected void SafeLog(string message)
    {
        if (IsSilentMode)
            return;
            
        try
        {
            Console.WriteLine(message);
        }
        catch (ObjectDisposedException)
        {
            // Console writer already closed - ignore in MCP mode
        }
        catch (IOException)
        {
            // Cannot write to closed TextWriter - ignore in MCP mode  
        }
    }

    protected async Task<IWebApiConfig> getSettings(string? user = null, string? password = null, Guid? appGuid = null, string? env = null)
    {
        env = env ?? Environment.GetEnvironmentVariable("IDAS_ENV") ?? "dev";
        appGuid = appGuid ?? Guid.Parse(Environment.GetEnvironmentVariable("IDAS_APP_TOKEN") ?? Guid.Empty.ToString());
        
        lock (_initLock)
        {
            if (!_initializedAppTokens.Contains(appGuid.Value))
            {
                WebApiConfigurations.InitializeAsync(appGuid.Value).Wait();
                _initializedAppTokens.Add(appGuid.Value);
            }
        }
        var settings = WebApiConfigurations.ByName(env);
        
        if (File.Exists("token"))
        {
            var token = await File.ReadAllTextAsync("token");
            settings.AuthToken = JsonSerializer.Deserialize<UserAuthTokenDTO>(token);
            if (settings.AuthToken != null && await new WebRoutinenBase(settings).LoginAsync())
            {
                SafeLog($"Login from stored token successful: User={settings.UserName} Mandant={settings.AuthToken.Mandant.Name}, Environment={settings.FriendlyName}");
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
                    // Always write data output, even in silent mode (needed for MCP)
                    try
                    {
                        Console.WriteLine(output);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Console closed - ignore
                    }
                    catch (IOException)
                    {
                        // Cannot write - ignore
                    }
                }
                break;
    
            default:
                // Always write data output, even in silent mode (needed for MCP)
                try
                {
                    Console.WriteLine(data.ToString());
                }
                catch (ObjectDisposedException)
                {
                    // Console closed - ignore
                }
                catch (IOException)
                {
                    // Cannot write - ignore
                }
                break;
        }
    }
}