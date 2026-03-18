namespace IdasCli.Services;

/// <summary>
/// Service for output formatting and serialization
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Dumps output in the specified format (JSON or CSV)
    /// </summary>
    Task DumpOutputAsync(object? data);
    
    /// <summary>
    /// Converts data to CSV format
    /// </summary>
    string ConvertToCsv(object? data, string separator = ";");
}
