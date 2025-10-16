using System.Diagnostics;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeensyRom.Tools.DeepSidExporter.Configuration;
using TeensyRom.Tools.DeepSidExporter.Models.Database;
using TeensyRom.Tools.DeepSidExporter.Models.Export;
using TeensyRom.Tools.DeepSidExporter.Services.Database;
using TeensyRom.Tools.DeepSidExporter.Services.Export;
using TeensyRom.Tools.DeepSidExporter.Services.Infrastructure;
using TeensyRom.Tools.DeepSidExporter.Services.Transformation;
using static TeensyRom.Tools.DeepSidExporter.Services.Infrastructure.ConsoleHelper;

namespace TeensyRom.Tools.DeepSidExporter.Services.Orchestration;

/// <summary>
/// Service for orchestrating the complete export process
/// </summary>
public class ExportOrchestrationService : IExportOrchestrationService
{
    private readonly IDeepSidDatabaseService _databaseService;
    private readonly IDataTransformationService _dataTransformationService;
    private readonly IPathTransformationService _pathTransformationService;
    private readonly IUrlBuilderService _urlBuilderService;
    private readonly IJsonExportService _jsonExportService;
    private readonly IProgressReporter _progress;
    private readonly ExportConfig _exportConfig;
    private readonly ILogger<ExportOrchestrationService> _logger;

    public ExportOrchestrationService(
        IDeepSidDatabaseService databaseService,
        IDataTransformationService dataTransformationService,
        IPathTransformationService pathTransformationService,
        IUrlBuilderService urlBuilderService,
        IJsonExportService jsonExportService,
        IProgressReporter progress,
        IOptions<ExportConfig> exportConfig,
        ILogger<ExportOrchestrationService> logger)
    {
        _databaseService = databaseService;
        _dataTransformationService = dataTransformationService;
        _pathTransformationService = pathTransformationService;
        _urlBuilderService = urlBuilderService;
        _jsonExportService = jsonExportService;
        _progress = progress;
        _exportConfig = exportConfig.Value;
        _logger = logger;
    }

    /// <summary>
    /// Execute the complete export process
    /// </summary>
    public async Task<bool> ExecuteExportAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            WriteStepLine("🔄 Step 1: Testing database connectivity...");
            WriteInfo("  Connecting to database... ");
            var connStopwatch = Stopwatch.StartNew();
            if (!await _databaseService.TestConnectionAsync(cancellationToken))
            {
                WriteErrorLine($"❌ Failed ({connStopwatch.ElapsedMilliseconds:N0}ms)");
                return false;
            }
            WriteSuccessLine($"✓ Connected ({connStopwatch.ElapsedMilliseconds:N0}ms)");
            Console.WriteLine();

            // Setup output paths - use Core Assets folder for the export
            var solutionRoot = FindSolutionRoot(Directory.GetCurrentDirectory());
            var outputDir = Path.Combine(solutionRoot, "src", "TeensyRom.Core", "Assets", "Music", "DeepSidDB");
            
            // Ensure output directory exists
            Directory.CreateDirectory(outputDir);
            
