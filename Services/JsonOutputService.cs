using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace IdasCli.Services;

/// <summary>
/// JSON implementation of output service
/// </summary>
public class JsonOutputService : IOutputService
{
    private readonly ILogger<JsonOutputService> _logger;
    private readonly OutputParameters _commonParameters;

    public JsonOutputService(ILogger<JsonOutputService> logger, OutputParameters commonParameters)
    {
        _logger = logger;
        _commonParameters = commonParameters;
    }

    public async Task DumpOutputAsync(object? data)
    {
        if (data == null)
        {
            _logger.LogInformation("No data to output.");
            return;
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var output = JsonSerializer.Serialize(data, options);
        
        if (!string.IsNullOrEmpty(_commonParameters.FileName))
        {
            await File.WriteAllTextAsync(_commonParameters.FileName, output);
        }
        else
        {
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
    }

    public string ConvertToCsv(object? data, string separator = ";")
    {
        throw new NotSupportedException("JSON output service does not support CSV conversion");
    }
}
