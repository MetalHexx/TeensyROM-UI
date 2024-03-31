using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler: IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        private readonly ISerialStateContext _serialState;
        private readonly ILoggingService _log;

        public LaunchFileHandler(ISerialStateContext serialState, ILoggingService log)
        {
            _serialState = serialState;
            _log = log;
        }

        public async Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.LaunchFile, 2);
            _serialState.HandleAck();
            _serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{request.Path}\0");
            _serialState.HandleAck();
            await Task.Delay(400);
            var response = _serialState.ReadSerialAsString(400);
            var resultType = ParseResponse(response);

            return new LaunchFileResult
            {
                LaunchResult = resultType
            };
        }

        private LaunchFileResultType ParseResponse(string response)
        {
            var sidError = new[] { "PSID not found", "Mem conflict w/ TR app", "PSID/RSID not found", "IO1 mem conflict", "Unexpected Version", "Unexpected Data Offset" };
            var programError = new[] { "Not enough room", "Unsupported HW Type" };

            if (sidError.Any(response.Contains))
            {
                _log.ExternalError($"Failed to launch sid: \r\n{response}");
                return LaunchFileResultType.SidError;
            }
            if (programError.Any(response.Contains))
            {
                _log.ExternalError($"Failed to launch program: \r\n{response}");
                return LaunchFileResultType.ProgramError;
            }
            _log.External(response);
            return LaunchFileResultType.Success;
        }
    }
}