            var outputFile = Path.Combine(outputDir, _exportConfig.OutputFileName);
            var zipFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(_exportConfig.OutputFileName) + ".zip");
            
            Console.WriteLine($"📁 Output directory: {outputDir}");
            Console.WriteLine($"📄 Output file: {_exportConfig.OutputFileName}");
            Console.WriteLine();

            // Clean up existing files
            Console.WriteLine("🧹 Cleaning up existing files...");
            if (_exportConfig.DeleteExistingFile)
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                    Console.WriteLine($"  Removed existing JSON: {Path.GetFileName(outputFile)}");
                }
                if (File.Exists(zipFile))
                {
                    File.Delete(zipFile);
                    Console.WriteLine($"  Removed existing ZIP: {Path.GetFileName(zipFile)}");
                }
            }
            Console.WriteLine();

            Console.WriteLine("📊 Step 2: Loading data from database tables...");
            Console.WriteLine("  Loading tables sequentially with progress feedback...");
            Console.WriteLine();

            // Load data sequentially with progress feedback to avoid overwhelming the database
            // This approach is slower but more stable and provides better feedback
            
            Console.Write("  [1/6] Loading HVSC files... ");
            var loadStopwatch = Stopwatch.StartNew();
            var hvscFiles = await _databaseService.LoadHvscFilesAsync(cancellationToken);
            Console.WriteLine($"✓ {hvscFiles.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");
            
            Console.Write("  [2/6] Loading composers... ");
            loadStopwatch.Restart();
            var composers = await _databaseService.LoadComposersAsync(cancellationToken);
            Console.WriteLine($"✓ {composers.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");
            
            Console.Write("  [3/6] Loading composer links... ");
            loadStopwatch.Restart();
            var composerLinks = await _databaseService.LoadComposerLinksAsync(cancellationToken);
            Console.WriteLine($"✓ {composerLinks.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");
            
            Console.Write("  [4/6] Loading tags... ");
            loadStopwatch.Restart();
            var tags = await _databaseService.LoadTagsAsync(cancellationToken);
            Console.WriteLine($"✓ {tags.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");
            
            Console.Write("  [5/6] Loading YouTube videos... ");
            loadStopwatch.Restart();
            var youtubeVideos = await _databaseService.LoadYoutubeVideosAsync(cancellationToken);
            Console.WriteLine($"✓ {youtubeVideos.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");
            
            Console.Write("  [6/6] Loading competitions... ");
            loadStopwatch.Restart();
            var competitions = await _databaseService.LoadCompetitionsAsync(cancellationToken);
            Console.WriteLine($"✓ {competitions.Count:N0} records ({loadStopwatch.ElapsedMilliseconds:N0}ms)");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ✓ All tables loaded successfully");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("🔄 Step 3: Building denormalized JSON structure...");
            Console.Write("  Building lookup dictionaries... ");
            var transformStopwatch = Stopwatch.StartNew();

            // Build lookup dictionaries for efficient processing
            var composerDict = composers.ToDictionary(c => c.Id, c => c);
            var composerLinksDict = composerLinks.GroupBy(cl => cl.ComposersId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var tagsDict = tags.GroupBy(t => t.FileId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var youtubeDict = youtubeVideos.GroupBy(y => y.FileId)
                .ToDictionary(g => g.Key, g => g.ToList());
            var competitionsDict = competitions.GroupBy(c => c.FileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Console.WriteLine($"✓ ({transformStopwatch.ElapsedMilliseconds:N0}ms)");
            Console.WriteLine($"  Processing {hvscFiles.Count:N0} HVSC files...");
            transformStopwatch.Restart();

            var exportFiles = new List<HvscFile>(hvscFiles.Count); // Pre-allocate capacity
            var totalFiles = hvscFiles.Count;
            var processedCount = 0;
            var lastPercent = -1;

            foreach (var hvscFile in hvscFiles)
            {
                processedCount++;
                var percent = (int)Math.Floor((double)processedCount / totalFiles * 100);
                
                // Show progress every 5% instead of 10% for better feedback
                if (percent != lastPercent && percent % 5 == 0)
                {
                    var elapsedTime = transformStopwatch.Elapsed;
                    var rate = processedCount / elapsedTime.TotalSeconds;
                    var remaining = TimeSpan.FromSeconds((totalFiles - processedCount) / rate);
                    _progress.Report($"    {percent,3}% ({processedCount:N0}/{totalFiles:N0}) - {rate:N0} files/sec - ETA: {remaining:mm\\:ss}");
                    lastPercent = percent;
                }

                var exportFile = BuildExportFile(hvscFile, composerDict, composerLinksDict, 
                    tagsDict, youtubeDict, competitionsDict);
                exportFiles.Add(exportFile);
            }

            _progress.Clear(); // Clear the progress line
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ Built {exportFiles.Count:N0} file objects in {transformStopwatch.Elapsed:mm\\:ss}");
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("💾 Step 4: Writing JSON file...");
            Console.Write($"  Serializing {exportFiles.Count:N0} records to {Path.GetFileName(outputFile)}... ");
            var writeStopwatch = Stopwatch.StartNew();
            await _jsonExportService.ExportToJsonAsync(exportFiles, outputFile, cancellationToken);
            var fileInfo = new FileInfo(outputFile);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            Console.WriteLine($"✓ {fileSizeMB:N2} MB written ({writeStopwatch.ElapsedMilliseconds:N0}ms)");
            Console.WriteLine();

            if (_exportConfig.ValidateOutput)
            {
                Console.WriteLine("✅ Step 5: Validating JSON structure...");
                Console.Write("  Parsing and validating JSON... ");
                var validationStopwatch = Stopwatch.StartNew();
                var isValid = await _jsonExportService.ValidateJsonAsync(outputFile, cancellationToken);
                if (!isValid)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Failed ({validationStopwatch.ElapsedMilliseconds:N0}ms)");
                    Console.ResetColor();
                    return false;
                }
                Console.WriteLine($"✓ Valid ({validationStopwatch.ElapsedMilliseconds:N0}ms)");
                Console.WriteLine();
            }

            // Compress the JSON file
            Console.WriteLine("🗜️  Step 6: Compressing JSON file...");
            Console.Write($"  Creating {Path.GetFileName(zipFile)}... ");
            var compressStopwatch = Stopwatch.StartNew();
            
            using (var archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(outputFile, Path.GetFileName(outputFile), CompressionLevel.Optimal);
            }
            
            var zipFileInfo = new FileInfo(zipFile);
            var zipFileSizeMB = zipFileInfo.Length / (1024.0 * 1024.0);
            var compressionRatio = ((1.0 - (zipFileInfo.Length / (double)fileInfo.Length)) * 100);
            Console.WriteLine($"✓ {zipFileSizeMB:N2} MB ({compressionRatio:N1}% compression, {compressStopwatch.ElapsedMilliseconds:N0}ms)");
            
            // Delete the uncompressed JSON file to save space
            Console.Write("  Removing uncompressed JSON... ");
            File.Delete(outputFile);
            Console.WriteLine("✓");
            Console.WriteLine();

            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
            var fileSize = Math.Round(zipFileInfo.Length / (1024.0 * 1024.0), 2);

            // Display summary statistics
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.WriteLine("🎉 Export Complete!");
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.ResetColor();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📊 Summary:");
            Console.ResetColor();
            Console.WriteLine($"  Total files processed: {exportFiles.Count:N0}");
            Console.WriteLine($"  With composers: {exportFiles.Count(f => f.Composer != null):N0} ({exportFiles.Count(f => f.Composer != null) * 100.0 / exportFiles.Count:N1}%)");
            Console.WriteLine($"  With composer links: {exportFiles.Count(f => f.Composer?.Links.Count > 0):N0}");
            Console.WriteLine($"  With tags: {exportFiles.Count(f => f.Tags.Count > 0):N0}");
            Console.WriteLine($"  With YouTube: {exportFiles.Count(f => f.YoutubeVideos.Count > 0):N0}");
            Console.WriteLine($"  With competitions: {exportFiles.Count(f => f.Competitions.Count > 0):N0}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("📁 Output:");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  {zipFile}");
            Console.ResetColor();
            Console.WriteLine($"  Size: {fileSize} MB (compressed)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("⏱️  Performance:");
            Console.ResetColor();
            Console.WriteLine($"  Total time: {elapsed:mm\\:ss\\.fff}");
            Console.WriteLine($"  Average: {exportFiles.Count / elapsed.TotalSeconds:N0} files/second");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✨ Your fully denormalized JSON is ready to use!");
            Console.ResetColor();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export process failed");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Export failed: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    private HvscFile BuildExportFile(
        HvscFileDbModel hvscFile,
        Dictionary<int, ComposerDbModel> composerDict,
        Dictionary<int, List<ComposerLinkDbModel>> composerLinksDict,
        Dictionary<int, List<TagDbModel>> tagsDict,
        Dictionary<int, List<YoutubeDbModel>> youtubeDict,
        Dictionary<int, List<CompetitionDbModel>> competitionsDict)
    {
        // Clean file path
        var cleanFilePath = _pathTransformationService.CleanHvscPath(hvscFile.Fullname);

        // Find matching composer by path
        Composer? composer = null;
        foreach (var (composerId, composerData) in composerDict)
        {
            var composerPath = _pathTransformationService.CleanHvscPath(composerData.Fullname);
            if (_pathTransformationService.IsComposerPathMatch(cleanFilePath, composerPath))
            {
                composer = BuildComposer(composerData, composerLinksDict);
                break;
            }
        }

        // Build tags
        var exportTags = new List<Tag>();
        if (tagsDict.TryGetValue(hvscFile.Id, out var fileTags))
        {
            exportTags.AddRange(fileTags.Select(tag => new Tag
            {
                Name = tag.Name,
                Type = _dataTransformationService.GetDefaultTagType(tag.Type)
            }));
        }

        // Build YouTube videos
        var exportYouTube = new List<YoutubeVideo>();
        if (youtubeDict.TryGetValue(hvscFile.Id, out var fileYouTube))
        {
            exportYouTube.AddRange(fileYouTube.Select(video => new YoutubeVideo
            {
                VideoId = video.VideoId,
                Url = _urlBuilderService.BuildYouTubeUrl(video.VideoId) ?? "",
                Subtune = video.Subtune,  // Already an int
                Channel = string.IsNullOrWhiteSpace(video.Channel) ? "" : video.Channel
            }));
        }

        // Build competitions
        var exportCompetitions = new List<Competition>();
        if (competitionsDict.TryGetValue(hvscFile.Id, out var fileCompetitions))
        {
            exportCompetitions.AddRange(fileCompetitions.Select(comp => new Competition
            {
                Name = comp.Name,
                Place = comp.Place == 0 ? (int?)null : comp.Place  // Convert 0 to null
            }));
        }

        // Build final file object
        var csdbId = hvscFile.CsdbId == 0 ? (int?)null : hvscFile.CsdbId;  // Convert 0 to null
        return new HvscFile
        {
            Id = hvscFile.Id,
            FilePath = cleanFilePath,
            Title = hvscFile.Name,
            Author = hvscFile.Author,
            Copyright = hvscFile.Copyright,
            Composer = composer,
            PlayerType = hvscFile.Player,
            SubtuneLengths = hvscFile.Lengths,
            FileType = hvscFile.Type,
            Version = int.TryParse(hvscFile.Version, out var ver) ? ver : 0,  // Convert string to int
            Playertype = hvscFile.PlayerType,
            Playercompat = hvscFile.PlayerCompat,
            Clockspeed = hvscFile.ClockSpeed,
            Sidmodel = hvscFile.SidModel,
            DataOffset = hvscFile.DataOffset,
            DataSize = hvscFile.DataSize,
            LoadAddr = hvscFile.LoadAddr,
            InitAddr = hvscFile.InitAddr,
            PlayAddr = hvscFile.PlayAddr,
            Subtunes = hvscFile.Subtunes,
            StartSubtune = hvscFile.StartSubtune,
            Hash = hvscFile.Hash,
            Stil = string.IsNullOrWhiteSpace(hvscFile.Stil) ? null : hvscFile.Stil,
            IsNew = hvscFile.New,
            LastUpdated = hvscFile.Updated,
            CsdbType = string.IsNullOrWhiteSpace(hvscFile.CsdbType) ? null : hvscFile.CsdbType,
            CsdbId = csdbId,
            CsdbUrl = _urlBuilderService.BuildCsdbUrl(hvscFile.CsdbType, csdbId),
            Application = string.IsNullOrWhiteSpace(hvscFile.Application) ? "" : hvscFile.Application,
            Gb64 = string.IsNullOrWhiteSpace(hvscFile.Gb64) ? "" : hvscFile.Gb64,
            Tags = exportTags,
            YoutubeVideos = exportYouTube,
            Competitions = exportCompetitions,
            AvgRating = null,
            RatingCount = 0
        };
    }

    private Composer BuildComposer(
        ComposerDbModel composerData,
        Dictionary<int, List<ComposerLinkDbModel>> composerLinksDict)
    {
        // Build composer links
        var links = new List<ComposerLink>();
        if (composerLinksDict.TryGetValue(composerData.Id, out var composerLinks))
        {
            links.AddRange(composerLinks.Select(link => new ComposerLink
            {
                Name = link.Name,
                Url = link.Url
            }));
        }

        // Build CSDB info
        var csdbId = composerData.CsdbId == 0 ? (int?)null : composerData.CsdbId;  // Convert 0 to null
        CsdbInfo? csdb = null;
        if (csdbId.HasValue && !string.IsNullOrWhiteSpace(composerData.CsdbType))
        {
            csdb = new CsdbInfo
            {
                Id = csdbId.Value,
                Type = composerData.CsdbType,
                Url = _urlBuilderService.BuildCsdbUrl(composerData.CsdbType, csdbId) ?? ""
            };
        }

        return new Composer
        {
            Id = composerData.Id,
            Path = _pathTransformationService.CleanHvscPath(composerData.Fullname),
            Name = composerData.Name,
            Shortname = string.IsNullOrWhiteSpace(composerData.Shortname) ? null : composerData.Shortname,
            Handles = _dataTransformationService.ParseHandles(composerData.Handles),
            Shorthandle = string.IsNullOrWhiteSpace(composerData.Shorthandle) ? null : composerData.Shorthandle,
            ActiveYear = composerData.Active == 0 ? null : composerData.Active.ToString(),  // Convert int year to string
            Born = _dataTransformationService.ParseNullableDate(composerData.Born),
            Died = _dataTransformationService.ParseNullableDate(composerData.Died),
            Country = string.IsNullOrWhiteSpace(composerData.Country) ? null : composerData.Country,
            Csdb = csdb,
            ImageSource = string.IsNullOrWhiteSpace(composerData.ImageSource) ? null : composerData.ImageSource,
            Notable = _dataTransformationService.ProcessNotableField(composerData.Notable),
            Employment = string.IsNullOrWhiteSpace(composerData.Employment) ? null : composerData.Employment,
            Affiliation = string.IsNullOrWhiteSpace(composerData.Affiliation) ? null : composerData.Affiliation,
            Brand = string.IsNullOrWhiteSpace(composerData.Brand) ? null : composerData.Brand,
            BrandDark = string.IsNullOrWhiteSpace(composerData.BrandDark) ? null : composerData.BrandDark,
            Links = links
        };
    }

    /// <summary>
    /// Find the solution root by looking for the .sln file
    /// </summary>
    private static string FindSolutionRoot(string startPath)
    {
        var currentPath = startPath;
        
        while (currentPath != null)
        {
            // Look for .sln file in current directory
            if (Directory.GetFiles(currentPath, "*.sln").Length > 0)
            {
                return currentPath;
            }
            
            // Move up one directory
            var parent = Directory.GetParent(currentPath);
            currentPath = parent?.FullName;
        }
        
        // If not found, fall back to a relative path from typical project structure
        // Assumes we're in: src/TeensyRom.Tools.DeepSidExporter/bin/Debug/net9.0
        // Need to go up to the solution root
        var binDebugPath = Path.Combine(startPath, "..", "..", "..", "..", "..");
        return Path.GetFullPath(binDebugPath);
    }
}