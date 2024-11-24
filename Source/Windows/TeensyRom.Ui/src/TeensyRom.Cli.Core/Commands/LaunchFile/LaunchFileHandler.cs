using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler(ISerialStateContext serialState, ILoggingService log, IAlertService alert) : IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        private const int _reconnectDelayMs = 1000;
        public async Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            var ack = AttemptLaunch(request);

            if (ack == TeensyToken.Ack)
            {
                return await HandleAckResponse(request);
            }
            if (ack == TeensyToken.RetryLaunch)
            {
                return await HandleRetryResponse(request);
            }
            return new() 
            { 
                IsSuccess = false,
                Error = "Failed to launch file"
            };
        }

        private TeensyToken AttemptLaunch(LaunchFileCommand request)
        {
            log.Internal($"LaunchFileHandler: Clearing serial buffers");
            serialState.ClearBuffers();

            log.Internal($"LaunchFileHandler: Sending {TeensyToken.LaunchFile} token.");
            serialState.SendIntBytes(TeensyToken.LaunchFile, 2);

            var ack = serialState.HandleAck();

            LogAck(ack);

            if (ack == TeensyToken.RetryLaunch)
            {                
                return TeensyToken.RetryLaunch;
            }

            log.Internal($"LaunchFileHandler: Sending storage token to TeensyROM");
            serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);

            log.Internal($"LaunchFileHandler: Sending {request.LaunchItem.Path} to TeensyROM");

            serialState.Write($"{request.Path}\0");
            ack = serialState.HandleAck();

            LogAck(ack);

            return ack;
        }

        private async Task<LaunchFileResult> HandleAckResponse(LaunchFileCommand request)
        {
            if (request.LaunchItem is HexItem or ImageItem)
            {
                return new() { LaunchResult = LaunchFileResultType.Success };
            }
            var response = PollResponse();

            if (response != LaunchFileResultType.Disconnected) 
            {
                return GetFinalResult(response);
            }
            await HandleReconnection(_reconnectDelayMs);
            return GetFinalResult(PollResponse());
        }

        private async Task<LaunchFileResult> HandleRetryResponse(LaunchFileCommand request) 
        {
            alert.Publish("Detected launch retry request from TeensyROM.");
            log.Internal($"LaunchFileHandler: Initiating Launch Retry of {request.LaunchItem.Name}");

            log.Internal("LaunchFileHandler: Waiting for re-connection to TeensyROM");

            log.Internal($"LaunchFileHandler: Waiting {_reconnectDelayMs}ms for TeensyROM to catch up");

            await HandleReconnection(_reconnectDelayMs);

            alert.Publish($"Attempting to re-launch {request.LaunchItem.Name}");

            AttemptLaunch(request);

            if (request.LaunchItem.Size >= 575000)
            {
                log.Internal($"LaunchFileHandler: Reconnecting again to new COM port due to large file launch retry.");
                await HandleReconnection(_reconnectDelayMs);
            }
            return GetFinalResult(PollResponse());
        }

        private LaunchFileResultType PollResponse()
        {
            try
            {
                var resultType = LaunchFileResultType.NoResponse;
                List<byte> bytesRead = [];

                for (int i = 0; i < 40; i++)
                {
                    var responseBytes = serialState.ReadSerialBytes(25);
                    bytesRead.AddRange(responseBytes);
                    resultType = ParseResponse([.. bytesRead]);

                    if (resultType != LaunchFileResultType.NoResponse)
                    {
                        return resultType;
                    }
                }
                return LaunchFileResultType.Success;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("port is closed", StringComparison.OrdinalIgnoreCase))
                {
                    return LaunchFileResultType.Disconnected;
                }
                throw;
            }
        }

        private LaunchFileResultType ParseResponse(byte[] responseBytes)
        {
            var resultString = responseBytes.ToAscii();
            var resultToCheck = resultString.Replace("Loading IO handler: TeensyROM", string.Empty);
            var foundTokens = responseBytes.FindTRTokens();

            if (foundTokens.Any(t => t == TeensyToken.GoodSIDToken))
            {
                log.External(resultString);
                return LaunchFileResultType.Success;
            }
            if (foundTokens.Any(t => t == TeensyToken.BadSIDToken))
            {
                log.ExternalError($"LaunchFileHandler: Failed to launch sid: \r\n{resultString}");
                return LaunchFileResultType.SidError;
            }
            if (resultString.Contains("Loading IO handler:", StringComparison.OrdinalIgnoreCase))
            {
                log.External(resultString);
                return LaunchFileResultType.Success;
            }
            var programError = new[] { "Not enough room", "Unsupported HW Type" };

            if (programError.Any(error => resultString.Contains(error, StringComparison.OrdinalIgnoreCase)))
            {
                log.ExternalError($"LaunchFileHandler: Failed to launch program: \r\n{resultString}");
                return LaunchFileResultType.ProgramError;
            }
            return LaunchFileResultType.NoResponse;
        }

        private async Task HandleReconnection(int waitMs)
        {
            alert.Publish("TeensyROM is rebooting to switch modes.");
            await Task.Delay(waitMs);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    serialState.EnsureConnection();
                    return;
                }
                catch (TeensyException)
                {
                    if (i == 2) throw;
                }
            }
        }

        private LaunchFileResult GetFinalResult(LaunchFileResultType resultType) 
        {
            return resultType switch
            {
                LaunchFileResultType.Success => new() { LaunchResult = LaunchFileResultType.Success },
                LaunchFileResultType.SidError => new() { IsSuccess = false, LaunchResult = LaunchFileResultType.SidError },
                LaunchFileResultType.ProgramError => new() { IsSuccess = false, LaunchResult = LaunchFileResultType.ProgramError },
                LaunchFileResultType.NoResponse => new() { IsSuccess = false, LaunchResult = LaunchFileResultType.NoResponse },
                _ => new() { LaunchResult = LaunchFileResultType.Success },
            };
        }

        private void LogAck(TeensyToken ack) 
        {
            if (ack == TeensyToken.Ack || ack == TeensyToken.RetryLaunch)
            {
                log.Internal($"LaunchFileHandler: Received {ack} token from TeensyROM");
                return;
            }
            log.InternalError($"LaunchFileHandler: Received {ack} token from TeensyROM");
        }
    }
}