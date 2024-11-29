using MediatR;
using Microsoft.VisualBasic;
using System.Reactive.Linq;
using TeensyRom.Core.Commands.LaunchFile;
using TeensyRom.Core.Commands.Reset;
using TeensyRom.Core.Commands.ToggleMusic;
using TeensyRom.Core.Progress;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Player.Tests
{
    public class PlayerTests
    {
        private readonly Player _player;
        private readonly IMediator _mediatorMock;
        private readonly IProgressTimer _timerMock;
        private readonly ISettingsService _settingsServiceMock;
        public PlayerTests()
        {
            _mediatorMock = Substitute.For<IMediator>();
            _mediatorMock
                .Send(Arg.Any<LaunchFileCommand>())
                .Returns(new LaunchFileResult { LaunchResult = LaunchFileResultType.Success });
            _timerMock = Substitute.For<IProgressTimer>();
            _settingsServiceMock = Substitute.For<ISettingsService>();
            _settingsServiceMock.GetSettings().Returns(new TeensySettings());
            _player = new Player(_timerMock, _mediatorMock, _settingsServiceMock);
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
            var items = new List<ILaunchableItem> { new SongItem(), new SongItem() };
            _player.SetPlaylist(items);

            //Act
            var result = _player.GetPlaylist();

            //Assert
            result.ForEach(item => items.Any(i => item.Id == i.Id));
            result.Count.Should().Be(items.Count);

        }

        [Fact]
        public void Given_PlaylistSet_WhenNewItemsSet_ReturnsItems()
        {
            //Arrange
            var oldItems = new List<ILaunchableItem> { new SongItem(), new SongItem() };
            var newItems = new List<ILaunchableItem> { new SongItem(), new SongItem() };

            _player.SetPlaylist(oldItems);
            _player.SetPlaylist(newItems);

            //Act
            var result = _player.GetPlaylist();

            //Assert
            result.ForEach(item => newItems.Any(i => item.Id == i.Id));
            result.Count.Should().Be(newItems.Count);

        }

        [Fact]
        public void Given_ItemsSet_When_ItemSet_ReturnsItem()
        {
            //Arrange
            var items = new List<ILaunchableItem> { new SongItem(), new SongItem() };
            _player.SetPlaylist(items);
            
            //Act
            _player.SetPlayItem(items.First());
            var result = _player.GetPlaylist();
            
            //Assert
            result.First().Id.Should().Be(items.First().Id);
        }

        [Fact]
        public async Task Given_ItemsSet_When_ItemSet_EmitsItem() 
        {
            //Arrange
            var items = new List<ILaunchableItem> { new SongItem(), new SongItem() };
            _player.SetPlaylist(items);
            ILaunchableItem? emittedItem = null;
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            using var subscription = _player.CurrentItem.Skip(1).Subscribe(item =>
            {
                emittedItem = item;
                tcs.TrySetResult(item!);
            });

            //Act
            _player.SetPlayItem(items.First());
            await tcs.Task;

            //Assert
            emittedItem.Should().Be(items.First());
        }

        [Fact]
        public async Task Given_NoItemSet_When_Played_ThrowsException()
        {
            // Act
            Exception exception = null!;

            try
            {
                await _player.PlayItem();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            exception.Should().NotBeNull();
            exception.Should().BeOfType<Exception>();
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
            var playItem = new SongItem();
            _player.SetPlaylist([playItem]);
            _player.SetPlayItem(playItem);

            // Act
            await _player.PlayItem();
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Playing);
        }

        [Fact]
        public async Task Given_Stopped_When_Play_Then_EmitsCurrentTimeZero()
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);

            TimeSpan? emittedTime = null;

            var tcs = new TaskCompletionSource<TimeSpan>();
            using var subscription = _player.CurrentTime.Subscribe(state =>
            {
                emittedTime = state;
                tcs.TrySetResult(state);
            });

            // Act
            _player.PlayItem();
            await tcs.Task;

            // Assert
            emittedTime.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public async Task Given_Playing2Seconds_Then_EmitsTimer2Seconds()
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            var player = new Player(new ProgressTimer(), _mediatorMock, _settingsServiceMock);

            player.SetPlaylist(items);
            player.SetPlayItem(playItem);
            await player.PlayItem();

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
            var playItem = new SongItem { PlayLength = TimeSpan.FromSeconds(5) };

            // Act
            _player.SetPlaylist([playItem]);
            _player.SetPlayItem(playItem);
            await _player.PlayItem();

            //Assert
            _timerMock.Received().StartNewTimer(playItem.PlayLength);
        }

        [Fact]
        public async Task Given_NonSongSet_AndPlayTimerNull_When_Played_TimerNotSet()
        {
            // Arrange
            var playItem = new GameItem();

            // Act
            _player.SetPlaylist([playItem]);
            _player.SetPlayItem(playItem);
            await _player.PlayItem();

            //Assert
            _timerMock.DidNotReceive().StartNewTimer(playItem.PlayLength);
        }

        [Fact]
        public void Given_NonSongSet_AndPlayTimerNotNull_When_Played_TimerSet()
        {
            // Arrange
            var mockTimer = Substitute.For<IProgressTimer>();
            var player = new Player(mockTimer, _mediatorMock, _settingsServiceMock);
            var playItem = new GameItem();            
            player.SetPlaylist([playItem]);
            player.SetPlayItem(playItem);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));

            // Act                     
            player.PlayItem();

            //Assert
            mockTimer.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public void Given_SongSet_And_PlayTimerSet_And_SongTimerOverrideSet_When_Played_TimerSetToPlayTimer() 
        {
            // Arrange
            var playItem = new SongItem { PlayLength = TimeSpan.FromSeconds(5) };
            var mockTimer = Substitute.For<IProgressTimer>();
            var player = new Player(mockTimer, _mediatorMock, _settingsServiceMock);

            // Act
            player.SetPlaylist([playItem]);
            player.SetPlayItem(playItem);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));
            player.EnableSongTimeOverride();
            player.PlayItem();

            //Assert
            mockTimer.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public void Given_SongSet_And_PlayTimerSet_And_SongTimerOverrideSet_When_Played_TimerSetToSongTime()
        {
            // Arrange
            var playItem = new SongItem { PlayLength = TimeSpan.FromSeconds(5) };
            var mockTimer = Substitute.For<IProgressTimer>();
            var player = new Player(mockTimer, _mediatorMock, _settingsServiceMock);

            // Act
            player.SetPlaylist([playItem]);
            player.SetPlayItem(playItem);
            player.SetPlayTimer(TimeSpan.FromSeconds(3));
            player.EnableSongTimeOverride();
            player.PlayItem();

            //Assert
            mockTimer.Received().StartNewTimer(TimeSpan.FromSeconds(3));
        }

        [Fact]
        public async Task Given_Paused_When_PlayCalled_EmitsPlaying()
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            _player.Pause();

            PlayerState? emittedState = null;
            var tcs = new TaskCompletionSource<PlayerState>();
            using var subscription = _player.PlayerState.Subscribe(state =>
            {
                emittedState = state;
                tcs.TrySetResult(state);
            });

            // Act

            _player.PlayItem();
            await tcs.Task;

            // Assert
            emittedState.Should().Be(PlayerState.Playing);
        }

        [Fact]
        public async Task Given_Paused_And_MusicPlaying_When_PlayCalled_Sends_ToggleMusicCommand() 
        {
            // Arrange         
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            var player = new Player(_timerMock, _mediatorMock, _settingsServiceMock);
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            _player.Pause();

            // Act
            _player.PlayItem();            

            // Assert
            await _mediatorMock.Received().Send(Arg.Any<ToggleMusicCommand>());
        }        

        [Fact]
        public async Task Given_Stopped_When_PlayedCalled_Sends_LaunchFileCommand()
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);

            // Act
            _player.PlayItem();
            
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
        public async Task Given_Stopped_SendsResetCommand() 
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            _player.PlayItem();

            // Act
            _player.Stop();

            // Assert
            await _mediatorMock.Received().Send(Arg.Any<ResetCommand>());
        }

        [Fact]
        public void Given_PlayingMusic_When_Paused_PausesTimer() 
        {
            // Arrange
            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            _player.PlayItem();

            // Act
            _player.Pause();
            
            // Assert
            _timerMock.Received().PauseTimer();
        }

        [Fact]
        public async Task Given_BadSid_WhenPlayed_EmitsBadFile() 
        {
            // Arrange
            var mediatorMock = Substitute.For<IMediator>();
            mediatorMock
                .Send(Arg.Any<LaunchFileCommand>())
                .Returns(new LaunchFileResult { LaunchResult = LaunchFileResultType.SidError });

            var player = new Player(_timerMock, mediatorMock, _settingsServiceMock);

            var playItem = new SongItem();
            List<ILaunchableItem> items = [playItem, new SongItem()];
            player.SetPlaylist(items);
            player.SetPlayItem(playItem);

            ILaunchableItem? emittedItem = null;
            var tcs = new TaskCompletionSource<ILaunchableItem>();
            using var subscription = player.BadFile.Subscribe(item =>
            {
                emittedItem = item;
                tcs.TrySetResult(item);
            });

            // Act
            await player.PlayItem();

            // Assert
            emittedItem.Should().Be(playItem);
        }

        [Fact] 
        public async Task Given_FilesSet_When_Next_PlaysNext()
        {
            // Arrange
            var playItem = new SongItem();
            var nextItem = new GameItem();
            List<ILaunchableItem> items = [playItem, nextItem];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            await _player.PlayItem();
            
            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == nextItem.Id));
        }

        [Fact]
        public async Task Given_FilesSet_And_LastItem_When_Next_GoesToFirst() 
        {
            // Arrange
            var playItem = new SongItem();
            var nextItem = new GameItem();
            List<ILaunchableItem> items = [nextItem, playItem];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            await _player.PlayItem();

            // Act
            await _player.PlayNext();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == nextItem.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilterIsMusic_GoesToNextSong() 
        {
            // Arrange
            var first = new SongItem();
            var second = new GameItem();
            var third = new SongItem();

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetPlayItem(first);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem();

            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == third.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_And_FilterIsMusic_And_OnlyOneSong_WhenNext_RestartsTheSong() 
        {
            // Arrange
            var first = new SongItem();
            var second = new GameItem();
            var third = new GameItem();

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetPlayItem(first);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem();

            // Act
            await _player.PlayNext();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == first.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilerIsMusic_And_LastSong_GoesToFirstSong()
        {
            // Arrange
            var first = new SongItem();
            var second = new GameItem();
            var third = new SongItem();

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetPlayItem(third);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem();
            
            // Act
            await _player.PlayNext();
            
            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == first.Id));
        }

        [Fact]
        public async Task Given_FilesSet_When_Previous_PlaysPrevious()
        {
            // Arrange
            var playItem = new SongItem();
            var previousItem = new GameItem();
            List<ILaunchableItem> items = [previousItem, playItem];
            _player.SetPlaylist(items);
            _player.SetPlayItem(playItem);
            await _player.PlayItem();

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == previousItem.Id));
        }

        [Fact]
        public async Task Given_FilesSet_And_FirstItem_When_Previous_GoesToLast()
        {
            // Arrange
            var firstItem = new SongItem();
            var lastItem = new GameItem();
            List<ILaunchableItem> items = [firstItem, lastItem];
            _player.SetPlaylist(items);
            _player.SetPlayItem(firstItem);
            await _player.PlayItem();

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == lastItem.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_When_FilterIsMusic_GoesToPreviousSong()
        {
            // Arrange
            var first = new SongItem();
            var second = new GameItem();
            var third = new SongItem();

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetPlayItem(third);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem();

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == first.Id));
        }

        [Fact]
        public async Task Given_MixedFileSet_And_FilterIsMusic_And_OnlyOneSong_WhenPrevious_RestartsTheSong()
        {
            // Arrange
            var first = new SongItem();
            var second = new GameItem();
            var third = new GameItem();

            List<ILaunchableItem> items = [first, second, third];
            _player.SetPlaylist(items);
            _player.SetPlayItem(first);
            _player.SetFilter(TeensyFilterType.Music);
            await _player.PlayItem();

            // Act
            await _player.PlayPrevious();

            // Assert
            await _mediatorMock.Received().Send(Arg.Is<LaunchFileCommand>(command => command.LaunchItem.Id == first.Id));
        }
    }
}