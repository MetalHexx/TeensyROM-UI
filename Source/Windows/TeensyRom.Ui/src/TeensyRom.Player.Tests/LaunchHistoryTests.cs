using AutoFixture;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Player.Tests
{
    public class LaunchHistoryTests
    {
        private ILaunchHistory _history = new LaunchHistory();
        private List<ILaunchableItem> _songs = [];
        private List<ILaunchableItem> _games = [];
        private Fixture _dataGenerator = new();
        public LaunchHistoryTests()
        {
            _songs = _dataGenerator
                .Build<SongItem>()
                .With(s => s.PlayLength, TimeSpan.FromSeconds(30))
                .With(s => s.Path, $"/path/{Guid.NewGuid().ToString()}.sid")
                .CreateMany(10)
                .Select(s => s as ILaunchableItem)
                .ToList();

            _games = _dataGenerator
                .Build<GameItem>()
                .With(g => g.PlayLength, TimeSpan.FromSeconds(30))
                .With(g => g.Path, $"/path/{Guid.NewGuid().ToString()}.crt")
                .CreateMany(10)
                .Select(g => g as ILaunchableItem)
                .ToList();

        }

        [Fact]
        public void When_ItemsAdded_History_ContainsExpectedItems()
        {
            // Arrange
            var expectedItems = _songs;

            // Act
            foreach (var item in expectedItems)
            {
                _history.Add(item);
            }
            
            var actualItems = _history.GetHistory();
            
            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public void When_ItemsAdded_NextAndPrevious_ReturnsExpectedItems()
        {
            // Arrange
            var expectedNext = _songs[0];
            var expectedPrevious = _songs[1];

            // Act
            _history.Add(expectedPrevious);
            _history.Add(expectedNext); 

            var actualPrevious = _history.GetPrevious();
            var actualNext = _history.GetNext();

            // Assert
            actualPrevious!.Id.Should().Be(expectedPrevious.Id);
            actualNext!.Id.Should().Be(expectedNext.Id);
        }

        [Fact]
        public void When_ItemsAddedAndRemoved_NextAndPrevious_ReturnsExpectedItems()
        {
            // Arrange
            var expectedNext = _songs[0];
            var itemToRemove = _songs[1];
            var expectedPrevious = _songs[2];

            // Act
            _history.Add(expectedPrevious);
            _history.Add(itemToRemove);
            _history.Add(expectedNext);
            _history.Remove(itemToRemove);

            var actualPrevious = _history.GetPrevious();
            var actualNext = _history.GetNext();

            // Assert
            actualPrevious!.Id.Should().Be(expectedPrevious.Id);
            actualNext!.Id.Should().Be(expectedNext.Id);
        }

        [Fact]
        public void When_ItemsAddedAndRemoved_GetHistory_ReturnsExpectedItems() 
        {
            // Arrange
            var item1 = _songs[0];
            var item2 = _songs[1];
            var expectedItems = new List<ILaunchableItem> { item2 };

            // Act
            _history.Add(item1);
            _history.Add(item2);
            _history.Remove(item1);

            var actualItems = _history.GetHistory();

            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public void When_ItemsAddedAndCleared_NextAndPrevious_ReturnsNull()
        {
            // Arrange
            var expectedNext = _songs[0];
            var expectedPrevious = _songs[1];

            // Act
            _history.Add(expectedNext);
            _history.Add(expectedPrevious);
            _history.Clear();
            var actualPrevious = _history.GetPrevious();
            var actualNext = _history.GetNext();
            
            // Assert
            actualPrevious.Should().BeNull();
            actualNext.Should().BeNull();
        }

        [Fact]
        public void When_ItemsAddedAndCleared_GetHistory_ReturnsNull() 
        {
            // Arrange
            var item1 = _songs[0];
            var item2 = _songs[1];

            // Act
            _history.Add(item1);
            _history.Add(item2);
            _history.Clear();
            var actualItems = _history.GetHistory();
            
            // Assert
            actualItems.Should().BeEmpty();
        }

        [Fact]
        public void Given_ItemsExist_And_OnItemOne_When_ForwardHistoryCleared_NextReturnsNull() 
        {
            // Arrange
            var firstItem = _songs[0];
            var nextItem = _songs[1];

            // Act
            _history.Add(firstItem);
            _history.Add(nextItem);
            _history.GetPrevious();
            _history.ClearForwardHistory();
            var actualNext = _history.GetNext();

            // Assert
            actualNext.Should().BeNull();
        }

        [Fact]
        public void Given_ItemsExist_When_ForwardHistoryCleared_GetHistory_ReturnsExpectedItems()
        {
            // Arrange
            var firstItem = _songs[0];
            var nextItem = _songs[1];
            var expectedItems = new List<ILaunchableItem> { firstItem };

            // Act
            _history.Add(firstItem);
            _history.Add(nextItem);
            _history.GetPrevious();
            _history.ClearForwardHistory();
            var actualItems = _history.GetHistory();
            
            // Assert
            actualItems.Should().BeEquivalentTo(expectedItems);
        }

        [Fact]
        public void Given_ItemsAdded_WhenCleared_NextAndPrevious_Return_Null()
        {
            // Arrange
            var item1 = _songs[0];
            var item2 = _songs[1];

            // Act
            _history.Add(item1);
            _history.Add(item2);
            _history.Clear();
            var actualPrevious = _history.GetPrevious();
            var actualNext = _history.GetNext();
            
            // Assert
            actualPrevious.Should().BeNull();
            actualNext.Should().BeNull();
        }

        [Fact]
        public void Given_ItemAdded_Then_CurrentIsNewIsTrue()
        {
            // Arrange
            var item = _songs[0];

            // Act
            _history.Add(item);
            
            // Assert
            _history.CurrentIsNew.Should().BeTrue();
        }

        [Fact]
        public void Given_ItemAddedAndRemoved_Then_CurrentIsNewIsFalse()
        {
            // Arrange
            var item = _songs[0];
                        
            // Act
            _history.Add(item);
            _history.Remove(item);
            
            // Assert
            _history.CurrentIsNew.Should().BeFalse();
        }

        [Fact]
        public void Given_ItemAdded_AndPrevious_CurrentItemIsNotNew() 
        {
            // Arrange
            var item = _songs[0];
            var item2 = _songs[1];

            // Act
            _history.Add(item);
            _history.Add(item2);
            _history.GetPrevious();

            // Assert
            _history.CurrentIsNew.Should().BeFalse();
        }

        [Fact]
        public void Given_MixedFileTypes_When_Filtered_Next_ReturnsExpectedItem() 
        {
            // Arrange
            var song1 = _songs[0];
            var song2 = _songs[1];
            var game1 = _games[0];
            var game2 = _games[1];
            var expectedNext = game1;

            // Act
            _history.Add(song1);
            _history.Add(song2);
            _history.Add(game1);
            _history.Add(game2);
            _history.GetPrevious();
            _history.GetPrevious();
            _history.GetPrevious();
            var actualNext = _history.GetNext(TeensyFileType.Crt);
            
            // Assert
            actualNext!.Id.Should().Be(expectedNext.Id);
        }

        [Fact]
        public void Given_MixedFileTypes_When_Filtered_Previous_ReturnsExpectedItem()
        {
            // Arrange
            var expectedPrevious = _songs[0];
            var game1 = _games[0];
            var game2 = _games[1];

            // Act
            _history.Add(expectedPrevious);            
            _history.Add(game1);
            _history.Add(game2);

            var actualPrevious = _history.GetPrevious(TeensyFileType.Sid);

            // Assert
            actualPrevious!.Id.Should().Be(expectedPrevious.Id);
        }

        [Fact]
        public void UltimateTest_AllOperations_FinalStateIsExpected_MiddleOfSet()
        {
            // Arrange
            var firstSong = _songs[0];
            var secondSong = _songs[1];
            var thirdSong = _songs[2];
            var firstGame = _games[0];
            var secondGame = _games[1];
            var thirdGame = _games[2];
            var expectedHistory = new List<ILaunchableItem> { firstSong, thirdSong, firstGame, secondGame };
            var expectedPrevious = secondGame;
            var expectedNext = thirdSong;

            // Act
            _history.Add(firstSong);
           //_history.Add(secondSong);
            _history.Add(thirdSong);
            _history.Add(firstGame);
            _history.Add(secondGame);
            //_history.Add(thirdGame);
            _history.Remove(secondSong);
            _history.Remove(thirdGame);
            _history.GetPrevious();
            _history.GetPrevious();
            _history.GetPrevious();
            _history.GetNext();
            _history.GetNext();
            _history.GetNext();
            _history.ClearForwardHistory();
            var finalHistory = _history.GetHistory();
            var finalPrevious = _history.GetPrevious();
            var finalNext = _history.GetNext();

            // Assert
            finalHistory.Should().BeEquivalentTo(expectedHistory);
            finalPrevious!.Id.Should().Be(expectedPrevious.Id);
            finalNext!.Id.Should().Be(expectedNext.Id);
        }
    }
}
