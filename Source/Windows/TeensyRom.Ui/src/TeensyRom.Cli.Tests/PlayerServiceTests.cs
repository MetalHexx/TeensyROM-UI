using NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using TeensyRom.Cli.Core.Storage.Entities;
using MediatR;
using TeensyRom.Cli.Core.Storage.Services;
using TeensyRom.Cli.Core.Progress;
using TeensyRom.Cli.Core.Serial.State;
using TeensyRom.Cli.Core.Settings;
using System.Reactive.Linq;
using TeensyRom.Cli.Core.Commands.File.LaunchFile;
using TeensyRom.Cli.Core.Player;
using System.Reactive.Subjects;
using Unit = System.Reactive.Unit;
using TeensyRom.Cli.Services;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using System;
using TeensyRom.Cli.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Tests
{
    public class PlayerServiceTests
    {
        private ICachedStorageService _storageService;
        private IProgressTimer _progressTimer;
        private IMediator _mediator;
        private ISettingsService _settingsService;
        private ISerialStateContext _serialContext;
        private ILaunchHistory _launchHistory;
        private const string _sidPath = "/music/MUSIC/1.sid";

        private IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

        public PlayerServiceTests()
        {
            _storageService = _fixture.Freeze<ICachedStorageService>();
            _progressTimer = _fixture.Freeze<IProgressTimer>();
            _mediator = _fixture.Freeze<IMediator>();
            _mediator.Send(Arg.Any<LaunchFileCommand>()).Returns(new LaunchFileResult { IsSuccess = true });
            _serialContext = _fixture.Freeze<ISerialStateContext>();

            _settingsService = _fixture.Freeze<ISettingsService>();
            var settings = new TeensySettings();
            settings.InitializeDefaults();
            _settingsService.GetSettings().Returns(s => settings);

            _launchHistory = new LaunchHistory();
            _fixture.Inject(_launchHistory);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SDStorage_Then_SettingsCorrect()
        {
            //Arrange
            SetupSettingsWithStorage(TeensyStorageType.SD);

            var expectedSettings = new PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = PlayState.Stopped,
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.All,
                ScopePath = "/",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_USBStorage_Then_SettingsCorrect()
        {
            //Arrange
            SetupSettingsWithStorage(TeensyStorageType.USB);

            var expectedSettings = new PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.USB,
                PlayState = PlayState.Stopped,
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.All,
                ScopePath = "/",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_AllFilter_Then_SettingsCorrect()
        {
            //Arrange
            SetupSettingsWithFilter(TeensyFilterType.All);

            var expectedSettings = new PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = PlayState.Stopped,
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.All,
                ScopePath = "/",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_GameFilter_Then_SettingsCorrect()
        {
            //Arrange
            SetupSettingsWithFilter(TeensyFilterType.Games);

            var expectedSettings = new PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = PlayState.Stopped,
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.Games,
                ScopePath = "/",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetScope_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                ScopePath = "/music",
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryScope("/images");
            playerService.SetDirectoryScope("/music");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetSearchMode_ThenSettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                PlayMode = PlayMode.Search,
                SearchQuery = "this is a query",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetDirectoryMode_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                PlayMode = PlayMode.CurrentDirectory,
                ScopePath = "/music",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            playerService.SetDirectoryMode("/music");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetRandom_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                PlayMode = PlayMode.Random,
                ScopePath = "/music",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            playerService.SetRandomMode("/music");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetFilter_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                PlayMode = PlayMode.Random,
                ScopePath = "/music",
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            playerService.SetRandomMode("/music");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetStreamTime_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                PlayState = PlayState.Playing,
                PlayMode = PlayMode.Random,
                PlayTimer = TimeSpan.FromSeconds(3),
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetStreamTime(TimeSpan.FromSeconds(1));
            playerService.SetStreamTime(expectedSettings.PlayTimer);
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetSidTimer_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new PlayerState
            {
                SidTimer = SidTimer.TimerOverride
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSidTimer(SidTimer.SongLength);
            playerService.SetSidTimer(SidTimer.TimerOverride);
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }


        [Fact]
        public async Task When_SetSidTimerOverride_PlayTimerReset()
        {
            //Arrange
            SetupStorageService(CreateFile<SongItem>("/music/sid1.sid"));

            //Act
            var playerService = _fixture.Create<PlayerService>();

            await playerService.LaunchFromDirectory("/music/sid1.sid");
            playerService.SetSidTimer(SidTimer.TimerOverride);
            playerService.SetStreamTime(TimeSpan.FromDays(1));

            _progressTimer.Received(1).StartNewTimer(TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task Given_SidTimeSongLength_When_SetStreamTime_Then_PlayTimerNotReset()
        {
            //Arrange
            SetupStorageService(CreateFile<SongItem>("/music/sid1.sid"));

            //Act
            var playerService = _fixture.Create<PlayerService>();

            playerService.SetSidTimer(SidTimer.SongLength);
            await playerService.LaunchFromDirectory("/music/sid1.sid");
            playerService.SetStreamTime(TimeSpan.FromDays(1));
            _progressTimer.Received(0).StartNewTimer(TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task When_FileLaunched_Then_Emit() 
        {
            //Arrange
            var playerService = _fixture.Create<PlayerService>();
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            //Act
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            playerService.FileLaunched.Subscribe(tcs.SetResult);
            
            await playerService.LaunchFromDirectory("/music/sid1.sid");

            var actualFile = await tcs.Task;

            // Assert
            actualFile.Should().BeEquivalentTo(expectedFile);
        }

        [Theory]
        [InlineData(TeensyStorageType.USB)]
        [InlineData(TeensyStorageType.SD)]
        public async Task Given_FileDoesNotExist_When_LaunchedRequested_Then_SettingsAreCorrect(TeensyStorageType storageType)
        {
            //Arrange
            PlayerState expectedSettings = new();
            expectedSettings.CurrentItem = null;
            expectedSettings.StorageType = TeensyStorageType.SD;
            expectedSettings.PlayState = PlayState.Stopped;

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public async Task Given_FileDoesNotExist_When_LaunchedRequested_Then_FileDoesNotLaunch()
        {
            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");

            //Assert
            await _mediator.DidNotReceive().Send(Arg.Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_FileDoesNotExist_When_LaunchedRequested_Then_TimerDoesNotStart()
        {
            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");

            //Assert
            _progressTimer.DidNotReceive().StartNewTimer(Arg.Any<TimeSpan>());
        }


        [Theory]
        [InlineData(TeensyStorageType.USB)]
        [InlineData(TeensyStorageType.SD)]
        public async Task Given_DirectoryDoesNotExist_When_Launched_Then_SettingsAreCorrect(TeensyStorageType storageType)
        {
            var existingSong = CreateFile<SongItem>("/");
            existingSong.Path = "/music/MUSIC/1.sid";
            SeedStorageDirectory([]);

            //Arrange
            PlayerState expectedSettings = new();
            expectedSettings.CurrentItem = null;
            expectedSettings.StorageType = TeensyStorageType.SD;
            expectedSettings.PlayState = PlayState.Stopped;

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public async Task Given_DirectoryDoesNotExist_When_Launched_Then_FileDoesNotLaunch()
        {
            //Arrange
            var existingSong = CreateFile<SongItem>("/");
            existingSong.Path = "/music/MUSIC/1.sid";
            SeedStorageDirectory([]);

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            await _mediator.DidNotReceive().Send(Arg.Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryDoesNotExist_When_Launched_Then_TimerDoesNotStart()
        {
            //Arrange
            var existingSong = CreateFile<SongItem>("/music/MUSIC/1.sid");
            SeedStorageDirectory([]);

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            _progressTimer.DidNotReceive().StartNewTimer(Arg.Any<TimeSpan>());
        }

        private void SeedStorageDirectory(List<IFileItem> items)
        {
            _storageService.GetDirectory(Arg.Any<string>()).Returns(new StorageCacheItem
            {
                Files = items
            });
        }

        [Theory]
        [InlineData(TeensyStorageType.USB)]
        [InlineData(TeensyStorageType.SD)]
        public async Task Given_FileExists_When_LaunchedRequested_Then_SettingsAreCorrect(TeensyStorageType storageType)
        {
            //Arrange
            var existingSong = CreateFile<SongItem>("/");
            SetupStorageService(existingSong);
            SetupMediatorSuccess();

            PlayerState expectedSettings = new();
            expectedSettings.FilterType = TeensyFilterType.All;
            expectedSettings.CurrentItem = existingSong;
            expectedSettings.StorageType = storageType;
            expectedSettings.PlayState = PlayState.Playing;

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetStorage(storageType);
            await playerService.LaunchFromDirectory(existingSong.Path);
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public async Task Given_FileExists_When_LaunchRequested_Then_FileLaunched()
        {
            //Arrange
            var existingSong = CreateFile<SongItem>("/");
            SetupStorageService(existingSong);
            SetupMediatorSuccess();

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory(existingSong.Path);

            //Assert
            _mediator.ReceivedCalls().Should().HaveCount(1);
        }

        private void SetupMediatorSuccess()
        {
            _mediator.Send(Arg.Any<LaunchFileCommand>()).Returns(new LaunchFileResult { IsSuccess = true });
        }

        [Theory]
        [InlineData(TeensyFilterType.All)]
        [InlineData(TeensyFilterType.Music)]
        [InlineData(TeensyFilterType.Games)]
        [InlineData(TeensyFilterType.Images)]
        public async Task Given_ModeIsRandom_When_FilePlayEnds_Then_NextRandomPlayed_OfFilterType(TeensyFilterType filter)
        {
            //Arrange
            var timer = SetupTimer();
            var settings = SetupSettingsWithFilter(filter);
            var existingSong = CreateFile<SongItem>("/");
            SetupStorageService(existingSong);

            var expectedFileTypes = settings.GetFileTypes(filter);

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchItem(existingSong);

            timer.OnNext(Unit.Default);

            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
            _storageService.Received(1).GetRandomFile(Any<StorageScope>(), Any<string>(), expectedFileTypes);
        }

        [Fact]
        public async Task Given_ModeIsRandom_When_FilePlayEnds_Then_NextRandomPlayed_WithinScope()
        {
            //Arrange
            var timer = SetupTimer();
            var existingSong = CreateFile<SongItem>("/music/MUSIC/1.sid");
            SetupStorageService(existingSong);
            var expectedScope = "/";

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryScope(expectedScope);

            timer.OnNext(Unit.Default);

            await _mediator.Received(1).Send(Any<LaunchFileCommand>());
            _storageService.Received(1).GetRandomFile(Any<StorageScope>(), expectedScope, Any<TeensyFileType[]>());
        }

        [Fact]
        public async Task Given_DirectoryMode_When_FilePlayEnds_Then_NextDirectoryFilePlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/music/2.sid");
            var expectedFile = CreateFile<SongItem>("/music/3.sid");

            SeedStorageDirectory(
            [
                CreateFile<SongItem>("/music/1.sid"),
                currentFile,
                expectedFile,
                CreateFile<SongItem>("/music/4.sid"),
            ]);
            var expectedSettings = new PlayerState
            {
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_When_FilePlayEnds_AndLastFile_Then_FirstPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/music/4.sid");
            var expectedFile = CreateFile<SongItem>("/music/1.sid");

            SeedStorageDirectory(
            [
                expectedFile,
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
                currentFile,
            ]);
            var expectedSettings = new PlayerState
            {
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }


        [Fact]
        public async Task Given_DirectoryMode_When_FilePlayEnds_Then_NextFilePlayed()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.All);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/files/2.sid");
            var expectedFile = CreateFile<GameItem>("/files/3.crt");

            SeedStorageDirectory(
            [
                CreateFile<GameItem>("/files/1.crt"),
                currentFile,
                expectedFile,
                CreateFile<SongItem>("/files/4.sid"),
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.All,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory                
            };

            //Act            
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_And_FilterIsMusic_When_FilePlayEnds_Then_NextFilePlayed()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.All);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/files/2.sid");
            var expectedFile = CreateFile<SongItem>("/files/4.sid");

            SeedStorageDirectory(
            [
                CreateFile<GameItem>("/files/1.crt"),
                currentFile,
                CreateFile<GameItem>("/files/3.crt"),
                expectedFile,
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act            
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_FilterSetToMusic_When_DirectoryModeSelected_FilterChangesToAll()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();

            var expectedFile = CreateFile<SongItem>("/files/4.sid");

            SeedStorageDirectory(
            [
                expectedFile,
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.All,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory(expectedFile.Path);
            playerService.SetDirectoryMode(expectedFile.Path.GetUnixParentPath());
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public async Task Given_DirectoryMode_AndFilterChanged_When_FilePlayEnds_And_OutOfRange_Then_NextFilePlayed()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/files/2.sid");
            var expectedFile = CreateFile<SongItem>("/files/6.sid");

            SeedStorageDirectory(
            [
                CreateFile<GameItem>("/files/1.crt"),
                currentFile,
                CreateFile<GameItem>("/files/3.crt"),
                CreateFile<GameItem>("/files/4.crt"),
                CreateFile<GameItem>("/files/5.crt"),
                expectedFile,
                CreateFile<SongItem>("/files/7.sid"),
                CreateFile<SongItem>("/files/8.sid"),
                CreateFile<GameItem>("/files/9.crt"),
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_And_FilterChange_When_FilePlayEnds_And_OutOfRange_Then_WrapAround_And_PlayNextFile()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<GameItem>("/files/6.crt");
            var expectedFile = CreateFile<SongItem>("/files/1.sid");

            SeedStorageDirectory(
            [
                expectedFile,
                CreateFile<GameItem>("/files/1.crt"),
                CreateFile<GameItem>("/files/3.crt"),
                CreateFile<GameItem>("/files/4.crt"),
                CreateFile<GameItem>("/files/5.crt"),
                currentFile,
                CreateFile<GameItem>("/files/7.crt"),
                CreateFile<GameItem>("/files/8.crt"),
                CreateFile<GameItem>("/files/9.crt"),
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            timer.OnNext(Unit.Default);
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_When_Previous_Then_PreviousFilePlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/music/2.sid");
            var expectedFile = CreateFile<SongItem>("/music/3.sid");

            SeedStorageDirectory(
            [
                CreateFile<SongItem>("/music/1.sid"),
                expectedFile,
                currentFile,
                CreateFile<SongItem>("/music/4.sid"),
            ]);
            var expectedSettings = new PlayerState
            {
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            await playerService.LaunchFromDirectory(currentFile.Path);
            await playerService.PlayPrevious();
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_AndLastFile_When_Previous_Then_FirstPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/music/4.sid");
            var expectedFile = CreateFile<SongItem>("/music/1.sid");

            SeedStorageDirectory(
            [
                currentFile,
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
                expectedFile,

            ]);
            var expectedSettings = new PlayerState
            {
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            await playerService.LaunchFromDirectory(currentFile.Path);
            await playerService.PlayPrevious();
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }


        [Fact]
        public async Task Given_DirectoryMode_WithFilterChange_When_Previous_Then_PreviousFilePlayed()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/files/2.sid");
            var expectedFile = CreateFile<SongItem>("/files/4.sid");

            SeedStorageDirectory(
            [
                CreateFile<GameItem>("/files/1.crt"),
                expectedFile,
                CreateFile<GameItem>("/files/3.crt"),
                currentFile,
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            await playerService.PlayPrevious();
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_SongPlaying_When_Toggled_SongIsPaused() 
        {
            //Arrange
            var playerService = _fixture.Create<PlayerService>();
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            await playerService.LaunchFromDirectory(expectedFile.Path);

            //Act
            playerService.TogglePlay();

            //Assert
            var settings = playerService.GetState();
            settings.PlayState.Should().Be(PlayState.Paused);
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            _progressTimer.Received(1).PauseTimer();
            await _mediator.Received(1).Send(Any<ToggleMusicCommand>());
        }

        [Fact]
        public async Task Given_SongPaused_When_Toggled_SongResumes()
        {
            //Arrange
            var playerService = _fixture.Create<PlayerService>();
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            await playerService.LaunchFromDirectory(expectedFile.Path);
            playerService.TogglePlay();

            //Act
            playerService.TogglePlay();

            //Assert
            var settings = playerService.GetState();
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            settings.PlayState.Should().Be(PlayState.Playing);
            _progressTimer.Received(1).ResumeTimer();
        }

        [Fact]
        public async Task Given_NonSongPlaying_When_Toggled_ItemIsPaused()
        {
            //Arrange
            var playerService = _fixture.Create<PlayerService>();
            var expectedFile = CreateFile<GameItem>("/games/game.crt");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            await playerService.LaunchFromDirectory(expectedFile.Path);

            //Act
            playerService.TogglePlay();

            //Assert
            var settings = playerService.GetState();
            settings.PlayState.Should().Be(PlayState.Paused);
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            _progressTimer.Received(1).PauseTimer();
            await _mediator.Received(1).Send(Any<ResetCommand>());
        }

        [Fact]
        public async Task Given_NonSongPaused_When_Toggled_ItemResumes()
        {
            //Arrange
            var playerService = _fixture.Create<PlayerService>();
            var expectedFile = CreateFile<SongItem>("/games/game.crt");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            await playerService.LaunchFromDirectory(expectedFile.Path);
            playerService.TogglePlay();

            //Act
            playerService.TogglePlay();

            //Assert
            var settings = playerService.GetState();
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            settings.PlayState.Should().Be(PlayState.Playing);
            _progressTimer.Received(1).ResumeTimer();
        }

        [Fact]
        public async Task Given_DirectoryMode_WithFilterChange_And_OutOfRange_When_Previous_Then_PreviousFilePlayed()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<SongItem>("/files/2.sid");
            var expectedFile = CreateFile<SongItem>("/files/6.sid");

            SeedStorageDirectory(
            [
                CreateFile<GameItem>("/files/1.crt"),
                expectedFile,
                CreateFile<GameItem>("/files/3.crt"),
                CreateFile<GameItem>("/files/4.crt"),
                CreateFile<GameItem>("/files/5.crt"),
                currentFile,
                CreateFile<SongItem>("/files/7.sid"),
                CreateFile<SongItem>("/files/8.sid"),
                CreateFile<GameItem>("/files/9.crt"),
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            await playerService.PlayPrevious();
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_WithFilterChange_When_OutOfRange_Then_PreviousFilePlayed_WithWrapAround()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();
            var timer = SetupTimer();
            var currentFile = CreateFile<GameItem>("/files/6.crt");
            var expectedFile = CreateFile<SongItem>("/files/1.sid");

            SeedStorageDirectory(
            [
                currentFile,
                CreateFile<GameItem>("/files/1.crt"),
                CreateFile<GameItem>("/files/3.crt"),
                CreateFile<GameItem>("/files/4.crt"),
                CreateFile<GameItem>("/files/5.crt"),
                expectedFile,
                CreateFile<GameItem>("/files/7.crt"),
                CreateFile<GameItem>("/files/8.crt"),
                CreateFile<GameItem>("/files/9.crt"),
            ]);
            var expectedSettings = new PlayerState
            {
                FilterType = TeensyFilterType.Music,
                CurrentItem = expectedFile,
                PlayState = PlayState.Playing,
                ScopePath = expectedFile.Path.GetUnixParentPath(),
                PlayMode = PlayMode.CurrentDirectory
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode(currentFile.Path.GetUnixParentPath());
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.LaunchFromDirectory(currentFile.Path);
            await playerService.PlayPrevious();
            var resultingSettings = playerService.GetState();

            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_DirectoryMode_When_PlayRandom_Then_RandomLaunched_And_SettingsCorrect()
        {
            //Arrange
            SetupMediatorSuccess();

            var expectedFile = CreateFile<SongItem>("/music/1.sid");

            var expectedSettings = new PlayerState
            {
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.Music,
                PlayState = PlayState.Playing,
                ScopePath = "/music/",
                CurrentItem = expectedFile
            };
            var playerService = _fixture.Create<PlayerService>();
            _storageService.GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns(expectedFile);

            //Act
            playerService.SetDirectoryScope("/music/");
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.PlayRandom();
            var settings = playerService.GetState();

            //Assert
            settings.Should().BeEquivalentTo(expectedSettings);
            _storageService.GetRandomFile(Any<StorageScope>(), "/music/", Any<TeensyFileType[]>());
            await _mediator.Received(1).Send(Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_RandomMode_And_PlayingCurrent_When_Previous_Then_PreviousInHistoryPlayed()
        {
            //Arrange
            SetupMediatorSuccess();

            var playerService = _fixture.Create<PlayerService>();

            _storageService.GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns(CreateFile<SongItem>("/music/1.sid"));
            playerService.SetDirectoryScope("/music/");
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.PlayRandom();

            _storageService.GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns(CreateFile<SongItem>("/music/2.sid"));
            await playerService.PlayRandom();

            //Act
            await playerService.PlayPrevious();
            var settings = playerService.GetState();

            //Assert
            settings.CurrentItem!.Path.Should().Be("/music/1.sid");
            _storageService.Received(2).GetRandomFile(Any<StorageScope>(), "/music/", Any<TeensyFileType[]>());
        }

        [Fact]
        public async Task Given_PlayingHistory_When_Next_Then_NextInHistoryPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            var playerService = _fixture.Create<PlayerService>();

            _storageService.GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns(CreateFile<SongItem>("/music/1.sid"));
            playerService.SetDirectoryScope("/music/");
            playerService.SetFilter(TeensyFilterType.Music);
            await playerService.PlayRandom();

            _storageService.GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns(CreateFile<SongItem>("/music/2.sid"));
            await playerService.PlayRandom();

            //Act
            await playerService.PlayPrevious();
            await playerService.PlayNext();
            var settings = playerService.GetState();

            //Assert
            settings.CurrentItem!.Path.Should().Be("/music/2.sid");
            _storageService.Received(2).GetRandomFile(Any<StorageScope>(), "/music/", Any<TeensyFileType[]>());
        }

        [Fact]
        public async Task Given_SearchMode_When_FileComples_Then_And_NextSearchSongPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            SetupSearchStorageService(
            [
                CreateFile<SongItem>("/music/1.sid"),
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
            ]);
            var timer = SetupTimer();

            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/1.sid");
            playerService.SetSearchMode("search query");

            //Act
            timer.OnNext(Unit.Default);
            var settings = playerService.GetState();

            //Arrange
            settings.CurrentItem!.Path.Should().Be("/music/2.sid");
        }

        [Fact]
        public async Task Given_SearchMode_And_LastItemPlayed_When_FileComples_Then_And_FirstSearchSongPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            SetupSearchStorageService(
            [
                CreateFile<SongItem>("/music/1.sid"),
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
            ]);
            var timer = SetupTimer();

            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/3.sid");
            playerService.SetSearchMode("search query");

            //Act
            timer.OnNext(Unit.Default);
            var settings = playerService.GetState();

            //Arrange
            settings.CurrentItem!.Path.Should().Be("/music/1.sid");
        }

        [Fact]
        public async Task Given_SearchMode_When_Previous_Then_PreviousSearchSongPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            SetupSearchStorageService(
            [
                CreateFile<SongItem>("/music/1.sid"),
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
            ]);

            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/2.sid");
            playerService.SetSearchMode("search query");

            //Act
            await playerService.PlayPrevious();
            var settings = playerService.GetState();

            //Arrange
            settings.CurrentItem!.Path.Should().Be("/music/1.sid");
        }

        [Fact]
        public async Task Given_SearchMode_And_FirstSearchResultPlaying_When_Previous_Then_LastSearchSongPlayed()
        {
            //Arrange
            SetupMediatorSuccess();
            SetupSearchStorageService(
            [
                CreateFile<SongItem>("/music/1.sid"),
                CreateFile<SongItem>("/music/2.sid"),
                CreateFile<SongItem>("/music/3.sid"),
            ]);

            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchFromDirectory("/music/1.sid");
            playerService.SetSearchMode("search query");

            //Act
            await playerService.PlayPrevious();
            var settings = playerService.GetState();

            //Arrange
            settings.CurrentItem!.Path.Should().Be("/music/3.sid");
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Launched_And_ModeRandom_And_FileBad_Then_FileMarkedIncompatible(LaunchFileResultType launchResult) 
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            SetupStorageServiceRandom(file1);
            SetupStorageService(file1, file2);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo => 
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file1.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = true,
                        LaunchResult = LaunchFileResultType.Success
                    };
                }
                return new LaunchFileResult 
                {
                    IsSuccess = false,
                    LaunchResult = launchResult
                };                    
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetRandomMode("/music/");

            //Act

            await playerService.LaunchFromDirectory(file2.Path);

            //Assert
            _storageService.Received(1).MarkIncompatible(file2);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Launched_And_ModeDirectory_And_FileBad_Then_FileMarkedIncompatible(LaunchFileResultType launchResult)
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            SetupStorageServiceRandom(file1);
            SetupStorageService(file1, file2);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo =>
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file1.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = true,
                        LaunchResult = LaunchFileResultType.Success
                    };
                }
                return new LaunchFileResult
                {
                    IsSuccess = false,
                    LaunchResult = launchResult
                };
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode("/music/");

            //Act

            await playerService.LaunchFromDirectory(file2.Path);

            //Assert
            _storageService.Received(1).MarkIncompatible(file2);
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Launched_And_ModeDirectory_And_FileBad_Then_SkipFileAndPlayNext(LaunchFileResultType launchResult)
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            SetupStorageServiceRandom(file1, file2);
            SetupStorageService(file1, file2);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo =>
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file1.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = true,
                        LaunchResult = LaunchFileResultType.Success
                    };
                }
                return new LaunchFileResult
                {
                    IsSuccess = false,
                    LaunchResult = launchResult
                };
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode("/music/");

            //Act

            await playerService.LaunchFromDirectory(file2.Path);

            //Assert
            _storageService.Received(1).MarkIncompatible(file2);
            await _storageService.Received(2).GetDirectory(Any<string>());
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Launched_And_ModeRandom_And_FileBad_Then_SkipFileAndPlayNext(LaunchFileResultType launchResult)
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            SetupStorageServiceRandom(file1, file2);
            SetupStorageService(file1, file2);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo =>
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file1.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = true,
                        LaunchResult = LaunchFileResultType.Success
                    };
                }
                return new LaunchFileResult
                {
                    IsSuccess = false,
                    LaunchResult = launchResult
                };
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetRandomMode("/");

            //Act

            await playerService.LaunchFromDirectory(file2.Path);

            //Assert
            _storageService.Received(1).MarkIncompatible(file2);
            _storageService.Received(1).GetRandomFile(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>());
            await _storageService.Received(1).GetDirectory(Any<string>());
            await _mediator.Received(2).Send(Any<LaunchFileCommand>());            
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Previous_And_ModeDirectory_And_FileBad_Then_SkipFileAndPlayPrevious(LaunchFileResultType launchResult)
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            var file3 = CreateFile<SongItem>("/music/sid3.sid");
            SetupStorageService(file1, file2, file3);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo =>
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file2.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = false,
                        LaunchResult = launchResult
                    };
                }
                return new LaunchFileResult
                {
                    IsSuccess = true,
                    LaunchResult = LaunchFileResultType.Success
                };
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode("/music/");

            //Act
            await playerService.LaunchFromDirectory(file3.Path);
            await playerService.PlayPrevious();
            var finalState = playerService.GetState();

            //Assert
            finalState.CurrentItem.Should().BeEquivalentTo(file1);
            _storageService.Received(1).MarkIncompatible(file2);
            await _mediator.Received(3).Send(Any<LaunchFileCommand>());
        }

        [Theory]
        [InlineData(LaunchFileResultType.ProgramError)]
        [InlineData(LaunchFileResultType.SidError)]
        public async Task When_Next_And_ModeDirectory_And_FileBad_Then_SkipFileAndPlayPrevious(LaunchFileResultType launchResult)
        {
            //Arrange
            var file1 = CreateFile<SongItem>("/music/sid1.sid");
            var file2 = CreateFile<SongItem>("/music/sid2.sid");
            var file3 = CreateFile<SongItem>("/music/sid3.sid");
            SetupStorageService(file1, file2, file3);

            _mediator.Send(Any<LaunchFileCommand>()).Returns(callInfo =>
            {
                var launchCommand = callInfo.Arg<LaunchFileCommand>();

                if (launchCommand.Path == file3.Path)
                {
                    return new LaunchFileResult
                    {
                        IsSuccess = false,
                        LaunchResult = launchResult
                    };
                }
                return new LaunchFileResult
                {
                    IsSuccess = true,
                    LaunchResult = LaunchFileResultType.Success
                };
            });
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetDirectoryMode("/music/");

            //Act
            await playerService.LaunchFromDirectory(file2.Path);
            await playerService.PlayNext();
            var finalState = playerService.GetState();

            //Assert
            finalState.CurrentItem.Should().BeEquivalentTo(file1);
            _storageService.Received(1).MarkIncompatible(file3);
            await _mediator.Received(3).Send(Any<LaunchFileCommand>());
        }


        private T CreateFile<T>(string path) where T : ILaunchableItem
            {
                var name = path.GetFileNameFromPath();
                return _fixture.Build<T>()
                    .With(s => s.Name, $"{name}")
                    .With(s => s.Path, $"{path}")
                    .With(s => s.IsCompatible, true)
                    .Create();
            }

        private Subject<Unit> SetupTimer()
        {
            var timer = new Subject<Unit>();
            _progressTimer.TimerComplete.Returns(timer);
            return timer;
        }

        private void SetupStorageServiceRandom(params ILaunchableItem[] items)
        {
            var random = new Random();

            _storageService
                .GetRandomFile(Arg.Any<StorageScope>(), Arg.Any<string>(), Arg.Any<TeensyFileType[]>())
                .Returns(_ =>
                {
                    if (items.Length > 1) 
                    {
                        return items[random.Next(items.Length - 1)];
                    }
                    return items[0];
                });
        }

        private void SetupStorageService(params ILaunchableItem[] items)
        {
            var random = new Random();

            SetupStorageServiceRandom(items);

            _storageService
                .GetDirectory(Any<string>())
                .Returns(new StorageCacheItem { Files = items.Cast<IFileItem>().ToList() });
        }

        private void SetupSearchStorageService(List<ILaunchableItem> items)
        {
            _storageService
                .GetDirectory(Any<string>())
                .Returns(new StorageCacheItem { Files = items.Cast<IFileItem>().ToList() });

            _storageService.Search(Any<string>(), Any<TeensyFileType[]>())
                .Returns(items);
        }

        private TeensySettings SetupSettingsWithFilter(TeensyFilterType filter)
        {
            var settings = new TeensySettings
            {
                StartupFilter = filter
            };
            settings.InitializeDefaults();

            _settingsService.GetSettings().Returns(s => settings);
            return settings;
        }

        private TeensySettings SetupSettingsWithStorage(TeensyStorageType storage)
        {
            var settings = new TeensySettings
            {
                StorageType = storage
            };
            settings.InitializeDefaults();

            _settingsService.GetSettings().Returns(s => settings);
            return settings;
        }





        private T Any<T>() => Arg.Any<T>();
    }
    
}