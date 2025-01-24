using MediatR;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using TeensyRom.Core.Commands.LaunchFile;
using TeensyRom.Core.Commands.Reset;
using TeensyRom.Core.Commands.ToggleMusic;
using TeensyRom.Core.Progress;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using AutoFixture;
using NSubstitute.Extensions;
using System.Reactive.Subjects;

namespace TeensyRom.Player.Tests
{
    public class PlayerTests
    {
        private readonly Player _player;
        private readonly IMediator _mediatorMock;
        private readonly IProgressTimer _timerMock;
        private readonly ISettingsService _settingsServiceMock;
        private Fixture _fixture = new();
        private List<ILaunchableItem> _songs = [];
        private List<ILaunchableItem> _games = [];
        public PlayerTests()
        {
            _mediatorMock = Substitute.For<IMediator>();
            _mediatorMock
                .Send(Arg.Any<LaunchFileCommand>())
                .Returns(new LaunchFileResult { LaunchResult = LaunchFileResultType.Success });

            _timerMock = Substitute.For<IProgressTimer>();
            var timerCompleteSubject = new Subject<System.Reactive.Unit>();
            _timerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());

            _settingsServiceMock = Substitute.For<ISettingsService>();
            _settingsServiceMock.GetSettings().Returns(new TeensySettings());
            _player = new Player(_timerMock, _mediatorMock, _settingsServiceMock);

            _songs = _fixture
                .Build<SongItem>()
                .With(s => s.PlayLength, TimeSpan.FromSeconds(30))
                .With(s => s.Path, $"/path/{Guid.NewGuid().ToString()}.sid")
                .CreateMany(10)
                .Select(s => s as ILaunchableItem)
                .ToList();

            _games = _fixture
                .Build<GameItem>()
                .With(g => g.PlayLength, TimeSpan.FromSeconds(30))
                .With(g => g.Path, $"/path/{Guid.NewGuid().ToString()}.crt") 
                .CreateMany(10)
                .Select(g => g as ILaunchableItem)
                .ToList();
        }

