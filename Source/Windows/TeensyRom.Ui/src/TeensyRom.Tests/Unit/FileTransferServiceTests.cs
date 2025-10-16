using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;
using TeensyRom.Core.Storage.Tools.D64Extraction;
using TeensyRom.Core.Storage.Tools.Zip;
using TeensyRom.Core.ValueObjects;
using TeensyRom.Ui.Services.Logging;
using NSubstitute;
using MediatR;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Common;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Abstractions;
using FluentAssertions;
using TeensyRom.Core.Games;
using TeensyRom.Core.Music;

namespace TeensyRom.Tests.Unit
{
    public class FileTransferServiceTests : IDisposable
    {
        private readonly string _testZipPath;
        private readonly string _outputDirectory;
        private readonly string _baseTargetPath;
        private readonly FileTransferItem _zipFileItem;
        private readonly FileTransferItem _testFileItem;
        private readonly FileTransferItem _sidFileItem;
        private readonly FileTransferItem _gameFileItem;
        private readonly IFileTransferService _fileTransferService;
        private readonly ICachedStorageService _mockStorage = Substitute.For<ICachedStorageService>();
        private readonly ILoggingService _mockLogger = Substitute.For<ILoggingService>();
        private readonly IMediator _mockMediator = Substitute.For<IMediator>();
        private readonly IAlertService _mockAlert = Substitute.For<IAlertService>();
        private readonly ISidMetadataService _mockSidMetadata = Substitute.For<ISidMetadataService>();
        private readonly IGameMetadataService _mockGameMetadata = Substitute.For<IGameMetadataService>();

