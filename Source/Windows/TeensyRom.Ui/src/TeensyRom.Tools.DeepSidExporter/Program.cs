using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeensyRom.Tools.DeepSidExporter.Configuration;
using TeensyRom.Tools.DeepSidExporter.Services.Database;
using TeensyRom.Tools.DeepSidExporter.Services.Export;
using TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;
using TeensyRom.Tools.DeepSidExporter.Services.Orchestration;
using TeensyRom.Tools.DeepSidExporter.Services.Transformation;

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("🎵 DeepSID Complete JSON Export - C# Edition");
Console.WriteLine("===========================================");
Console.ResetColor();
Console.WriteLine();

// Build configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

// Build host with dependency injection
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Configuration
        services.Configure<DatabaseConfig>(configuration.GetSection("Database"));
        services.Configure<ExportConfig>(configuration.GetSection("Export"));
        services.Configure<TransformationConfig>(configuration.GetSection("Transformations"));

        // Services
        services.AddSingleton<IProgressReporter, ProgressReporter>();
        services.AddScoped<IDockerStartupService, DockerStartupService>();
        services.AddScoped<IDeepSidDatabaseService, DeepSidDatabaseService>();
        services.AddScoped<IDataTransformationService, DataTransformationService>();
        services.AddScoped<IPathTransformationService, PathTransformationService>();
        services.AddScoped<IUrlBuilderService, UrlBuilderService>();
        services.AddScoped<IJsonExportService, JsonExportService>();
        services.AddScoped<IExportOrchestrationService, ExportOrchestrationService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning); // Changed from Information to Warning
    })
    .Build();

try
{
    // Ensure Docker environment is ready
    var dockerService = host.Services.GetRequiredService<IDockerStartupService>();
    
    if (!await dockerService.EnsureDockerRunningAsync())
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("❌ Docker environment is not ready. Cannot proceed with export.");
        Console.ResetColor();
        return 1;
    }

    // Get the orchestration service and run the export
    var exportService = host.Services.GetRequiredService<IExportOrchestrationService>();
    
    var success = await exportService.ExecuteExportAsync();

    // Stop Docker containers after export completes (success or failure)
    await dockerService.StopDockerAsync();
    
    return success ? 0 : 1;
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ Fatal error: {ex.Message}");
    Console.ResetColor();
    
    // Log full exception details for debugging
    var logger = host.Services.GetService<ILogger<Program>>();
    logger?.LogError(ex, "Application failed with unhandled exception");

    // Ensure Docker is stopped even on exception
    try
    {
        var dockerService = host.Services.GetService<IDockerStartupService>();
        if (dockerService != null)
        {
            await dockerService.StopDockerAsync();
        }
    }
    catch
    {
        // Ignore errors during cleanup
    }
    
    return 1;
}
