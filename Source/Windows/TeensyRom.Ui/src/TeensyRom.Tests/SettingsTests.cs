using TeensyRom.Core.Settings;
using TeensyRom.Ui.Features.Settings;
using FluentAssertions;
using TeensyRom.Core.Files;
using Newtonsoft.Json;

namespace TeensyRom.Tests
{
    public class SettingsTests: IDisposable
    {
        private readonly string _settingsFileName = "Settings.json";
        [Fact]
        public void Given_ApplicationStarts_When_SettingsFileDoesntExist_Then_LoadsDefaultSettings()
        {
            //Arrange
            var settingsService = new SettingsService();
            var vm = new SettingsViewModel(settingsService);
            var expectedWatchLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            expectedWatchLocation = Path.Combine(expectedWatchLocation, "Downloads");

            //Assert
            vm.Settings.SidTargetPath.Should().BeEmpty();
            vm.Settings.PrgTargetPath.Should().BeEmpty();
            vm.Settings.CrtTargetPath.Should().BeEmpty();            
            vm.Settings.TargetType.Should().Be(TeensyStorageType.SD);
            vm.Settings.WatchDirectoryLocation.Should().Be($"{expectedWatchLocation}");
        }

        [Fact]
        public void Given_UserSavesSettings_When_WatchFolderNotFound_Then_ReturnsErrorLog()
        {
            //Arrange
            var settingsService = new SettingsService();
            var vm = new SettingsViewModel(settingsService);
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

            var settingsService = new SettingsService();
            var vm = new SettingsViewModel(settingsService);

            var expectedLog = $"The watch directory '{vm.Settings.WatchDirectoryLocation}' was not found.  Please go create it.";

            //Assert
            vm.Logs.Should().Contain(expectedLog);            
        }

        [Fact]
        public void Given_SettingsFileExists_When_SettingsLoaded_Then_ViewContainsCorrectValues()
        {
            //Arrange
            var savedSettings = new TeensySettings()
            {
                SidTargetPath = "/target/sid",
                PrgTargetPath = "/target/prg",
                CrtTargetPath = "/target/crt",
                WatchDirectoryLocation = @"C:\path\to\watch"
            };
            var json = JsonConvert.SerializeObject(savedSettings);
            File.WriteAllText(_settingsFileName, json);

            var settingsService = new SettingsService();
            var vm = new SettingsViewModel(settingsService);

            //Assert
            vm.Settings.SidTargetPath.Should().Be(savedSettings.SidTargetPath);
            vm.Settings.PrgTargetPath.Should().Be(savedSettings.PrgTargetPath);
            vm.Settings.CrtTargetPath.Should().Be(savedSettings.CrtTargetPath);
            vm.Settings.WatchDirectoryLocation.Should().Be(savedSettings.WatchDirectoryLocation);

        }

        [Fact]
        public void Given_UserEntersNewSettings_WhenSaved_SuccessMessageReturned()
        {
            //Arrange
            var settingsService = new SettingsService();
            var vm = new SettingsViewModel(settingsService);
            var expectedLog = "Settings are all valid and saved successfully.";

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
