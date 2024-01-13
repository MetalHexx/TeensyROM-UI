using TeensyRom.Ui.Features.Settings;
using FluentAssertions;
using Newtonsoft.Json;
using System.Windows.Threading;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Commands.Behaviors;
using TeensyRom.Ui.Services;

namespace TeensyRom.Tests.Integration
{
    public class SettingsTests : IDisposable
    {
        private ILoggingService _logService = new LoggingService();
        private ICommandErrorService _commandErrorService = new CommandErrorService();
        private IAlertService _snackbar;
        private readonly string _settingsFileName = "Settings.json";

        [Fact]
        public void Given_ApplicationStarts_When_SettingsFileDoesntExist_Then_LoadsDefaultSettings()
        {
            //Arrange
            var logService = new LoggingService();
            var settingsService = new SettingsService(logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, logService);
            _snackbar = new AlertService(Dispatcher.CurrentDispatcher, _commandErrorService);
            var expectedWatchLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            expectedWatchLocation = Path.Combine(expectedWatchLocation, "Downloads");
            Thread.Sleep(1000);

            //Assert
            vm.Settings.GetFileTypePath(TeensyFileType.Sid).Should().Be("libraries/music");
            vm.Settings.GetFileTypePath(TeensyFileType.Prg).Should().Be("libraries/programs");
            vm.Settings.GetFileTypePath(TeensyFileType.Crt).Should().Be("libraries/programs");
            vm.Settings.GetFileTypePath(TeensyFileType.Hex).Should().Be("libraries/hex");

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
                        LibraryType = TeensyLibraryType.Music,
                        DisplayName = "SID",
                        Extension = ".sid"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Prg,
                        LibraryType = TeensyLibraryType.Programs,
                        DisplayName = "PRG",
                        Extension = ".prg"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Crt,
                        LibraryType = TeensyLibraryType.Programs,
                        DisplayName = "CRT",
                        Extension = ".crt"
                    },
                    new TeensyTarget
                    {
                        Type = TeensyFileType.Hex,
                        LibraryType = TeensyLibraryType.Hex,
                        DisplayName = "HEX",
                        Extension = ".hex"
                    }
                },
                Libraries =
                [
                    new TeensyLibrary
                    {
                        Type = TeensyLibraryType.Music,
                        DisplayName = "Music",
                        Path = "test/libraries/music"
                    },
                    new TeensyLibrary
                    {
                        Type = TeensyLibraryType.Programs,
                        DisplayName = "Programs",
                        Path = "test/libraries/programs"
                    },
                    new TeensyLibrary
                    {
                        Type = TeensyLibraryType.Hex,
                        DisplayName = "Hex",
                        Path = "test/libraries/hex"
                    }
                ]
            };
            var json = JsonConvert.SerializeObject(savedSettings);
            File.WriteAllText(_settingsFileName, json);

            var settingsService = new SettingsService(_logService);
            var vm = new SettingsViewModel(settingsService, _snackbar, _logService);

            //Assert
            vm.Settings.GetFileTypePath(TeensyFileType.Sid).Should().Be("test/libraries/music");
            vm.Settings.GetFileTypePath(TeensyFileType.Prg).Should().Be("test/libraries/programs");
            vm.Settings.GetFileTypePath(TeensyFileType.Crt).Should().Be("test/libraries/programs");
            vm.Settings.GetFileTypePath(TeensyFileType.Hex).Should().Be("test/libraries/hex");

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
