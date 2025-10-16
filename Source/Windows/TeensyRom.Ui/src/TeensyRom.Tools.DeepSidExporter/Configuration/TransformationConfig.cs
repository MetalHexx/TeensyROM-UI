namespace TeensyRom.Tools.DeepSidExporter.Configuration;

public class TransformationConfig
{
    public string HvscPathPrefix { get; set; } = "_High Voltage SID Collection";
    public NotableReplacementConfig NotableReplacement { get; set; } = new();
    public string DefaultTagType { get; set; } = "OTHER";
}

public class NotableReplacementConfig
{
    public string From { get; set; } = "Created this web site";
    public string To { get; set; } = "Creator of DeepSID";
}