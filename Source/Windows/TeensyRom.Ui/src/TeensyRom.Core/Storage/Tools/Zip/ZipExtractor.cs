using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Core.Storage.Tools.Zip
{
    public class ZipExtractor : IZipExtractor
    {
        private string? AssemblyBasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string OutputPath => Path.Combine(AssemblyBasePath!, StorageConstants.Extraction_Path);
        public void ClearOutputDirectory()
        {
            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
        }

        public ExtractionResult Extract(FileTransferItem zipItem)
        {
            var assetFullPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), zipItem.SourcePath);
            var zipFullPath = Path.Combine(assetFullPath);

            if (!File.Exists(zipFullPath)) 
            {
                return new ExtractionResult(zipItem.Name, []);
            }
            var zipOutputPath = Path.Combine(OutputPath, Guid.NewGuid().ToString(), zipItem.Name);
            EnsureOutputDirectory(zipOutputPath);
            ZipFile.ExtractToDirectory(zipFullPath, zipOutputPath, true);

            var files = Directory.GetFiles(zipOutputPath, "*", SearchOption.AllDirectories);

            if (files.Length == 0) 
            {
                return new ExtractionResult(zipItem.Name, []);
            }

            return new ExtractionResult(zipItem.Name, files.Select(f => new FileInfo(f)).ToList());
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
