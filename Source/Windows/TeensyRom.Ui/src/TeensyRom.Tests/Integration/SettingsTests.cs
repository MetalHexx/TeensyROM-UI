﻿using TeensyRom.Ui.Features.Settings;
using FluentAssertions;
using Newtonsoft.Json;
using System.Windows.Threading;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Logging;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Core.Settings;

namespace TeensyRom.Tests.Integration
{
    public class SettingsTests : IDisposable
    {
        private ILoggingService _logService = new LoggingService();
        private ISnackbarService _snackbar = new SnackbarService(Dispatcher.CurrentDispatcher);
        private readonly string _settingsFileName = "Settings.json";

        [Fact]
        public void Given_ApplicationStarts_When_SettingsFileDoesntExist_Then_LoadsDefaultSettings()
        {
            //Arrange
            var logService = new LoggingService();
            var settingsService = new SettingsService(logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, logService);
            var expectedWatchLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            expectedWatchLocation = Path.Combine(expectedWatchLocation, "Downloads");
            Thread.Sleep(1000);

            //Assert
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Sid).TargetPath.Should().Be("sid");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Prg).TargetPath.Should().Be("prg");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Crt).TargetPath.Should().Be("crt");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Hex).TargetPath.Should().Be("hex");
            vm.Settings.TargetType.Should().Be(TeensyStorageType.SD);
            vm.Settings.AutoFileCopyEnabled.Should().BeFalse();
            vm.Settings.WatchDirectoryLocation.Should().Be($"{expectedWatchLocation}");
        }

        [Fact]
        public void Given_UserSavesSettings_When_WatchFolderNotFound_Then_ReturnsErrorLog()
        {
            //Arrange
            var settingsService = new SettingsService(_logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, _logService);
            vm.Settings.WatchDirectoryLocation = @"C:\some_nonexistant_location";
            var expectedLog = $"The watch directory '{vm.Settings.WatchDirectoryLocation}' was not found.  Please go create it.";

            //Act
            vm.SaveSettingsCommand.Execute().Subscribe();

            //Assert
            vm.Logs.Should().Contain(expectedLog);
        }

        [Fact]
        public void Given_WatchDirectoryInvalid_When_ApplicationLoads_Then_ReturnsErrorLog()
        {
            //Arrange
            var savedSettings = new TeensySettings()
            {
                WatchDirectoryLocation = @"C:\some_nonexistant_location"
            };
            var json = JsonConvert.SerializeObject(savedSettings);
            File.WriteAllText(_settingsFileName, json);
            string logs = string.Empty;

            _logService.Logs.Subscribe(log =>
            {
                logs += log;
            });

            var settingsService = new SettingsService(_logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, _logService);

            var expectedLog = $"The watch directory '{vm.Settings.WatchDirectoryLocation}' was not found.  Please go create it.";

            //Assert
            logs.Should().Contain(expectedLog);
        }

        [Fact]
        public void Given_SettingsFileExists_When_SettingsLoaded_Then_ViewContainsCorrectValues()
        {
            //Arrange
            var savedSettings = new TeensySettings()
            {
                AutoFileCopyEnabled = true,
                FileTargets = new List<TeensyTarget>
                {
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Sid,
                        DisplayName = "SID",
                        Extension = ".sid",
                        TargetPath = "sid-test"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Prg,
                        DisplayName = "PRG",
                        Extension = ".prg",
                        TargetPath = "prg-test"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Crt,
                        DisplayName = "CRT",
                        Extension = ".crt",
                        TargetPath = "crt-test"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Hex,
                        DisplayName = "HEX",
                        Extension = ".hex",
                        TargetPath = "hex-test"
                    }
                }
            };
            var json = JsonConvert.SerializeObject(savedSettings);
            File.WriteAllText(_settingsFileName, json);

            var settingsService = new SettingsService(_logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, _logService);

            //Assert
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Sid).TargetPath.Should().Be("sid-test");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Prg).TargetPath.Should().Be("prg-test");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Crt).TargetPath.Should().Be("crt-test");
            vm.Settings.FileTargets.First(t => t.Type == TeensyFileType.Hex).TargetPath.Should().Be("hex-test");
            vm.Settings.FileTargets.Should().HaveCount(4);
            vm.Settings.AutoFileCopyEnabled.Should().BeTrue();
            vm.Settings.WatchDirectoryLocation.Should().Be(savedSettings.WatchDirectoryLocation);
        }

        [Fact]
        public void Given_UserEntersNewSettings_WhenSaved_SuccessMessageReturned()
        {
            //Arrange
            var settingsService = new SettingsService(_logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, _logService);
            var expectedLog = "Settings saved successfully.";

            //Act
            vm.SaveSettingsCommand.Execute().Subscribe();

            //Assert
            vm.Logs.Should().Contain(expectedLog);
        }

        public void Dispose()
        {
            if (File.Exists(_settingsFileName))
            {
                File.Delete("Settings.json");
            }
        }
    }
}