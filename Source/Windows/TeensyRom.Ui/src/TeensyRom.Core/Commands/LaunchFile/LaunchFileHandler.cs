using MediatR;
using System.Reactive.Linq;
using System.Reflection.Metadata.Ecma335;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands.File.LaunchFile
{
    public class LaunchFileHandler(ISerialStateContext serialState, ILoggingService log, IAlertService alert) : IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        public async Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            if (request.LaunchItem.Size > 575000) 
            {
                alert.Publish($"Launching files over 575k may cause a re-connect cycle.");
            }
            var ack = AttemptLaunch(request);

            if (ack == TeensyToken.Ack)
            {
                return new LaunchFileResult { LaunchResult = LaunchFileResultType.Success };
            }
            if (ack == TeensyToken.RetryLaunch) 
            {                
                log.Internal($"LaunchFileHandler: Initiating Launch Retry of {request.LaunchItem.Name}");

                log.Internal("LaunchFileHandler: Waiting for re-connection to TeensyROM");

                await serialState.CurrentState
                    .OfType<SerialConnectedState>()
                    .Take(2)
                    .FirstAsync();

                var ms = 1000;

                log.Internal($"LaunchFileHandler: Waiting {ms}ms for TeensyROM to catch up");
                await Task.Delay(ms);

                //Locking the serial manually since the connection will get dropped during this workflow.
                //The serial behavior will unlock it once the command completes.
                serialState.Lock();
                log.Internal($"LaunchFileHandler: Manually locking Serial Port");
                ack = AttemptLaunch(request);

                if (ack == TeensyToken.Ack)
                {
                    return new LaunchFileResult { LaunchResult = LaunchFileResultType.Success };
                }
                return new LaunchFileResult { LaunchResult = LaunchFileResultType.ProgramError };
            }            

            if (request.LaunchItem is HexItem or ImageItem)
            {
                return new LaunchFileResult { LaunchResult = LaunchFileResultType.Success };
            }

            var resultType = PollResponse();
            serialState.ReadAndLogSerialAsString(100);
            return new LaunchFileResult { LaunchResult = resultType };
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

            serialState.Write($"{request.LaunchItem.Path}\0");
            ack = serialState.HandleAck();

            LogAck(ack);

            return ack;
        }

        private void LogAck(TeensyToken ack) 
        {
            if (ack == TeensyToken.Ack || ack == TeensyToken.RetryLaunch)
            {
                log.Internal($"LaunchFileHandler: Received {ack} token from TeensyROM");
            }
            else
            {
                log.InternalError($"LaunchFileHandler: Received {ack} token from TeensyROM");
            }
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
                    //TODO: I don't love this, make it cleaner.  This covers race condition when the TR disconnects during dual boot when going from COM5 to COM3 (say, on a large game to SID launch).   This will prevent an incorrect error message in the logs
                    return LaunchFileResultType.Success;
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
    }
}