        [Fact]
        public async Task Given_NewState_ReturnsStopped()
        {
            // Act
            PlayerState? emittedState = null;
            var tcs = new TaskCompletionSource<PlayerState>();

            using var subscription = _player.PlayerState.Subscribe(state =>
            {
                emittedState = state;
                tcs.TrySetResult(state);
            });
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Stopped);
        }

        [Fact]
        public void Given_NoPlaylistSet_ReturnsEmptyList()
        {
            //Act
            var result = _player.GetPlaylist();
            //Assert
            result.Should().BeEmpty();
        }
        [Fact]
        public void Given_PlaylistSet_ReturnsItems()
        {
            //Arrange
            _player.SetPlaylist(_songs);

            //Act
            var result = _player.GetPlaylist();

            //Assert
            result.ForEach(item => _songs.Any(i => item.Id == i.Id));
            result.Count.Should().Be(_songs.Count);

        }

        [Fact]
        public void Given_PlaylistSet_WhenNewItemsSet_ReturnsItems()
        {
            //Arrange
            _player.SetPlaylist(_songs);
            _player.SetPlaylist(_games);

            //Act
            var result = _player.GetPlaylist();

            //Assert
            result.ForEach(item => _games.Any(i => item.Name == i.Name));
            result.Count.Should().Be(_games.Count);

        }

        [Fact]
        public void Given_ItemsSet_When_ItemSet_ReturnsItem()
        {
            //Arrange
            _player.SetPlaylist(_songs);
            
            //Act
            var result = _player.GetPlaylist();
            
            //Assert
            result.First().Name.Should().Be(_songs.First().Name);
        }

        [Fact]
        public async Task Given_ItemsSet_When_ItemSet_EmitsItem() 
        {
            //Arrange
            _player.SetPlaylist(_songs);
            ILaunchableItem? emittedItem = null;
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            using var subscription = _player.CurrentItem.Skip(1).Subscribe(item =>
            {
                emittedItem = item;
                tcs.TrySetResult(item!);
            });

            //Act
            await _player.PlayItem(_songs.First());
            await tcs.Task;

            //Assert
            emittedItem.Should().Be(_songs.First());
        }

        [Fact]
        public async Task Given_Stopped_When_PlayCalled_EmitsPlayingState()
        {
            // Arrange
            PlayerState? emittedState = null;
            var tcs = new TaskCompletionSource<PlayerState>();
            using var subscription = _player.PlayerState.Subscribe(state =>
            {
                emittedState = state;
                tcs.TrySetResult(state);
            });
            _player.SetPlaylist(_songs);

            // Act
            await _player.PlayItem(_songs.First());
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Playing);
        }

        [Fact]
        public async Task Given_Stopped_When_Play_Then_EmitsCurrentTimeZero()
        {
            // Arrange
            _player.SetPlaylist(_songs);

            TimeSpan? emittedTime = null;

            var tcs = new TaskCompletionSource<TimeSpan>();
            using var subscription = _player.CurrentTime.Subscribe(state =>
            {
                emittedTime = state;
                tcs.TrySetResult(state);
            });

            // Act
            await _player.PlayItem(_songs.First());
            await tcs.Task;

            // Assert
            emittedTime.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task Given_Playing2Seconds_Then_EmitsTimer2Seconds()
        {
            // Arrange
            var player = new Player(new ProgressTimer(), _mediatorMock, _settingsServiceMock);

            player.SetPlaylist(_songs);
            await player.PlayItem(_songs.First());

            TimeSpan? emittedTime = null;
            var tcs = new TaskCompletionSource<TimeSpan>();

            using var subscription = player.CurrentTime.Subscribe(state =>
            {
                emittedTime = state;
                tcs.TrySetResult(state);
            });
            // Act
            await Task.Delay(2000);
            await tcs.Task;
            // Assert
            emittedTime.Should().Be(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task Given_SongSet_And_PlayStateStopped_When_Play_TimerSetToSongLength() 
        {
            // Arrange
            var playItem = _songs.First() as SongItem; 

            var timerMock = Substitute.For<IProgressTimer>();
            var timerCompleteSubject = new Subject<System.Reactive.Unit>();
            timerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());            

            var player = new Player(timerMock, _mediatorMock, _settingsServiceMock);

            // Act
            _player.SetPlaylist([playItem]);
            await player.PlayItem(playItem);

            //Assert
            timerMock.Received(1).StartNewTimer(Arg.Is<TimeSpan>(t => t == playItem.PlayLength));
        }

        [Fact]
        public async Task Given_NonSongSet_AndPlayTimerNull_When_Played_TimerNotSet()
        {
            // Arrange

            // Act
            _player.SetPlaylist([_games.First()]);
            await _player.PlayItem(_games.First());

            //Assert
            _timerMock.DidNotReceive().StartNewTimer(Arg.Any<TimeSpan>());
        }

        [Fact]
        public async Task Given_NonSongSet_AndPlayTimerNotNull_When_Played_TimerSet()
        {
            // Arrange
            var timerMock = Substitute.For<IProgressTimer>();
            var timerCompleteSubject = new Subject<System.Reactive.Unit>();
            timerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());

            var player = new Player(timerMock, _mediatorMock, _settingsServiceMock);          
            player.SetPlaylist([_games.First()]);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));

            // Act                     
            await player.PlayItem(_games.First());

            //Assert
            timerMock.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task Given_SongSet_And_PlayTimerSet_And_SongTimerOverrideSet_When_Played_TimerSetToPlayTimer() 
        {
            // Arrange
            var timerMock = Substitute.For<IProgressTimer>();
            var timerCompleteSubject = new Subject<System.Reactive.Unit>();
            timerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());

            var player = new Player(timerMock, _mediatorMock, _settingsServiceMock);

            // Act
            player.SetPlaylist(_songs);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));
            player.EnableSongTimeOverride();
            await player.PlayItem(_songs.First());

            //Assert
            timerMock.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task Given_SongSet_And_PlayTimerSet_And_SongTimerOverrideSet_When_Played_TimerSetToSongTime()
        {
            // Arrange
            var playItem = _songs.First() as SongItem;
            var timerMock = Substitute.For<IProgressTimer>();
            var timerCompleteSubject = new Subject<System.Reactive.Unit>();
            timerMock.TimerComplete.Returns(timerCompleteSubject.AsObservable());
            var player = new Player(timerMock, _mediatorMock, _settingsServiceMock);

            // Act
            player.SetPlaylist([playItem]);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));
            player.DisableSongTimeOverride();
            await player.PlayItem(playItem!);

            //Assert
            timerMock.Received().StartNewTimer(playItem!.PlayLength);
        }

        [Fact]
        public async Task Given_Paused_When_PlayCalled_EmitsPlaying()
        {
            // Arrange
            var playItem = _songs.First() as SongItem;
            _player.SetPlaylist(_songs);
            await _player.Pause();

            PlayerState? emittedState = null;
            var tcs = new TaskCompletionSource<PlayerState>();
            using var subscription = _player.PlayerState.Subscribe(state =>
            {
                emittedState = state;
                tcs.TrySetResult(state);
            });

            // Act
            await _player.ResumeItem();
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Playing);
        }

        [Fact]
        public async Task Given_Paused_And_MusicPlaying_When_PlayCalled_Sends_ToggleMusicCommand() 
        {
            // Arrange         
            _player.SetPlaylist(_songs);
            await _player.PlayItem(_songs.First());   
            await _player.Pause();

            // Act
            await _player.ResumeItem();            

            // Assert
            await _mediatorMock.Received().Send(Arg.Any<ToggleMusicCommand>());
        }        

        [Fact]
        public async Task Given_Stopped_When_PlayedCalled_Sends_LaunchFileCommand()
        {
            // Arrange
            _player.SetPlaylist(_songs);

            // Act
            await _player.PlayItem(_songs.First());
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_Playing_When_PlayedCalled_Sends_LaunchFileCommand()
        {
            // Arrange
            _player.SetPlaylist(_songs);

            // Act
            await _player.PlayItem(_songs.First());

            // Assert
            await _mediatorMock.Received().Send(Arg.Any<LaunchFileCommand>());
        }

        [Fact]
        public async Task Given_Stopped_EmitsStoppedState() 
        {
            // Arrange
            PlayerState? emittedState = null;
            var tcs = new TaskCompletionSource<PlayerState>();
            using var subscription = _player.PlayerState.Subscribe(state =>
            {
                emittedState = state;
                tcs.TrySetResult(state);
            });

            // Act
            _player.Stop();
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Stopped);
        }

        [Fact]
        public async Task Given_PlayingMusic_When_Paused_PausesTimer() 
        {
            // Arrange            
            _player.SetPlaylist(_songs);
            await _player.PlayItem(_songs.First());

            // Act
            await _player.Pause();

            // Assert
            _timerMock.ReceivedWithAnyArgs(1).PauseTimer();
        }

        [Fact]
        public async Task Given_BadSid_WhenPlayed_EmitsBadFile() 
        {
            // Arrange
            var mediatorMock = Substitute.For<IMediator>();
            mediatorMock
                .Send(Arg.Any<LaunchFileCommand>())
                .Returns(new LaunchFileResult { LaunchResult = LaunchFileResultType.SidError });

            var player = new Player(new ProgressTimer(), mediatorMock, _settingsServiceMock);

            var playItem = _songs.First();
            player.SetPlaylist(_songs);            

            ILaunchableItem? emittedItem = null;
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            using var subscription = player.BadFile.Subscribe(item =>
            {
                emittedItem = item;
                tcs.TrySetResult(item);
            });

            // Act
            await player.PlayItem(playItem);

            // Assert
            emittedItem!.Id.Should().Be(playItem.Id);
        }

        [Fact] 
        public async Task Given_FilesSet_When_Next_PlaysNext()
        {
            // Arrange
            var playItem = _songs.First();
            var nextItem = _games.Last();
            List<ILaunchableItem> items = [playItem, nextItem];
            _player.SetPlaylist(items);
            await _player.PlayItem(playItem);
            
            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == nextItem.Id));
        }

        [Fact]
        public async Task Given_FilesSet_And_LastItem_When_Next_GoesToFirst() 
        {
            // Arrange
            var playItem = _songs.First();
            var nextItem = _games.Last();
            List<ILaunchableItem> items = [nextItem, playItem];
            _player.SetPlaylist(items);
            await _player.PlayItem(playItem);

            // Act
            await _player.PlayNext();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == nextItem.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilterIsMusic_GoesToNextSong() 
        {
            // Arrange
            var first = _songs.First() as SongItem;
            var second = _games.First() as GameItem;
            var third = _songs.Last() as SongItem;

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem(first!);

            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == third!.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_And_FilterIsMusic_And_OnlyOneSong_WhenNext_RestartsTheSong() 
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem(first);

            // Act
            await _player.PlayNext();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id== first.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilterIsMusic_And_LastSong_GoesToFirstSong()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem(third);
            
            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id== first.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilerIsMusic_And_GameOutOfRange_PlaysNextRelativeSong ()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _songs[1];
            var fourth = _games[1];
            var fifth = _games[2];
            var sixth = _games[3];
            var seventh = _games[4];
            var eigth = _songs[2];

            List<ILaunchableItem> items = [first, second, third, fourth, fifth, sixth, seventh, eigth];
            _player.SetPlaylist(items);            
            await _player.PlayItem(seventh);
            _player.SetFilter(TeensyFilterType.Music);

            // Act
            await _player.PlayNext();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == eigth.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilerIsMusic_And_GameOutOfRange_PlaysPreviousRelativeSong()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _songs[1];
            var fourth = _games[1];
            var fifth = _games[2];
            var sixth = _games[3];
            var seventh = _games[4];
            var eigth = _songs[2];

            List<ILaunchableItem> items = [first, second, third, fourth, fifth, sixth, seventh, eigth];
            _player.SetPlaylist(items);
            await _player.PlayItem(eigth);
            _player.SetFilter(TeensyFilterType.Games);

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == seventh.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_SwitchingFilters_Then_PreviousHistoryIsMaintained()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _songs[1];
            var fourth = _games[1];
            var fifth = _games[2];
            var sixth = _games[3];
            var seventh = _games[4];
            var eigth = _songs[2];

            List<ILaunchableItem> items = [first, second, third, fourth, fifth, sixth, seventh, eigth];
            _player.SetPlaylist(items);

            // Act
            await _player.PlayItem(seventh);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayNext();
            await _player.PlayNext();
            _player.SetFilter(TeensyFilterType.Games);
            await _player.PlayPrevious();

            // Assert
            var lastCommand = _mediatorMock.ReceivedCalls()
                .Last(call => call.GetMethodInfo().Name == nameof(IMediator.Send));

            var lastFileLaunched = lastCommand.GetArguments()[0] as LaunchFileCommand;
            lastFileLaunched.Should().NotBeNull();
            lastFileLaunched!.LaunchItem.Id.Should().Be(seventh.Id);
        }

        [Fact]
        public async Task Given_MixedFileSet_When_AndFilterAll_Then_PreviousHistoryIsMaintained()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];
            var fourth = _games[2];


            List<ILaunchableItem> items = [first, second, third, fourth];
            _player.SetPlaylist(items);

            // Act
            _player.SetFilter(TeensyFilterType.All);
            await _player.PlayItem(first);            
            await _player.PlayNext();
            await _player.PlayNext();
            await _player.PlayNext();
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayPrevious();

            // Assert
            var lastCommand = _mediatorMock.ReceivedCalls()
                .Last(call => call.GetMethodInfo().Name == nameof(IMediator.Send));

            var lastFileLaunched = lastCommand.GetArguments()[0] as LaunchFileCommand;
            lastFileLaunched.Should().NotBeNull();
            lastFileLaunched!.LaunchItem.Id.Should().Be(first.Id);
        }

        [Fact]
        public async Task Given_MixedFileSet_When_AndFilterAll_PlayHistory_ForSpecificFilesTypes_IsMaintained()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];
            var fourth = _games[2];


            List<ILaunchableItem> items = [first, second, third, fourth];
            _player.SetPlaylist(items);

            // Act
            _player.SetFilter(TeensyFilterType.All);
            await _player.PlayItem(first);
            await _player.PlayNext();
            await _player.PlayNext();
            await _player.PlayNext();
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayPrevious();

            // Assert
            var lastCommand = _mediatorMock.ReceivedCalls()
                .Last(call => call.GetMethodInfo().Name == nameof(IMediator.Send));

            var lastFileLaunched = lastCommand.GetArguments()[0] as LaunchFileCommand;
            lastFileLaunched.Should().NotBeNull();
            lastFileLaunched!.LaunchItem.Id.Should().Be(first.Id);
        }

        [Fact]
        public async Task Given_FilesSet_When_Previous_PlaysPrevious()
        {
            // Arrange
            var playItem = _songs.First();
            var previousItem = _games.Last();
            List<ILaunchableItem> items = [previousItem, playItem];
            _player.SetPlaylist(items);
            await _player.PlayItem(playItem);

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == previousItem.Id));
        }

        [Fact]
        public async Task Given_FilesSet_And_FirstItem_When_Previous_GoesToLast()
        {
            // Arrange
            var firstItem = _songs.First();
            var lastItem = _games.First();
            var player = new Player(new ProgressTimer(), _mediatorMock, _settingsServiceMock);

            player.SetPlaylist([firstItem, lastItem]);
            await player.PlayItem(firstItem);

            // Act
            await player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == lastItem.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilterIsMusic_GoesToPreviousSong()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem(third);

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id== first.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_And_FilterIsMusic_And_OnlyOneSong_WhenPrevious_RestartsTheSong()
        {
            // Arrange
            var first = _songs[0];
            var second = _games[0];
            var third = _games[1];

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem(first);

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id== first.Id));
        }

        [Fact]
        public async Task Given_SongPlaying_WhenSongEnds_NextSongPlays()
        {
            // Arrange
            var first = _songs.First() as SongItem;
            first!.PlayLength = TimeSpan.FromSeconds(2);
            var second = _songs.Last() as SongItem;

            List<ILaunchableItem> items = [first, second];

            var player = new Player(new ProgressTimer(), _mediatorMock, _settingsServiceMock);

            player.SetPlaylist(items);

            // Act
            await player.PlayItem(first!);

            var tcs = new TaskCompletionSource<ILaunchableItem?>();
            ILaunchableItem? emittedSong = null;

            using var subscription = player.CurrentItem
                .Where(item => item is not null)
                .Skip(1)
                .Subscribe(state =>
                {
                    emittedSong = state;
                    tcs.TrySetResult(state);
                });
            await tcs.Task;

            // Assert
            emittedSong!.Id.Should().Be(second!.Id);
        }

        [Fact]
        public async Task Given_SongPlaying_And_PlayTimerSet_And_SidOverrideSet_PlayTimerSet() 
        {
            // Arrange
            var first = _songs.First() as SongItem;
            first!.PlayLength = TimeSpan.FromSeconds(2);
            var second = _songs.Last() as SongItem;
            List<ILaunchableItem> items = [first, second];

            _player.SetPlaylist(items);
            _player.SetPlayTimer(TimeSpan.FromSeconds(3));
            _player.EnableSongTimeOverride();
            
            // Act
            await _player.PlayItem(first!);
            
            // Assert
            _timerMock.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public void Given_FileSet_When_RemoveItem_RemovesItem()
        {
            // Arrange
            var first = _songs.First();
            var second = _songs.Last();
            List<ILaunchableItem> items = [first, second];
            _player.SetPlaylist(items);
            
            // Act
            _player.RemoveItem(first);
            
            // Assert
            var result = _player.GetPlaylist();
            result.Count.Should().Be(1);
            result.First().Id.Should().Be(second.Id);
        }

        [Fact]
        public async Task Given_FileSet_And_FileMissing_When_Play_ThrowsException()
        {
            // Arrange
            var first = _songs.First();
            var second = _songs.Last();
            List<ILaunchableItem> items = new() { first, second };
            _player.SetPlaylist(items);

            // Act
            Func<Task> act = async () => await _player.PlayItem("/path/to/missingFile.sid");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Given_FileSet_And_FilePresent_When_Play_IsSuccessful() 
        {
            // Arrange
            var first = _songs.First();
            var second = _songs.Last();
            List<ILaunchableItem> items = new() { first, second };
            _player.SetPlaylist(items);

            // Act
            await _player.PlayItem(first.Path);
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Any<LaunchFileCommand>());
        }
    }    
}