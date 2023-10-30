using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public class TeensyException: Exception
    {
        public TeensyException(string message) : base(message) { }
        public TeensyException(string message, Exception ex) : base(message, ex) { }
    }
}
