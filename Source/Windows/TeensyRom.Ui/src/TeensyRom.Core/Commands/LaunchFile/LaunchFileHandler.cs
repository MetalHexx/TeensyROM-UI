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
    public class LaunchFileHandler(ISerialStateContext serialState, ILoggingService log, IAlertService alert) : IRequestHandler<LaunchFileCommand, LaunchFileResult>
    {
        public async Task<LaunchFileResult> Handle(LaunchFileCommand request, CancellationToken cancellationToken)
        {
            if (request.LaunchItem.Size > 575000) 
            {
                alert.Publish($"Launching files over 575k may cause a re-connect cycle.");
            }

            var ack = AttemptLaunch(request);

            if (ack == TeensyToken.RetryLaunch) 
            {
                
                await serialState.CurrentState
                    .OfType<SerialConnectedState>()
                    .Take(2)
                    .FirstAsync();

                await Task.Delay(4000);

                //Locking the serial manually since the connection will get dropped during this workflow.
                //The serial behavior will unlock it once the command completes.
                serialState.Lock();                
                ack = AttemptLaunch(request);

                if (ack == TeensyToken.Ack)
                {
                    return new LaunchFileResult
                    {
                        LaunchResult = LaunchFileResultType.Success
                    };
                }
                return new LaunchFileResult
                {
                    LaunchResult = LaunchFileResultType.ProgramError
                };
            }

            if (request.LaunchItem is HexItem or ImageItem)
            {
                return new LaunchFileResult
                {
                    LaunchResult = LaunchFileResultType.Success
                };
            }

            var resultType = PollResponse();
            serialState.ReadAndLogSerialAsString(100);

            return new LaunchFileResult
            {
                LaunchResult = resultType
            };
        }

        private TeensyToken AttemptLaunch(LaunchFileCommand request)
        {
            serialState.ClearBuffers();
            serialState.SendIntBytes(TeensyToken.LaunchFile, 2);
            var ack = serialState.HandleAck();

            if(ack == TeensyToken.RetryLaunch)
            {
                return TeensyToken.RetryLaunch;
            }
            serialState.SendIntBytes(request.StorageType.GetStorageToken(), 1);
            serialState.Write($"{request.LaunchItem.Path}\0");
            return serialState.HandleAck();
        }

        private LaunchFileResultType PollResponse()
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
                log.ExternalError($"Failed to launch sid: \r\n{resultString}");
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
                log.ExternalError($"Failed to launch program: \r\n{resultString}");
                return LaunchFileResultType.ProgramError;
            }
            return LaunchFileResultType.NoResponse;
        }
    }
}