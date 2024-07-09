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
            _serialState.ClearBuffers();
            _serialState.SendIntBytes(TeensyToken.LaunchFile, 2);
            _serialState.HandleAck();
            _serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            _serialState.Write($"{request.LaunchItem.Path}\0");
            _serialState.HandleAck();

            if (request.LaunchItem is HexItem or ImageItem) 
            {
                return new LaunchFileResult
                {
                    LaunchResult = LaunchFileResultType.Success
                };
            }

            var resultType = PollResponse();

            return new LaunchFileResult
            {
                LaunchResult = resultType
            };
        }

        private LaunchFileResultType PollResponse()
        {
            var resultType = LaunchFileResultType.NoResponse;
            List<byte> bytesRead = [];
            
            for (int i = 0; i < 40; i++)
            {
                var responseBytes = _serialState.ReadSerialBytes(25);
                bytesRead.AddRange(responseBytes);
                resultType = ParseResponse([.. bytesRead]);

                if (resultType != LaunchFileResultType.NoResponse)
                {
                    return resultType;
                }
            }
            return LaunchFileResultType.Success;
        }

        private LaunchFileResultType ParseResponse(byte[] responseBytes)
        {   
            var resultString = responseBytes.ToAscii();
            var resultToCheck = resultString.Replace("Loading IO handler: TeensyROM", string.Empty);
            var foundTokens = responseBytes.FindTRTokens();
            
            if (foundTokens.Any(t => t == TeensyToken.GoodSIDToken))
            {
                _log.External(resultString);
                return LaunchFileResultType.Success;
            }
            if (foundTokens.Any(t => t == TeensyToken.BadSIDToken))
            {
                _log.ExternalError($"Failed to launch sid: \r\n{resultString}");
                return LaunchFileResultType.SidError;
            }
            if (resultString.Contains("Loading IO handler:", StringComparison.OrdinalIgnoreCase))
            {
                _log.External(resultString);
                return LaunchFileResultType.Success;
            }
            var programError = new[] { "Not enough room", "Unsupported HW Type" };

            if (programError.Any(error => resultString.Contains(error, StringComparison.OrdinalIgnoreCase)))
            {
                _log.ExternalError($"Failed to launch program: \r\n{resultString}");
                return LaunchFileResultType.ProgramError;
            }
            return LaunchFileResultType.NoResponse;
        }
    }
}