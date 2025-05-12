using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial
{
    public class FwVersionChecker(ILoggingService log, IAlertService alert) : IFwVersionChecker
    {
        public static readonly Version FullRomVersion = new(0, 6, 6);
        public static readonly Version MinimalRomVersion = new(0, 0, 2);

        public (bool IsTeensyRom, bool? IsMinimal, bool? isVersionCompatible, Version? Version) GetAllVersionInfo(ISerialStateContext serial)
        {   
            serial.Write([(byte)TeensyToken.VersionCheck.Value], 0, 1);
            var versionResponse = serial.ReadAndLogSerialAsString(200);

            if (versionResponse.Contains("busy", StringComparison.OrdinalIgnoreCase))
            {
                return (true, null, null, null);
            }
            var isTeensyRom = versionResponse.IsTeensyRom();

            var (isCompatible, version) = VersionCheck(versionResponse);

            var isMinimal = versionResponse.Contains("minimal", StringComparison.OrdinalIgnoreCase);

            return (isTeensyRom, isMinimal, isCompatible, version);

        }


        public static (Version? version, bool isSupported) IsVersionSupported(string input)
        {
            string pattern = @"\d+\.\d+\.\d+";
            Match match = Regex.Match(input, pattern);
            Version? parsedVersion = null;

            if (match.Success && Version.TryParse(match.Value, out parsedVersion))
            {
                return input.Contains("minimal")
                    ? (parsedVersion, parsedVersion >= MinimalRomVersion)
                    : (parsedVersion, parsedVersion >= FullRomVersion);
            }
            return (parsedVersion, false);
        }

        public (bool, Version?) VersionCheck(string response)
        {
            var (version, passed) = IsVersionSupported(response);

            if (passed)
            {
                log.InternalSuccess($"Version Check Success: Found TeensyROM firmware v{version}.");
                return (true, version);
            }
            alert.Publish($"TeensyROM firmware check failed. v{FullRomVersion}+ is required. (See: Terminal Logs)");
            log.InternalError($"TeensyROM firmware check failed. v{FullRomVersion}+ is required.");

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

            return (false,version);
        }
    }
}
