using FluentAssertions;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using Xunit;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class DeleteFileCommandTests
    {
        private readonly TeensyFixture _fixture;

        public DeleteFileCommandTests()
        {
            _fixture = new TeensyFixture();
        }

        [Fact]
        public async Task Given_FileExists_When_CommandCalled_FileDeletedSuccessfully()
        {
            // Arrange
            _fixture.Initialize(initOpenPort: true);
            var testFile = _fixture.CreateTeensyFileInfo(TeensyFileType.Sid, TeensyFixture.IntegrationTestPath, TeensyStorageType.SD);
            await _fixture.Mediator.Send(new SaveFileCommand { File = testFile });

            // Act
            var delResult = await _fixture.Mediator.Send(new DeleteFileCommand 
            { 
                Path = testFile.TargetPath.UnixPathCombine(testFile.Name) 
            });

            // Assert
            var response = await _fixture.Mediator.Send(new GetDirectoryCommand
            {
                Path = testFile.TargetPath
            });
            response.DirectoryContent!.Files.Should().NotContain(f => f.Name == testFile.Name);
        }
    }
}
