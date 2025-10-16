namespace TeensyRom.Tools.DeepSidExporter.Services.Transformation;

/// <summary>
/// Service for data cleaning and transformation operations
/// </summary>
public interface IDataTransformationService
{
    /// <summary>
    /// Parse handles string into list, removing HTML tags and splitting on commas
    /// </summary>
    List<string> ParseHandles(string? handlesRaw);

    /// <summary>
    /// Process notable field with special replacements
    /// </summary>
    string? ProcessNotableField(string? notable);

    /// <summary>
    /// Parse nullable integer from string, handling NULL and "0" values
    /// </summary>
    int? ParseNullableInt(string? value);

    /// <summary>
    /// Parse nullable date from string, handling NULL and "0000-00-00" values
    /// </summary>
    string? ParseNullableDate(string? value);

    /// <summary>
    /// Clean NULL database values to proper nulls
    /// </summary>
    string? CleanNullValue(string? value);

    /// <summary>
    /// Get default tag type for empty values
    /// </summary>
    string GetDefaultTagType(string? tagType);
}