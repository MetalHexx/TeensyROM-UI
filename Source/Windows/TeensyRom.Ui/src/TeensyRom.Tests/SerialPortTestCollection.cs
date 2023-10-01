using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Tests
{
    [CollectionDefinition("SerialPortTests", DisableParallelization = true)]
    public class SerialPortTestCollection : ICollectionFixture<SerialPortTests> { }
}
