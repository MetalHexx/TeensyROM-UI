using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class CopyFileCommandTests : IDisposable
    {
        private readonly TeensyFixture _fixture;

        public CopyFileCommandTests()
        {
            _fixture = new TeensyFixture();
        }

        [Fact]
        public async Task Given_FileExists_When_CommandCalled_FileCopiedSuccessfully()
        {
            //Arrange
            var testId = Guid.NewGuid().ToString().Substring(0, 5);
            var sourceFilePath = "/integration-test-files/test-existing-folder/test-file.sid";
            var destParentPath = $"/integration-test-files/copy-dir-{testId}/";
            var destFilePath = $"{destParentPath}test-file-{testId}.sid";

            _fixture.Initialize(initOpenPort: true);

            //Act
            await _fixture.Mediator.Send(new CopyFileRequest(sourceFilePath, destFilePath));
            Thread.Sleep(100);
            var directory = _fixture.GetDirectoryContentCommand.Execute(destParentPath, 0, 1);

            //Assert
            directory.Files.First().Path.Should().Be(destFilePath);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }
    }
}
