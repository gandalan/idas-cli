using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Text;
using Microsoft.Extensions.Logging;

namespace IdasCli.Services;

/// <summary>
/// CSV implementation of output service
/// </summary>
public class CsvOutputService : IOutputService
{
    private readonly ILogger<CsvOutputService> _logger;
    private readonly OutputParameters _commonParameters;

    public CsvOutputService(ILogger<CsvOutputService> logger, OutputParameters commonParameters)
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

        var csvOutput = ConvertToCsv(data, ";");
        
        if (!string.IsNullOrEmpty(_commonParameters.FileName))
        {
            await File.WriteAllTextAsync(_commonParameters.FileName, csvOutput);
        }
        else
        {
            try
            {
                Console.WriteLine(csvOutput);
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
        if (data == null)
            return string.Empty;

        if (data is IEnumerable enumerable && data is not string)
        {
            var items = enumerable.Cast<object>().ToList();

            if (items.Count == 0)
                return string.Empty;

            return ConvertCollectionToCsv(items, separator);
        }

        return ConvertSingleObjectToCsv(data, separator);
    }

    private string ConvertCollectionToCsv(List<object> items, string separator)
    {
        var sb = new StringBuilder();

        var firstItem = items[0];
        var allProperties = firstItem.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        var properties = allProperties
            .Where(p => GetPropertyValue(p, firstItem) != null || !IsComplexCollection(p))
            .ToArray();

        if (properties.Length == 0)
            return string.Empty;

        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(separator, headers));

        foreach (var item in items)
        {
            var values = properties.Select(p => EscapeCsvValue(GetPropertyValue(p, item) ?? string.Empty, separator));
            sb.AppendLine(string.Join(separator, values));
        }

        return sb.ToString();
    }

    private string ConvertSingleObjectToCsv(object item, string separator)
    {
        var sb = new StringBuilder();

        var allProperties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        var properties = allProperties
            .Where(p => GetPropertyValue(p, item) != null || !IsComplexCollection(p))
            .ToArray();

        if (properties.Length == 0)
            return item.ToString() ?? string.Empty;

        var headers = properties.Select(p => p.Name);
        sb.AppendLine(string.Join(separator, headers));

        var values = properties.Select(p => EscapeCsvValue(GetPropertyValue(p, item) ?? string.Empty, separator));
        sb.AppendLine(string.Join(separator, values));

        return sb.ToString();
    }

    private bool IsComplexCollection(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        if (propertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(propertyType))
            return false;

        var elementType = GetCollectionElementType(propertyType);
        return !IsPrimitiveType(elementType);
    }

    private string? GetPropertyValue(PropertyInfo property, object item)
    {
        try
        {
            var value = property.GetValue(item);
            if (value == null)
                return string.Empty;

            if (value is IEnumerable enumerable && value is not string)
            {
                var elementType = GetCollectionElementType(property.PropertyType);

                if (IsPrimitiveType(elementType))
                {
                    return SerializePrimitiveCollection(enumerable);
                }

                return null;
            }

            return value switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"),
                Guid g => g.ToString("D"),
                bool b => b ? "true" : "false",
                _ => value.ToString() ?? string.Empty
            };
        }
        catch
        {
            return string.Empty;
        }
    }

    private bool IsPrimitiveType(Type? type)
    {
        if (type == null)
            return false;

        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType.IsPrimitive)
            return true;

        if (underlyingType == typeof(string) ||
            underlyingType == typeof(Guid) ||
            underlyingType == typeof(DateTime) ||
            underlyingType == typeof(DateTimeOffset) ||
            underlyingType == typeof(decimal) ||
            underlyingType == typeof(TimeSpan))
        {
            return true;
        }

        if (underlyingType.IsEnum)
            return true;

        return false;
    }

    private Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        if (collectionType.IsGenericType)
        {
            var genericArgs = collectionType.GetGenericArguments();
            if (genericArgs.Length > 0)
                return genericArgs[0];
        }

        foreach (var interfaceType in collectionType.GetInterfaces())
        {
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return interfaceType.GetGenericArguments()[0];
            }
        }

        return null;
    }

    private string SerializePrimitiveCollection(IEnumerable enumerable)
    {
        var values = new List<string>();

        foreach (var item in enumerable)
        {
            if (item == null)
            {
                values.Add(string.Empty);
                continue;
            }

            string formattedValue = item switch
            {
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss"),
                Guid g => g.ToString("D"),
                bool b => b ? "true" : "false",
                _ => item.ToString() ?? string.Empty
            };

            values.Add(formattedValue);
        }

        if (values.Count == 0)
            return "[]";

        return "[" + string.Join(",", values.Select(v => EscapeCsvCollectionValue(v))) + "]";
    }

    private string EscapeCsvCollectionValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        value = value.Replace("\\", "\\\\").Replace(",", "\\,").Replace("[", "\\[").Replace("]", "\\]");
        return value;
    }

    private string EscapeCsvValue(string value, string separator)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(separator) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }
}
