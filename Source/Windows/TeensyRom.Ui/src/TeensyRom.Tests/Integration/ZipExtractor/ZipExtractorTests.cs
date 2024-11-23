using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Storage.Entities;
using TeensyRom.Ui.Core.Storage.Services;
using TeensyRom.Ui.Core.Storage.Tools.Zip;

namespace TeensyRom.Tests.Integration.ZipExtractorTests
{
    public class ZipExtractorTests : IDisposable
    {
        private string? AssemblyBasePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private string TestFilePath => Path.Combine(AssemblyBasePath!, "Integration\\ZipExtractor\\test.zip");
        private ZipExtractor _extractor = new();

        public ZipExtractorTests()
        {
            _extractor.ClearOutputDirectory();
        }

        [Fact]
        public void Extract_WhenZipFileExists_ExtractsFiles()
        {
            // Arrange
            var fileToExtract = new FileTransferItem
            (
               sourcePath: Path.Combine(TestFilePath),
               targetPath: "",
               targetStorage: TeensyStorageType.USB
            );
            List<string> expectedFiles = ["the-ghost-a.d64", "the-ghost-b.d64"];

            // Act
            var result = _extractor.Extract(fileToExtract);

            // Assert
            result.ExtractedFiles.Select(f => f.Name).Should().BeEquivalentTo(expectedFiles);            
        }

        public void Dispose()
        {
            _extractor.ClearOutputDirectory();
        }
    }
}