        public FileTransferServiceTests()
        {
            _outputDirectory = Path.Combine(Path.GetTempPath(), "ZipExtractorTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_outputDirectory);

            var testFileName = "test.zip";
            _baseTargetPath = "/target-dir/";
            _testZipPath = Path.Combine(_outputDirectory, testFileName);

            CreateTestZipFile();

            _zipFileItem = new FileTransferItem(
                new FileInfo(_testZipPath),
                new FilePath($"{_baseTargetPath}{testFileName}"),
                TeensyStorageType.SD);

            var testFilePath = Path.Combine(_outputDirectory, "testfile.txt");
            File.WriteAllText(testFilePath, "Test content");
            _testFileItem = new FileTransferItem(
                new FileInfo(testFilePath),
                new FilePath($"{_baseTargetPath}testfile.txt"),
                TeensyStorageType.SD);

            var sidFilePath = Path.Combine(_outputDirectory, "testsong.sid");
            File.WriteAllText(sidFilePath, "SID file content");
            _sidFileItem = new FileTransferItem(
                new FileInfo(sidFilePath),
                new FilePath($"{_baseTargetPath}testsong.sid"),
                TeensyStorageType.SD);

            var gameFilePath = Path.Combine(_outputDirectory, "testgame.prg");
            File.WriteAllText(gameFilePath, "PRG file content");
            _gameFileItem = new FileTransferItem(
                new FileInfo(gameFilePath),
                new FilePath($"{_baseTargetPath}testgame.prg"),
                TeensyStorageType.SD);

            var mockD64Extractor = Substitute.For<ID64Extractor>();
            mockD64Extractor.Extract(Arg.Any<FileTransferItem>())
                .Returns(new ExtractionResult(_zipFileItem.TargetPath.FileName, []));

            _mockSidMetadata.EnrichSong(Arg.Any<SongItem>()).Returns(x => x.Arg<SongItem>());
            _mockGameMetadata.EnrichGame(Arg.Any<GameItem>()).Returns(x => x.Arg<GameItem>());

            _fileTransferService = new FileTransferService
            (
                new ZipExtractor(),
                mockD64Extractor,
                _mockMediator,
                _mockStorage,
                _mockSidMetadata,
                _mockGameMetadata,
                _mockAlert,
                _mockLogger
            );
        }

        public void Dispose()
        {
            if (File.Exists(_testZipPath))
            {
                File.Delete(_testZipPath);
            }
            if (Directory.Exists(_outputDirectory))
            {
                Directory.Delete(_outputDirectory, true);
            }
        }

        [Fact]
        public async Task Should_ProcessAndMapFiles_When_SendingFiles()
        {
            var fileList = new List<FileTransferItem>() { _testFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(new SaveFilesResult { SuccessfulFiles = fileList, FailedFiles = [] });

            await _fileTransferService.Send(fileList);

            await _mockMediator.Received(1).Send(
                Arg.Is<SaveFilesCommand>(cmd => cmd.Files.Count == 1),
                Arg.Any<CancellationToken>()
            );
            
            _mockStorage.Received(1).SaveFiles(Arg.Any<IEnumerable<FileItem>>());
            
            _mockAlert.Received(1).Publish(Arg.Is<string>(msg => msg == "1 files were saved to the TR."));
        }

        [Fact]
        public async Task Should_EnrichSidMetadata_When_SendingSidFiles()
        {
            var fileList = new List<FileTransferItem>() { _sidFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(new SaveFilesResult { SuccessfulFiles = fileList, FailedFiles = [] });

            await _fileTransferService.Send(fileList);

            _mockSidMetadata.Received(1).EnrichSong(Arg.Any<SongItem>());
            
            _mockStorage.Received(1).SaveFiles(Arg.Any<IEnumerable<FileItem>>());
        }

        [Fact]
        public async Task Should_EnrichGameMetadata_When_SendingGameFiles()
        {
            var fileList = new List<FileTransferItem>() { _gameFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(new SaveFilesResult { SuccessfulFiles = fileList, FailedFiles = [] });

            await _fileTransferService.Send(fileList);

            _mockGameMetadata.Received(1).EnrichGame(Arg.Any<GameItem>());
            
            _mockStorage.Received(1).SaveFiles(Arg.Any<IEnumerable<FileItem>>());
        }

        [Fact]
        public async Task Should_PreserveZipDirectoryStructure_When_ExtractingZipFile()
        {
            var extractedFiles = new List<FileTransferItem>();
            var savedFileItems = new List<FileItem>();
            
            _mockMediator.Send(Arg.Do<SaveFilesCommand>(cmd => extractedFiles.AddRange(cmd.Files)), Arg.Any<CancellationToken>())
                .Returns(x => {
                    var files = x.Arg<SaveFilesCommand>().Files;
                    return new SaveFilesResult { SuccessfulFiles = files.ToList(), FailedFiles = [] };
                });
                
            _mockStorage.When(x => x.SaveFiles(Arg.Any<IEnumerable<FileItem>>()))
                .Do(x => savedFileItems.AddRange(x.Arg<IEnumerable<FileItem>>()));

            await _fileTransferService.Send([_zipFileItem]);

            extractedFiles.Should().NotBeEmpty();
            extractedFiles.Count.Should().BeGreaterThan(1);
            
            var expectedPaths = new[]
            {
                $"{_baseTargetPath}file1.txt",
                $"{_baseTargetPath}file2.prg",
                $"{_baseTargetPath}file3.crt",
                $"{_baseTargetPath}nested/nestedfile1.sid",
                $"{_baseTargetPath}nested/deeper/deepnestedfile.crt"
            };
            
            extractedFiles.Select(f => f.TargetPath.Value).Should().BeEquivalentTo(expectedPaths);
            
            _mockStorage.Received(1).SaveFiles(Arg.Any<IEnumerable<FileItem>>());
            
            _mockSidMetadata.Received().EnrichSong(Arg.Any<SongItem>());
            _mockGameMetadata.Received().EnrichGame(Arg.Any<GameItem>());
        }

        [Fact]
        public async Task Should_Retry_Failed_File_Transfers()
        {
            var fileList = new List<FileTransferItem>() { _testFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(
                    new SaveFilesResult { FailedFiles = fileList, SuccessfulFiles = [] },
                    new SaveFilesResult { SuccessfulFiles = fileList, FailedFiles = [] }
                );

            await _fileTransferService.Send(fileList);

            await _mockMediator.Received(2).Send(
                Arg.Any<SaveFilesCommand>(),
                Arg.Any<CancellationToken>()
            );
            
            _mockStorage.Received(1).SaveFiles(Arg.Any<IEnumerable<FileItem>>());
            
            _mockAlert.Received(1).Publish(Arg.Is<string>(msg => msg == "1 files were saved to the TR."));
            
            _mockAlert.DidNotReceive().Publish(Arg.Is<string>(msg => msg.Contains("had an error being copied")));
        }

        [Fact]
        public async Task Should_Log_Retry_Messages()
        {
            var fileList = new List<FileTransferItem>() { _testFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(
                    new SaveFilesResult { FailedFiles = fileList, SuccessfulFiles = [] },
                    new SaveFilesResult { SuccessfulFiles = fileList, FailedFiles = [] }
                );

            await _fileTransferService.Send(fileList);

            _mockLogger.Received(1).InternalError(Arg.Is<string>(msg => msg.Contains("Retrying 1 previously failed file(s)")));
                
            await _mockMediator.Received(1).Send(Arg.Any<ResetCommand>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Should_Give_Up_After_Three_Failed_Attempts()
        {
            var fileList = new List<FileTransferItem>() { _testFileItem };
            
            _mockMediator.Send(Arg.Any<SaveFilesCommand>(), Arg.Any<CancellationToken>())
                .Returns(new SaveFilesResult { FailedFiles = fileList, SuccessfulFiles = [] });

            await _fileTransferService.Send(fileList);

            await _mockMediator.Received(4).Send(
                Arg.Any<SaveFilesCommand>(),
                Arg.Any<CancellationToken>()
            );
            
            _mockLogger.Received(3).InternalError(Arg.Is<string>(msg => msg.Contains("Retrying")));
            _mockLogger.Received(1).InternalError(Arg.Is<string>(msg => msg.Contains("Failed to transfer file after")));
            
            _mockAlert.Received(1).Publish(Arg.Is<string>(msg => msg.Contains("1 files had an error being copied")));
            
            _mockStorage.DidNotReceive().SaveFiles(Arg.Any<IEnumerable<FileItem>>());
        }

        private void CreateTestZipFile()
        {
            var tempDir = Path.Combine(_outputDirectory, "temp_for_zip");
            Directory.CreateDirectory(tempDir);

            try
            {
                File.WriteAllText(Path.Combine(tempDir, "file1.txt"), "Test content 1");
                File.WriteAllText(Path.Combine(tempDir, "file2.prg"), "Test content 2");
                File.WriteAllText(Path.Combine(tempDir, "file3.crt"), "Test content 3");

                var nestedDir = Path.Combine(tempDir, "nested");
                Directory.CreateDirectory(nestedDir);
                File.WriteAllText(Path.Combine(nestedDir, "nestedfile1.sid"), "Nested content 1");

                var deepNestedDir = Path.Combine(nestedDir, "deeper");
                Directory.CreateDirectory(deepNestedDir);
                File.WriteAllText(Path.Combine(deepNestedDir, "deepnestedfile.crt"), "Deep nested content");

                if (File.Exists(_testZipPath))
                {
                    File.Delete(_testZipPath);
                }

                ZipFile.CreateFromDirectory(tempDir, _testZipPath);
            }
            finally
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }
    }
}
