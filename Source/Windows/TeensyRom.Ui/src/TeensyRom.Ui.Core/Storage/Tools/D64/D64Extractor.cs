using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeensyRom.Core.Assets;
using TeensyRom.Ui.Core.Common;
using TeensyRom.Ui.Core.Logging;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Storage.Tools.D64Extraction
{
    public class D64Extractor(ILoggingService log) : ID64Extractor
    {
        private readonly ILoggingService _log = log;
        private string? AssemblyBasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string OutputPath => Path.Combine(AssemblyBasePath!, AssetConstants.VicePath, "output");

        public void ClearOutputDirectory()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
        }

        public ExtractionResult Extract(FileTransferItem d64)
        {
            _log.Internal($"***Starting d64 extraction for {d64.Name}***");

            var outputDir = Path.Combine(OutputPath, d64.Name);

            EnsureOutputDirectory(outputDir);

            var prgFileNames = GetPrgFiles(d64.SourcePath);

            var outputFiles = ExtractPrgFiles(d64.SourcePath, prgFileNames, outputDir);

            return new ExtractionResult
            (
                fileName: d64.Name,
                extractedFiles: outputFiles
            );
        }

        private List<FileInfo> ExtractPrgFiles(string d64Path, List<string> prgFiles, string outputDir)
        {
            List<FileInfo> outputFiles = [];

            foreach (var prgFile in prgFiles)
            {
                string sanitizedFileName = SanitizeFileName(prgFile);
                string uniqueFileName = EnsureUniqueFileName(sanitizedFileName, outputFiles);

                var outputFileName = $"{uniqueFileName}.prg";
                var outputFullPath = Path.Combine(outputDir, outputFileName);

                var arguments = $"-attach \"{d64Path}\" -read \"{prgFile}\" \"{outputFullPath}\"";

                _log.Internal($"Extracting ({prgFile}):\r .\\c1541 {arguments}");

                var createFileResult = ExecuteC1541Command(arguments);
                _log.Internal($"C1541 Response:\r {createFileResult}\r");

                if (IsErrorResult(createFileResult))
                {
                    _log.InternalError($"Failed to extract {sanitizedFileName}.prg");
                    continue;
                }
                var fileInfo = new FileInfo(outputFullPath);

                if (fileInfo.Exists)
                {
                    _log.InternalSuccess($"Successfully extracted {sanitizedFileName}.prg");
                    outputFiles.Add(fileInfo);
                    continue;
                }
                _log.InternalError($"Failed to extract {sanitizedFileName}.prg");
            }

            return outputFiles;
        }

        private bool IsErrorResult(string output) =>
                output.Contains("err =", StringComparison.OrdinalIgnoreCase)
                || output.Contains("invalid filename", StringComparison.OrdinalIgnoreCase)
                || output.Contains("file not found", StringComparison.OrdinalIgnoreCase)
                || output.Contains("c11541 timeout", StringComparison.OrdinalIgnoreCase)
                || output.Contains("out of bounds", StringComparison.OrdinalIgnoreCase);

        public List<string> GetPrgFiles(string d64Path)
        {
            var arguments = $"-attach \"{d64Path}\" -list";

            _log.Internal(@$"Finding PRG files: .\c1541.exe {arguments}");

            var listOutput = ExecuteC1541Command(arguments);
            _log.Internal($"C1541 Response:\r {listOutput}\r");

            var prgFiles = ParsePrgFiles(listOutput);

            _log.Internal($"Found {prgFiles.Count} PRG files: ({string.Join(") --- (", prgFiles)})");

            return prgFiles;
        }

        private string ExecuteC1541Command(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AssemblyBasePath!, AssetConstants.ViceC1541Executable),
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            StringBuilder outputBuilder = new();
            StringBuilder errorBuilder = new();

            using (process)
            {
                process.Start();

                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                    _log.InternalError("C11541 Timeout");
                    return "C11541 Timeout";
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                outputBuilder.Append(output);
                errorBuilder.Append(error);
            }

            return $"{outputBuilder}{errorBuilder}";
        }

        private static string SanitizeFileName(string prgFile)
        {
            var sanitizedFileName = Regex.Replace(prgFile, "[^a-zA-Z0-9]", "_");
            return sanitizedFileName;
        }

        private static string EnsureUniqueFileName(string fileName, IEnumerable<FileInfo> files)
        {
            var isDuplicate = files.Any(f => Path.GetFileNameWithoutExtension(f.Name) == fileName);

            if (isDuplicate)
            {
                return $"{fileName}_{Guid.NewGuid().ToString()[..4]}";
            }
            return fileName;
        }

        private List<string> ParsePrgFiles(string listOutput)
        {
            var prgRegex = new Regex("\"([^\"]+)\"\\s+prg(?:<|\\s|$)", RegexOptions.IgnoreCase);

            return prgRegex
                .Matches(listOutput)
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value)
                .ToList();
        }
        private static void EnsureOutputDirectory(string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
        }
    }
}