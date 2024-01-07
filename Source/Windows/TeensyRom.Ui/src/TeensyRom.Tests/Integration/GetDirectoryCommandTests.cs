using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Entities;

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
        public async Task Give_FilesExist_ReturnsFiles()
        {


            // Assert
            _fixture.Initialize(initOpenPort: true);
            var response = await _fixture.Mediator.Send(new GetDirectoryCommand
            {
                Path = "/libraries/programs/oneload64"
            });

        }
    }
}
