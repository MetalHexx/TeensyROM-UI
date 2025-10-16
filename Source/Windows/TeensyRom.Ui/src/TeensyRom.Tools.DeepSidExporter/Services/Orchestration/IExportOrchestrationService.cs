namespace TeensyRom.Tools.DeepSidExporter.Services.Orchestration;

/// <summary>
/// Service for orchestrating the complete export process
/// </summary>
public interface IExportOrchestrationService
{
    /// <summary>
    /// Execute the complete export process
    /// </summary>
    Task<bool> ExecuteExportAsync(CancellationToken cancellationToken = default);
}