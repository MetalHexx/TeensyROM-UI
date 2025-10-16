using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

using static TeensyRom.Tools.DeepSidExporter.Services.Infrastructure.ConsoleHelper;

namespace TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;

/// <summary>
/// Service for managing Docker container startup and SQL table extraction
/// </summary>
public interface IDockerStartupService
{
    Task<bool> EnsureDockerRunningAsync(CancellationToken cancellationToken = default);
    Task<bool> StopDockerAsync(CancellationToken cancellationToken = default);
}

public class DockerStartupService : IDockerStartupService
{
    private readonly ILogger<DockerStartupService> _logger;

    public DockerStartupService(ILogger<DockerStartupService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EnsureDockerRunningAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            WriteStepLine("🐳 Checking Docker environment...");
            Console.WriteLine();

            // Step 1: Extract SQL tables if needed
            if (!await ExtractSqlTablesAsync(cancellationToken))
            {
                return false;
            }

            // Step 2: Check if Docker is installed and running
            if (!await IsDockerAvailableAsync(cancellationToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Docker is not available. Please install Docker Desktop and start it.");
                
                return false;
            }

            // Step 3: Check if containers are already running
            var containersRunning = await AreContainersRunningAsync(cancellationToken);
            
            if (containersRunning)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✓ Docker containers are already running");
                
                Console.WriteLine();
                return true;
            }

            // Step 4: Start Docker Compose
            Console.WriteLine("📦 Starting Docker containers...");
            if (!await StartDockerComposeAsync(cancellationToken))
            {
                return false;
            }

            // Step 5: Wait for database to be ready
            Console.WriteLine("⏳ Waiting for database to be ready...");
            if (!await WaitForDatabaseAsync(cancellationToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Database did not become ready in time");
                
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Docker environment is ready");
            
            Console.WriteLine();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring Docker is running");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            
            return false;
        }
    }

    private async Task<bool> ExtractSqlTablesAsync(CancellationToken cancellationToken)
    {
        var projectDir = Directory.GetCurrentDirectory();
        var sqlDir = Path.Combine(projectDir, "sql");
        var zipFile = Path.Combine(sqlDir, "DeepSID_Database.zip");
        const string downloadUrl = "https://chordian.net/files/deepsid/DeepSID_Database.zip";

        // Create sql directory if it doesn't exist
        Directory.CreateDirectory(sqlDir);

        // Step 1: Download the latest database
        WriteInfo("📥 Downloading latest DeepSID database... ");
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(5); // Large file, needs time
            
            var downloadStopwatch = Stopwatch.StartNew();
            var response = await httpClient.GetAsync(downloadUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var zipBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            await File.WriteAllBytesAsync(zipFile, zipBytes, cancellationToken);
            
            var fileSizeMB = zipBytes.Length / (1024.0 * 1024.0);
            WriteSuccessLine($"✓ Downloaded {fileSizeMB:N2} MB ({downloadStopwatch.ElapsedMilliseconds:N0}ms)");
        }
        catch (Exception ex)
        {
            WriteErrorLine($"❌ Failed: {ex.Message}");
            
            _logger.LogError(ex, "Failed to download database");
            
            // Check if we have an existing zip file to fall back on
            if (!File.Exists(zipFile))
            {
                WriteErrorLine("No existing database file found. Cannot proceed.");
                return false;
            }
            
            WriteWarningLine("⚠️  Using existing database file");
            
        }

        // Step 2: Extract SQL files from zip
        try
        {
            WriteInfo("�� Extracting SQL tables... ");
            
            // Delete existing SQL files to ensure clean extraction
            var existingSqlFiles = Directory.GetFiles(sqlDir, "*.sql");
            foreach (var file in existingSqlFiles)
            {
                File.Delete(file);
            }
            
            await Task.Run(() => 
            {
                ZipFile.ExtractToDirectory(zipFile, sqlDir, true);
            }, cancellationToken);

            var extractedFiles = Directory.GetFiles(sqlDir, "*.sql");
            WriteSuccessLine($"✓ Extracted {extractedFiles.Length} SQL files");
            
            return true;
        }
        catch (Exception ex)
        {
            WriteErrorLine($"❌ Failed: {ex.Message}");
            
            _logger.LogError(ex, "Failed to extract SQL tables");
            return false;
        }
    }

    private async Task<bool> IsDockerAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            await process.WaitForExitAsync(cancellationToken);
            
            if (process.ExitCode == 0)
            {
                WriteSuccessLine("✓ Docker is installed and running");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Docker is not available");
            return false;
        }
    }

