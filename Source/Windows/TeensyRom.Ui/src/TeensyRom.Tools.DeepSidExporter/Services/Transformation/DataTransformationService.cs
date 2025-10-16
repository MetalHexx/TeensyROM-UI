using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using TeensyRom.Tools.DeepSidExporter.Configuration;

namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for data cleaning and transformation operations
/// </summary>
public class DataTransformationService : IDataTransformationService
{
    private readonly TransformationConfig _config;
    private static readonly Regex HtmlTagRegex = new(@"</?del>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public DataTransformationService(IOptions<TransformationConfig> config)
    {
        _config = config.Value;
    }

    /// <summary>
    /// Parse handles string into list, removing HTML tags and splitting on commas
    /// </summary>
    public List<string> ParseHandles(string? handlesRaw)
    {
        if (string.IsNullOrWhiteSpace(handlesRaw) || handlesRaw == "NULL")
            return new List<string>();

        return handlesRaw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(h => h.Trim())
            .Select(h => HtmlTagRegex.Replace(h, "").Trim())
            .Where(h => !string.IsNullOrWhiteSpace(h))
            .ToList();
    }

    /// <summary>
    /// Process notable field with special replacements
    /// </summary>
    public string? ProcessNotableField(string? notable)
    {
        if (string.IsNullOrWhiteSpace(notable) || notable == "NULL")
            return null;

        return notable == _config.NotableReplacement.From 
            ? _config.NotableReplacement.To 
            : notable;
    }

    /// <summary>
    /// Parse nullable integer from string, handling NULL and "0" values
    /// </summary>
    public int? ParseNullableInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "NULL" || value == "0")
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parse nullable date from string, handling NULL and "0000-00-00" values
    /// </summary>
    public string? ParseNullableDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "NULL" || value == "0000-00-00")
            return null;

        return value;
    }

    /// <summary>
    /// Clean NULL database values to proper nulls
    /// </summary>
    public string? CleanNullValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value == "NULL" ? null : value;
    }

    /// <summary>
    /// Get default tag type for empty values
    /// </summary>
    public string GetDefaultTagType(string? tagType)
    {
        return string.IsNullOrWhiteSpace(tagType) ? _config.DefaultTagType : tagType;
    }
}