namespace TeensyRom.Tools.DeepSidExporter.Configuration;

public class ExportConfig
{
    public string OutputDirectory { get; set; } = "export";
    public string OutputFileName { get; set; } = "deepsid_db.json";
    public bool DeleteExistingFile { get; set; } = true;
    public bool ValidateOutput { get; set; } = true;
}