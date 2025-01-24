using NSubstitute;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MediatR;
using TeensyRom.Core.Storage.Services;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Unit = System.Reactive.Unit;
using TeensyRom.Cli.Services;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Progress;
using TeensyRom.Core.Commands.LaunchFile;
using TeensyRom.Core.Commands.Reset;
using TeensyRom.Core.Commands.ToggleMusic;
using TeensyRom.Core.Player;
using TeensyRom.Player;

namespace TeensyRom.Cli.Tests
{
    public class PlayerServiceTests
    {
        private ICachedStorageService _storageService;
        private IProgressTimer _progressTimerMock;
        private IPlayer _playerMock;
        private Subject<ILaunchableItem> _currentItemSubject = new();
        private Subject<ILaunchableItem> _badFileSubject = new();
        private Subject<Player.PlayerState> _playStateSubject = new();
        private IMediator _mediator;
        private ISettingsService _settingsService;
        private ISerialStateContext _serialContext;
        private ILaunchHistory _launchHistory;
        private const string _sidPath = "/music/MUSIC/1.sid";

        private IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization() { ConfigureMembers = true });

        public PlayerServiceTests()
        {
            _storageService = _fixture.Freeze<ICachedStorageService>();            
            _mediator = _fixture.Freeze<IMediator>();
            _progressTimerMock = _fixture.Freeze<IProgressTimer>();
            _playerMock = _fixture.Freeze<IPlayer>();

            _playerMock.BadFile.Returns(_badFileSubject.AsObservable());
            _playerMock.PlayerState.Returns(_playStateSubject.AsObservable());
            _playerMock.CurrentItem.Returns(_currentItemSubject.AsObservable());

            var timerCompleteSubject = new Subject<Unit>();
            _progressTimerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());

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

            var expectedSettings = new Services.PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = Player.PlayerState.Stopped,
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

            var expectedSettings = new Services.PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.USB,
                PlayState = Player.PlayerState.Stopped,
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

            var expectedSettings = new Services.PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = Player.PlayerState.Stopped,
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

