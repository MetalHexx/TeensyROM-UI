using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial
{
    public class FwVersionChecker(ILoggingService log, IAlertService alert) : IFwVersionChecker
    {
        public static readonly Version FWVersion = new(0, 6, 7);

        public static (Version? version, bool isSupported) IsVersionSupported(string input)
        {
            string pattern = @"\d+\.\d+\.\d+";
            Match match = Regex.Match(input, pattern);
            Version? parsedVersion = null;

            if (match.Success && Version.TryParse(match.Value, out parsedVersion))
            {
                return (parsedVersion, parsedVersion >= FWVersion);
            }
            return (parsedVersion, false);
        }

        public bool VersionCheck(string response)
        {
            var (version, passed) = IsVersionSupported(response);

            if (passed)
            {
                log.InternalSuccess($"Version Check Success: Found TeensyROM firmware v{version}.");
                return true;
            }
            alert.Publish($"TeensyROM firmware check failed. v{FWVersion}+ is required. (See: Terminal Logs)");
            log.InternalError($"TeensyROM firmware check failed. v{FWVersion}+ is required.");

            if (version is null)
            {
                alert.Publish("Unable to determine the version of TeensyROM. (See: Terminal Logs)");
                log.InternalError("Unable to determine the version of TeensyROM.");
            }
            else
            {
                log.InternalError($"v{version} is not supported by this app and may lead to unexpected results.");
            }
            log.InternalError("FW Download: https://github.com/SensoriumEmbedded/TeensyROM/tree/main/bin/TeensyROM");
            log.InternalError("FW Instructions: https://github.com/SensoriumEmbedded/TeensyROM/blob/main/docs/General_Usage.md#firmware-updates");

            return false;
        }
    }
}
