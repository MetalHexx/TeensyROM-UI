//using FluentAssertions;
//using System.Reactive.Threading.Tasks;
//using TeensyRom.Ui.Core.Storage.Entities;

//namespace TeensyRom.Tests.Integration
//{
//    [Collection("SerialPortTests")]
//    public class DirectoryTests : IDisposable
//    {
//        private readonly TeensyFixture _fixture;
//        public DirectoryTests()
//        {
//            _fixture = new TeensyFixture();
//        }

//        [Fact]
//        public void Given_DefaultRootPathExists_When_FileTransferViewLoads_Then_CurrentDirectoryIsDefault()
//        {
//            //Arrange
//            _fixture.Initialize();
//            //Assert
//            _fixture.FileTransferViewModel.CurrentPath.Should().Be("/");
//        }

//        [Fact]
//        public async Task Given_DirectoriesLoaded_When_DirectoryClicked_Then_CurrentDirectoryChangesToNewPath()
//        {
//            //Arrange
//            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Root_Path;
//            _fixture.Initialize();
//            var directoryToSwitchTo = new DirectoryItem { Path = TestConstants.Integration_Test_Existing_Folder };
//            Thread.Sleep(1000);

//            //Act
//            await _fixture.FileTransferViewModel.LoadDirectoryContentCommand.Execute(directoryToSwitchTo).ToTask();

//            //Assert
//            _fixture.FileTransferViewModel.CurrentPath.Should().Be(TestConstants.Integration_Test_Existing_Folder);
//        }

//        [Fact]
//        public async Task Given_NotInRootPath_When_NavigatingUp_The_ParentFolderContentFetched()
//        {
//            //Arrange
//            _fixture.Settings.TargetRootPath = TestConstants.Integration_Test_Existing_Folder;
//            _fixture.Initialize();
//            Thread.Sleep(100);


//            //Assert
//            await _fixture.FileTransferViewModel.LoadParentDirectoryContentCommand.Execute().ToTask();
//            _fixture.FileTransferViewModel.CurrentPath.Should().Be(TestConstants.Integration_Test_Root_Path);

//        }

//        [Fact]
//        public async Task Given_TargetEmptyDirectoryExists_When_NavigatingToDirectory_IsTargetEmptyTrue()
//        {
//            //Arrange
//            var directoryToSwitchTo = new DirectoryItem { Path = TestConstants.Integration_Test_Existing_Empty_Folder };
//            _fixture.Initialize();
//            Thread.Sleep(1000);

//            //Act            
//            await _fixture.FileTransferViewModel.LoadDirectoryContentCommand.Execute(directoryToSwitchTo).ToTask();

//            //Assert
//            _fixture.FileTransferViewModel.TargetItems.Count.Should().Be(0);

//        }

//        public void Dispose()
//        {
//            _fixture.Dispose();

//            if (File.Exists(_fixture.SettingsFileName))
//            {
//                File.Delete(_fixture.SettingsFileName);
//            }
//        }
//    }
//}