            var expectedSettings = new Services.PlayerState
            {
                CurrentItem = null,
                StorageType = TeensyStorageType.SD,
                PlayState = Player.PlayerState.Stopped,
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
            var expectedSettings = new Services.PlayerState
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
            var expectedSettings = new Services.PlayerState
            {
                PlayMode = PlayMode.Search,
                SearchQuery = "this is a query",
                SidTimer = SidTimer.SongLength,
                FilterType = TeensyFilterType.All,
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
        public async Task Given_PlayerFirstInitialization_And_SetDirectoryMode_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new Services.PlayerState
            {
                PlayMode = PlayMode.CurrentDirectory,
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };
            var storageCacheItem = _fixture.Create<StorageCacheItem>();
            storageCacheItem.Files.Add(CreateFile<SongItem>("/music/song.sid"));

            _storageService.GetDirectory(Arg.Any<string>()).Returns(storageCacheItem);
            var playerService = _fixture.Create<PlayerService>();

            //Act            
            playerService.SetSearchMode("this is a query");
            await playerService.SetDirectoryMode("/music");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetRandom_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new Services.PlayerState
            {
                PlayMode = PlayMode.Random,
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            playerService.SetRandomMode();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetFilter_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new Services.PlayerState
            {
                PlayMode = PlayMode.Random,
                SidTimer = SidTimer.SongLength,
                PlayTimer = null,
                SearchQuery = null
            };

            //Act
            var playerService = _fixture.Create<PlayerService>();
            playerService.SetSearchMode("this is a query");
            playerService.SetRandomMode();
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public void Given_PlayerFirstInitialization_And_SetStreamTime_Then_SettingsCorrect()
        {
            //Arrange
            var expectedSettings = new Services.PlayerState
            {
                PlayState = Player.PlayerState.Stopped,
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
            var expectedSettings = new Services.PlayerState
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

            await playerService.LaunchItem("/music/sid1.sid");
            playerService.SetStreamTime(TimeSpan.FromDays(1));

            _playerMock.Received(1).SetPlayTimer(TimeSpan.FromDays(1));
        }

        [Fact]
        public async Task When_FileLaunched_Then_Emit()
        {
            //Arrange
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            var playerService = _fixture.Create<PlayerService>();

            //Act
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            playerService.FileLaunched.Subscribe(tcs.SetResult);

            await playerService.LaunchItem(expectedFile.Path);
            _currentItemSubject.OnNext(expectedFile);
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
            Services.PlayerState expectedSettings = new();
            expectedSettings.CurrentItem = null;
            expectedSettings.StorageType = TeensyStorageType.SD;
            expectedSettings.PlayState = Player.PlayerState.Stopped;

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchItem("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        [Fact]
        public async Task Given_FileDoesNotExist_When_LaunchedRequested_Then_FileDoesNotLaunch()
        {
            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchItem("/music/MUSIC/doesntExist.sid");

            //Assert
            await _playerMock.DidNotReceive().PlayItem(Arg.Any<ILaunchableItem>());
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
            Services.PlayerState expectedSettings = new();
            expectedSettings.CurrentItem = null;
            expectedSettings.StorageType = TeensyStorageType.SD;
            expectedSettings.PlayState = Player.PlayerState.Stopped;

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchItem("/music/MUSIC/doesntExist.sid");
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
            await playerService.LaunchItem("/music/MUSIC/doesntExist.sid");
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
            await playerService.LaunchItem("/music/MUSIC/doesntExist.sid");
            var playerSettings = playerService.GetState();

            //Assert
            _progressTimerMock.DidNotReceive().StartNewTimer(Arg.Any<TimeSpan>());
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

            Services.PlayerState expectedSettings = new()
            {
                FilterType = TeensyFilterType.All,
                CurrentItem = existingSong,
                StorageType = storageType,
                PlayState = Player.PlayerState.Playing
            };
            var playerService = _fixture.Create<PlayerService>();

            //Act
            playerService.SetStorage(storageType);
            await playerService.LaunchItem(existingSong.Path);
            _currentItemSubject.OnNext(existingSong);
            _playStateSubject.OnNext(Player.PlayerState.Playing);
            var playerSettings = playerService.GetState();

            //Assert
            playerSettings.Should().BeEquivalentTo(expectedSettings);
        }

        private void SetupMediatorSuccess()
        {
            _mediator.Send(Arg.Any<LaunchFileCommand>()).Returns(new LaunchFileResult { IsSuccess = true });
        }

        [Fact]
        public async Task Given_FilterSetToMusic_When_DirectoryModeSelected_FilterChangesToAll()
        {
            //Arrange            
            SetupSettingsWithFilter(TeensyFilterType.Music);
            SetupMediatorSuccess();

            var expectedSettings = new Services.PlayerState
            {
                FilterType = TeensyFilterType.All,
                PlayMode = PlayMode.CurrentDirectory,                
            };
            _storageService.GetDirectory("/some/path").Returns(new StorageCacheItem
            {
                Files = new List<IFileItem>() 
                {
                    CreateFile<SongItem>("/some/path/someSid.sid")
                }
            });

            //Act
            var playerService = _fixture.Create<PlayerService>();
            await playerService.SetDirectoryMode("/some/path");
            var resultingSettings = playerService.GetState();

            //Assert
            resultingSettings.Should().BeEquivalentTo(expectedSettings);
            _playerMock.Received(1).SetFilter(TeensyFilterType.All);
        }
                
        [Fact]
        public async Task Given_SongPlaying_When_Toggled_SongIsPaused() 
        {
            //Arrange            
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            var playerService = _fixture.Create<PlayerService>();
            await playerService.LaunchItem(expectedFile.Path);
            _playStateSubject.OnNext(Player.PlayerState.Playing);
            _currentItemSubject.OnNext(expectedFile);

            //Act
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Paused);

            //Assert
            var settings = playerService.GetState();
            settings.PlayState.Should().Be(Player.PlayerState.Paused);
            await _playerMock.Received(1).Pause();
        }

        [Fact]
        public async Task Given_SongPaused_When_Toggled_SongResumes()
        {
            //Arrange            
            var expectedFile = CreateFile<SongItem>("/music/sid1.sid");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            var playerService = _fixture.Create<PlayerService>();

            await playerService.LaunchItem(expectedFile.Path);
            _currentItemSubject.OnNext(expectedFile);
            _playStateSubject.OnNext(Player.PlayerState.Playing);
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Paused);

            //Act
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Playing);


            //Assert
            var settings = playerService.GetState();
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            settings.PlayState.Should().Be(Player.PlayerState.Playing);
            await _playerMock.Received(1).ResumeItem();
        }

        [Fact]
        public async Task Given_NonSongPlaying_When_Toggled_ItemIsStopped()
        {
            //Arrange            
            var expectedFile = CreateFile<GameItem>("/games/game.crt");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();

            var playerService = _fixture.Create<PlayerService>();

            await playerService.LaunchItem(expectedFile.Path);
            _currentItemSubject.OnNext(expectedFile);
            _playStateSubject.OnNext(Player.PlayerState.Playing);

            //Act
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Stopped);

            //Assert
            var settings = playerService.GetState();
            settings.PlayState.Should().Be(Player.PlayerState.Stopped);
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            _playerMock.Received(1).Stop();
        }

        [Fact]
        public async Task Given_NonSongPaused_When_Toggled_ItemResumes()
        {
            //Arrange
            var expectedFile = CreateFile<SongItem>("/games/game.crt");
            var playTimer = SetupTimer();
            SetupStorageService(expectedFile);
            SetupMediatorSuccess();
            var playerService = _fixture.Create<PlayerService>();

            await playerService.LaunchItem(expectedFile.Path);
            _currentItemSubject.OnNext(expectedFile);
            _playStateSubject.OnNext(Player.PlayerState.Playing);
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Stopped);

            //Act
            playerService.TogglePlay();
            _playStateSubject.OnNext(Player.PlayerState.Playing);

            //Assert
            var settings = playerService.GetState();
            settings.CurrentItem.Should().BeEquivalentTo(expectedFile);
            settings.PlayState.Should().Be(Player.PlayerState.Playing);
            await _playerMock.Received(1).ResumeItem();
        }

        [Fact]
        public async Task Given_DirectoryMode_When_PlayRandom_Then_RandomLaunched_And_SettingsCorrect()
        {
            //Arrange
            SetupMediatorSuccess();

            var expectedFile = CreateFile<SongItem>("/music/1.sid");

            var expectedSettings = new Services.PlayerState
            {
                PlayMode = PlayMode.Random,
                FilterType = TeensyFilterType.Music,
                PlayState = Player.PlayerState.Playing,
                ScopePath = "/music/",
                CurrentItem = expectedFile
            };

            var playerService = _fixture.Create<PlayerService>();
            _storageService.GetRandomFiles(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>()).Returns([expectedFile]);

            playerService.SetDirectoryScope("/music/");
            playerService.SetFilter(TeensyFilterType.Music);
            playerService.SetRandomMode();

            //Act
            await playerService.PlayRandom();
            _currentItemSubject.OnNext(expectedFile);
            _playStateSubject.OnNext(Player.PlayerState.Playing);
            var settings = playerService.GetState();

            //Assert
            settings.Should().BeEquivalentTo(expectedSettings);
            _storageService.GetRandomFiles(Any<StorageScope>(), "/music/", Any<TeensyFileType[]>());
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

            var playerService = _fixture.Create<PlayerService>();
            await playerService.SetDirectoryMode("/music/");

            //Act
            await playerService.LaunchItem(file2.Path);
            _badFileSubject.OnNext(file2);

            //Assert
            _storageService.Received(1).MarkIncompatible(file2);
            _playerMock.Received(1).RemoveItem(file2);

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

            var playerService = _fixture.Create<PlayerService>();

            //Act
            await playerService.SetDirectoryMode("/music/");
            await playerService.LaunchItem(file2.Path);
            _badFileSubject.OnNext(file1);

            //Assert                        
            await _playerMock.Received(1).PlayNext();
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

            _storageService.GetRandomFiles(Any<StorageScope>(), Any<string>(), Any<TeensyFileType[]>())
                .Returns(new List<ILaunchableItem> { file1, file2 });
            
            var playerService = _fixture.Create<PlayerService>();

            //Act
            playerService.SetRandomMode();
            await playerService.LaunchItem(file2.Path);
            _badFileSubject.OnNext(file1);

            //Assert                        
            await _playerMock.Received(1).PlayNext();
        }

        [Fact]
        public void When_FilterSet_PlayerFilterSet() 
        {
            // Arrange
            var playerService = _fixture.Create<PlayerService>();

            // Act
            playerService.SetFilter(TeensyFilterType.Games);
            var state = playerService.GetState();

            // Assert
            state.FilterType.Should().Be(TeensyFilterType.Games);
            _playerMock.Received(1).SetFilter(TeensyFilterType.Games);
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
            _progressTimerMock.TimerComplete.Returns(timer);
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