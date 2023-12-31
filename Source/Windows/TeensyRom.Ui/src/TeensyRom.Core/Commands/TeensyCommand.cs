using System.Diagnostics;
using System.Reactive.Linq;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Commands
{
    public abstract class TeensyCommand: IDisposable
    {
        protected readonly ISettingsService _settingsService;
        protected readonly IObservableSerialPort _serialPort;
        protected readonly ILoggingService _logService;

        private IDisposable _settingsSubscription;
        protected TeensySettings _settings = new();

        public TeensyCommand(ISettingsService settingsService, IObservableSerialPort serialPort, ILoggingService logService)
        {
            _settingsService = settingsService;
            _serialPort = serialPort;
            _logService = logService;
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _settingsSubscription = _settingsService.Settings
                .Subscribe(settings => _settings = settings);
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
