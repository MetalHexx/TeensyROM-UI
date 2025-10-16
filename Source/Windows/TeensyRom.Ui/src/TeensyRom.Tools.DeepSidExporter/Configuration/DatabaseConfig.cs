namespace TeensyRom.Tools.DeepSidExporter.Configuration;

public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    public int CommandTimeout { get; set; } = 300;
}