using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Cli.Core.Common
{
    public class TeensyException: Exception
    {
        public TeensyException(string message) : base(message) { }
        public TeensyException(string message, Exception ex) : base(message, ex) { }
    }

    public class TeensyBusyException(string message) : TeensyException(message) { }
    public class TeensyDuplicateException(string message) : TeensyException(message) { }

    public class TeensyStateException(string message) : Exception(message) { }
}
