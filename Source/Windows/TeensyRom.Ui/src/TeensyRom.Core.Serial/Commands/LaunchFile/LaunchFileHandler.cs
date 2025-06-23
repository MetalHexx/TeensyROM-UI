using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler(ILoggingService log, IAlertService alert, IDeviceConnectionManager deviceManager) : IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        private const int _reconnectDelayMs = 4000;

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

        private TeensyToken AttemptLaunch(LaunchFileCommand command)
        {
            log.Internal($"LaunchFileHandler: Clearing serial buffers");
            command.Serial.ClearBuffers();

            log.Internal($"LaunchFileHandler: Sending {TeensyToken.LaunchFile} token.");
            command.Serial.SendIntBytes(TeensyToken.LaunchFile, 2);

            var ack = command.Serial.HandleAck();

            LogAck(ack);

            if (ack == TeensyToken.RetryLaunch)
            {
                return TeensyToken.RetryLaunch;
            }

            log.Internal($"LaunchFileHandler: Sending storage token to TeensyROM");
            command.Serial.SendIntBytes(command.StorageType.GetStorageToken(), 1);

            log.Internal($"LaunchFileHandler: Sending {command.LaunchItem.Path} to TeensyROM");

            command.Serial.Write($"{command.LaunchItem.Path}\0");
            ack = command.Serial.HandleAck();

            LogAck(ack);

            return ack;
        }

        private async Task<LaunchFileResult> HandleAckResponse(LaunchFileCommand command)
        {
            if (command.LaunchItem is HexItem or ImageItem)
            {
                return new() { LaunchResult = LaunchFileResultType.Success };
            }
            var response = PollResponse(command);

            if (response != LaunchFileResultType.Disconnected) 
            {
                return GetFinalResult(response);
            }
            if (command.LaunchItem.Size >= 575000)
            {
                alert.Publish("Detected a large file launch. A reconnection will occur.");
            }
            await HandleReconnection(command);
            return GetFinalResult(PollResponse(command));
        }

        private async Task<LaunchFileResult> HandleRetryResponse(LaunchFileCommand command) 
        {
            alert.Publish("Detected launch retry request from TeensyROM. A reconnection will occur.");
            log.Internal($"LaunchFileHandler: Initiating Launch Retry of {command.LaunchItem.Name}");

            log.Internal("LaunchFileHandler: Waiting for re-connection to TeensyROM");

            log.Internal($"LaunchFileHandler: Waiting {_reconnectDelayMs}ms for TeensyROM to catch up");

            await HandleReconnection(command);

            AttemptLaunch(command);

            if (command.LaunchItem.Size >= 575000)
            {
                log.Internal($"LaunchFileHandler: Reconnecting again to new COM port due to large file launch retry.");
                await HandleReconnection(command);
            }
            return GetFinalResult(PollResponse(command));
        }

        private LaunchFileResultType PollResponse(LaunchFileCommand command)
        {
            try
            {
                var resultType = LaunchFileResultType.NoResponse;
                List<byte> bytesRead = [];

                for (int i = 0; i < 40; i++)
                {
                    var responseBytes = command.Serial.ReadSerialBytes(25);
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
            var resultString = responseBytes.ToUtf8();
            var resultToCheck = resultString.Replace("Loading IO handler: TeensyROM", string.Empty);
            var foundTokens = responseBytes.FindTRTokens();

            if (foundTokens.Any(t => t == TeensyToken.GoodSIDToken))
            {
                var resultHex = $"GoodSIDToken: 0x{responseBytes.ToHexString()}";
                log.External(resultHex);
                return LaunchFileResultType.Success;
            }
            if (foundTokens.Any(t => t == TeensyToken.BadSIDToken))
            {
                var resultHex = $"BadSIDToken: 0x{responseBytes.ToHexString()}";
                log.External(resultHex);
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

        private async Task HandleReconnection(LaunchFileCommand command)
        {
            for (int i = 0; i < 3; i++)
            {
                if (command.DeviceId is not null)
                {
                    await Task.Delay(4000);
                    var connected = await deviceManager.ConnectToNextPort(command.DeviceId);

                    if (connected) 
                    {
                        return;
                    }
                    if (i == 2) throw new TeensyException("Failed to handle launch file reconnection.");
                }
                else 
                {
                    try
                    {
                        command.Serial.EnsureConnection(_reconnectDelayMs);
                        return;
                    }
                    catch (TeensyException)
                    {
                        if (i == 2) throw;
                    }
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