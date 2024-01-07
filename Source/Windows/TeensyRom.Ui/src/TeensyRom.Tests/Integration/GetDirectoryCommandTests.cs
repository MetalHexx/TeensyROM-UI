using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;
using FluentAssertions;
using System.Diagnostics;

namespace TeensyRom.Tests.Integration
{
    [Collection("SerialPortTests")]
    public class GetDirectoryCommandTests
    {
        private readonly TeensyFixture _fixture;

        public GetDirectoryCommandTests()
        {
            _fixture = new TeensyFixture();
        }

        [Fact]
        public async Task Given_OneLoad64FilesExist_ReturnsFiles()
        {
            // Arrange
            _fixture.Initialize(initOpenPort: true);

            // Act
            var response = await _fixture.Mediator.Send(new GetDirectoryCommand
            {
                Path = "/libraries/programs/oneload64"
            });

            // Assert
            response.DirectoryContent!.TotalCount.Should().BeGreaterThan(0);
        }
    }
}
