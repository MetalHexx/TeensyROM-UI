using MediatR;
using System.Reactive.Linq;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public class FavoriteFileHandler : IRequestHandler<FavoriteFileCommand, FavoriteFileResult>
    {
        private readonly ISerialStateContext _serialState;
        private TeensySettings _settings;

        public FavoriteFileHandler(ISerialStateContext serialState, ISettingsService settings)
        {
            settings.Settings.Take(1).Subscribe(s => _settings = s);
            _serialState = serialState;
        }

        public Task<FavoriteFileResult> Handle(FavoriteFileCommand request, CancellationToken cancellationToken)
        {
            _serialState.SendIntBytes(TeensyToken.CopyFile, 2);
            _serialState.SendIntBytes(_settings.TargetType.GetStorageToken(), 1);
            _serialState.Write($"{request.SourcePath}\0");
            _serialState.Write($"{request.DestPath}\0");

            try
            {
                _serialState.HandleAck();
            }
            catch (TeensyException ex)
            {
                if (ParseBusyResponse(ex.Message))
                {
                    return Task.FromResult(new FavoriteFileResult
                    {
                        IsSuccess = false,
                        IsBusy = true
                    });

                }
                throw;
            }
            return Task.FromResult(new FavoriteFileResult());
        }

        private bool ParseBusyResponse(string response) => response.Contains("Busy") ? true : false;
    }
}