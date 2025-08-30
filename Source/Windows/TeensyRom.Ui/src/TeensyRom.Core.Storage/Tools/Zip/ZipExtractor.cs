using System.IO.Compression;
using System.Reflection;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage.Tools.Zip
{
    public class ZipExtractor : IZipExtractor
    {
        private string? AssemblyBasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string OutputPath => Path.Combine(AssemblyBasePath!, StorageHelper.Extraction_Path);
        public void ClearOutputDirectory()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
        }

        public ExtractionResult Extract(FileTransferItem zipItem)
        {
            var assetFullPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), zipItem.FullSourcePath);
            var zipFullPath = Path.Combine(assetFullPath);

            if (!File.Exists(zipFullPath)) 
            {
                return new ExtractionResult(zipItem.TargetPath.FileName, []);
            }
            var zipOutputPath = Path.Combine(OutputPath, Guid.NewGuid().ToString(), zipItem.TargetPath.FileName);
            EnsureOutputDirectory(zipOutputPath);
            ZipFile.ExtractToDirectory(zipFullPath, zipOutputPath, true);

            var files = Directory.GetFiles(zipOutputPath, "*", SearchOption.AllDirectories);

            if (files.Length == 0) 
            {
                return new ExtractionResult(zipItem.TargetPath.FileName, []);
            }

            return new ExtractionResult(zipItem.TargetPath.FileName, files.Select(f => new FileInfo(f)).ToList());
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
