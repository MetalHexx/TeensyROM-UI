using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Common
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan GetTimeFromPercent(this TimeSpan totalTime, double percentage)
        {
            return TimeSpan.FromTicks((long)(totalTime.Ticks * (percentage)));
        }
    }
}
