﻿using FluentAssertions;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class FileTransferTests : IDisposable
    {        
        private readonly TeensyFixture _fixture;
        public FileTransferTests()
        {
            _fixture = new TeensyFixture();
        }

        [Fact]
        public void Given_WatcherDisabled_When_FileCopied_Then_FileNotTransferred()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.sid";
            var savedText = $"File transfer complete!";

            _fixture.Settings.AutoFileCopyEnabled = false;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().NotContain(savedText);
        }

        [Fact]
        public void Given_WatcherEnabled_When_Disabled_And_Reenabled_And_FileCopied_Then_FileSuccessfullyTransferred()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: SD";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act            
            _fixture.SettingsViewModel.Settings.AutoFileCopyEnabled = false;
            _fixture.SettingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);
            _fixture.SettingsViewModel.Settings.AutoFileCopyEnabled = true;
            _fixture.SettingsViewModel.SaveSettingsCommand.Execute().Subscribe();

            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_BadFilePath_When_FileSaved_Then_ReturnsError()
        {
            //Arrange
            var errorText = $"Failed to ensure directory";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = "/$^*&#)@--bad/$^*&#)@--path/";
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(errorText);
        }

        [Fact]
        public void Given_EmptyRootFilePath_And_EmptyFileTargetPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            _fixture.SettingsViewModel.Settings.TargetRootPath = string.Empty;

            foreach (var target in _fixture.SettingsViewModel.Settings.FileTargets)
            {
                target.TargetPath = string.Empty;
            }
            _fixture.SettingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_NonExistantFileTargetPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Initialize();

            //Act            
            foreach (var target in _fixture.SettingsViewModel.Settings.FileTargets)
            {
                target.TargetPath = $"{Guid.NewGuid().ToString().Substring(0, 7)}-new-path";
            }
            _fixture.SettingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_EmptyRootFilePath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            _fixture.SettingsViewModel.Settings.TargetRootPath = string.Empty;

            _fixture.SettingsViewModel.SaveSettingsCommand.Execute().Subscribe();
            Thread.Sleep(500);

            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_RootAsRoothPath_When_FileSaved_Then_ReturnsSuccess()
        {
            //Arrange
            var savedText = $"File transfer complete!"; ;

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = "/";
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_SidSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: SD";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_PrgSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.prg";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Prg";
            var storageType = $"Storage Type: SD";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.prg", "Test prg");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_CrtSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.crt";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Crt";
            var storageType = $"Storage Type: SD";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act            
            File.WriteAllText($"{_fixture.FullSourceTestPath}.crt", "Test crt");
            Thread.Sleep(2000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_HexSaved_ToSD_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.hex";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Hex";
            var storageType = $"Storage Type: SD";

            _fixture.Settings.TargetType = TeensyStorageType.SD;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.hex", "Test hex");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_SidSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.sid";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Sid";
            var storageType = $"Storage Type: USB";

            _fixture.Settings.TargetType = TeensyStorageType.USB;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.sid", "Test sid");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_PrgSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.prg";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Prg";
            var storageType = $"Storage Type: USB";

            _fixture.Settings.TargetType = TeensyStorageType.USB;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.prg", "Test prg");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_CrtSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.crt";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Crt";
            var storageType = $"Storage Type: USB";

            _fixture.Settings.TargetType = TeensyStorageType.USB;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.crt", "Test crt");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        [Fact]
        public void Given_WatcherDetectsNewFile_When_HexSaved_ToUSB_Then_ReturnsSuccess()
        {
            //Arrange
            var fileDetectedText = @$"File detected: {_fixture.Settings.WatchDirectoryLocation}\{_fixture.TestFileName}.hex";
            var initiatedText = $"Initiating file transfer handshake";
            var savedText = $"File transfer complete!";
            var expectedType = $"Type: Hex";
            var storageType = $"Storage Type: USB";

            _fixture.Settings.TargetType = TeensyStorageType.USB;
            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path; ;
            _fixture.Settings.AutoFileCopyEnabled = true;
            _fixture.Initialize();

            //Act
            File.WriteAllText($"{_fixture.FullSourceTestPath}.hex", "Test hex");
            Thread.Sleep(1000);

            //Assert
            _fixture.FileTransferViewModel.Logs.Should().Contain(fileDetectedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(initiatedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(savedText);
            _fixture.FileTransferViewModel.Logs.Should().Contain(expectedType);
            _fixture.FileTransferViewModel.Logs.Should().Contain(storageType);
        }

        public void Dispose()
        {
            _fixture.Dispose();

            if (File.Exists(_fixture.FullSourceTestPath))
            {
                File.Delete(_fixture.FullSourceTestPath);
            }

            if (File.Exists(_fixture.SettingsFileName))
            {
                File.Delete(_fixture.SettingsFileName);
            }
        }
    }
}