using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Logging
{
    public static class LogConstants
    {
        public const string LogPath = @"Assets\System\Logs\";
        public const string LogFileName = "Logs-";
        public const string LogFileExtention = ".txt";
        public const string UnhandedErrorLogPath = @"Assets\System\Logs\";
        public const string UnhandledLogFileName = "UnhandledErrorLogs-";
        public const string UnhandledLogFileExtention = ".txt";
        public const string InternalColor = "#b39ddb"; //lavendar
        public const string InternalSuccessColor = "#86c691"; //green
        public const string InternalErrorColor = "#cc666c"; //soft red
        public const string ExternalColor = "#7FDBD6"; //teensy blue
        public const string ExternalSuccessColor = "#86c691"; //green
        public const string ExternalErrorColor = "#cc666c"; //soft red

    }
}
