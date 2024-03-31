//using FluentAssertions;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using TeensyRom.Core.Commands;
//using TeensyRom.Core.Commands.GetFile;
//using TeensyRom.Core.Storage.Entities;

//namespace TeensyRom.Tests.Integration
//{
//    [Collection("SerialPortTests")]
//    public class GetFileCommandTests
//    {
//        private readonly TeensyFixture _fixture;

//        public GetFileCommandTests()
//        {
//            _fixture = new TeensyFixture();
//        }

//        [Fact]
//        public async Task Given_OneloadInstalled_WhenFileFetch_ThenHasBytes()
//        {
//            // Arrange
//            _fixture.Initialize(initOpenPort: true);

//            // Act
//            var response = await _fixture.Mediator.Send(new GetFileCommand
//            {
//                FilePath = "/libraries/programs/oneload64/Extras/Images/LoadingScreens/10th Frame.png",
//                StorageType = TeensyStorageType.SD
//            });

//            // Assert
//            response.FileData!.Length.Should().BeGreaterThan(0);
//        }
//    }
//}