    private async Task<bool> AreContainersRunningAsync(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "ps --filter name=deepsid-mysql --filter status=running --quiet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return !string.IsNullOrWhiteSpace(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking container status");
            return false;
        }
    }

    private async Task<bool> StartDockerComposeAsync(CancellationToken cancellationToken)
    {
        try
        {
            var projectDir = Directory.GetCurrentDirectory();
            
            var psi = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "up -d",
                WorkingDirectory = projectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to start docker-compose");
                
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ docker-compose failed: {error}");
                
                return false;
            }

            Console.WriteLine("✓ Docker containers started");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting docker-compose");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error: {ex.Message}");
            
            return false;
        }
    }

    private async Task<bool> WaitForDatabaseAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 30;
        const int delaySeconds = 2;

        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "exec deepsid-mysql mysqladmin ping -h localhost -u root -pdeepsid_root_pass",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(psi);
                if (process != null)
                {
                    await process.WaitForExitAsync(cancellationToken);
                    
                    if (process.ExitCode == 0)
                    {
                        Console.WriteLine($"✓ Database is ready (attempt {i + 1}/{maxAttempts})");
                        // Give it a bit more time for the init scripts to complete
                        await Task.Delay(3000, cancellationToken);
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore errors during health check attempts
            }

            if (i < maxAttempts - 1)
            {
                Console.Write(".");
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        return false;
    }

    public async Task<bool> StopDockerAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine("🛑 Stopping Docker containers...");

            // Check if containers are running
            var containersRunning = await AreContainersRunningAsync(cancellationToken);
            
            if (!containersRunning)
            {
                Console.WriteLine("✓ Docker containers are already stopped");
                return true;
            }

            var projectDir = Directory.GetCurrentDirectory();
            
            var psi = new ProcessStartInfo
            {
                FileName = "docker-compose",
                Arguments = "down",
                WorkingDirectory = projectDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to stop docker-compose");
                
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ docker-compose down failed: {error}");
                
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Docker containers stopped successfully");
            
            Console.WriteLine();
            
            // Clean up extracted SQL files (but keep the zip)
            await CleanupSqlFilesAsync(cancellationToken);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping docker-compose");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error stopping containers: {ex.Message}");
            
            return false;
        }
    }

    private Task CleanupSqlFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var projectDir = Directory.GetCurrentDirectory();
            var sqlDir = Path.Combine(projectDir, "sql");

            if (!Directory.Exists(sqlDir))
            {
                return Task.CompletedTask;
            }

            WriteInfoLine("🧹 Cleaning up SQL files...");
            
            var sqlFiles = Directory.GetFiles(sqlDir, "*.sql");
            
            if (sqlFiles.Length == 0)
            {
                WriteInfoLine("  No SQL files to clean up");
                return Task.CompletedTask;
            }

            foreach (var sqlFile in sqlFiles)
            {
                try
                {
                    File.Delete(sqlFile);
                    _logger.LogDebug("Deleted SQL file: {SqlFile}", sqlFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete SQL file: {SqlFile}", sqlFile);
                }
            }

            WriteSuccessLine($"✓ Cleaned up {sqlFiles.Length} SQL file(s)");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up SQL files");
            WriteWarningLine($"⚠️  Could not clean up SQL files: {ex.Message}");
            Console.WriteLine();
        }
        
        return Task.CompletedTask;
    }
}
