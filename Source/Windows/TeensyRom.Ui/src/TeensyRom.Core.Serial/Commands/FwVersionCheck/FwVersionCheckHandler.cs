using MediatR;
using TeensyRom.Core.Abstractions;

namespace TeensyRom.Core.Serial.Commands.FwVersionCheck
{
    public class FwVersionCheckHandler(IFwVersionChecker versionChecker) : IRequestHandler<FwVersionCheckCommand, FwVersionCheckResult>
    {
        public Task<FwVersionCheckResult> Handle(FwVersionCheckCommand r, CancellationToken cancellationToken)
        {
            try
            {
                //r.Serial.Write([(byte)TeensyToken.VersionCheck.Value], 0, 1);
                r.Serial.SendIntBytes(TeensyToken.Ping, 2);
                var verionResponse = r.Serial.ReadAndLogSerialAsString(200);
                var isTeensyRom = verionResponse.IsTeensyRom();
                var (isCompatible, version) = GetVersion(verionResponse);

                var versionResult = new FwVersionCheckResult
                {
                    IsSuccess = true,
                    Version = version,
                    IsTeensyRom = isTeensyRom,
                    IsCompatible = isCompatible
                };
                return Task.FromResult(versionResult);

            }
            catch (Exception)
            {
                return Task.FromResult(new FwVersionCheckResult
                {
                    IsSuccess = false,
                    Version = null,
                    IsTeensyRom = false,
                    IsCompatible = false
                });
            }
        }

        private (bool, Version?) GetVersion(string response)
        {
            if (!response.Contains("busy", StringComparison.OrdinalIgnoreCase))
            {
                return versionChecker.VersionCheck(response);
            }
            return (false, null);
        }
    }
}
