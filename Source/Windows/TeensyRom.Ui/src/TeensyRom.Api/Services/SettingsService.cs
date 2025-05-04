using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Services
{
    public class SettingsService : ISettingsService
    {
        public IObservable<TeensySettings> Settings => _settings.AsObservable();

        private BehaviorSubject<TeensySettings> _settings;
        private TeensySettings? _currentSettings;
        private string _settingsFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), SettingsConstants.SettingsPath);

        private readonly ILoggingService _log;

        public SettingsService(ILoggingService log)
        {
            _log = log;
            _settings = new BehaviorSubject<TeensySettings>(GetSettings());
        }

        public TeensySettings GetSettings()
        {
            if (_currentSettings is not null) return MapNewSettings(_currentSettings);

            if (File.Exists(_settingsFilePath))
            {
                using var stream = File.Open(_settingsFilePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                _currentSettings = LaunchableItemSerializer.Deserialize<TeensySettings>(content);
            }
            if (_currentSettings is null)
            {
                _currentSettings = InitDefaultSettings();
                WriteSettings(_currentSettings);
            }
            ValidateAndLogSettings(_currentSettings);

            return MapNewSettings(_currentSettings);
        }

        private TeensySettings InitDefaultSettings()
        {
            var settings = new TeensySettings();
            settings.InitializeDefaults();
            return settings;
        }

        private void WriteSettings(TeensySettings settings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
            }
            File.WriteAllText(_settingsFilePath, LaunchableItemSerializer.Serialize(settings));

        }

        public bool ValidateAndLogSettings(TeensySettings settings)
        {
            if (!Directory.Exists(settings.WatchDirectoryLocation))
            {
                _log.InternalError($"The watch directory '{settings.WatchDirectoryLocation}' was not found.  Please go create it.");
                return false;
            }
            return true;
        }

        private TeensySettings MapNewSettings(TeensySettings settings)
        {
            return settings with
            {
                LastCart = settings.LastCart is null ? null : settings.LastCart with { }
            };
        }

        public bool SaveSettings(TeensySettings settings)
        {
            _settings.OnNext(settings);
            return true;
        }

        public void SetCart(string comPort)
        {
            //var serialProvider = SerialDeviceInfoProviderFactory.Create();
            //var pnpDeviceId = serialProvider.GetPnpDeviceId(comPort);

            //if (string.IsNullOrEmpty(pnpDeviceId))
            //{
            //    throw new Exception($"Unable to find PNPDeviceID for {comPort}");
            //}
            //var deviceHash = GetFileNameSafeHash(pnpDeviceId);

            //var settings = _settings.Value with { };

            //var device = settings.KnownCarts
            //    .FirstOrDefault(RemoteMachineInfo => RemoteMachineInfo.DeviceHash == deviceHash);

            //settings = settings with
            //{
            //    LastCart = device is null ? new KnownCart(deviceHash, pnpDeviceId, comPort, $"TeensyROM #{settings.KnownCarts.Count() + 1}", new(), null) : device
            //};

            //settings.LastCart.MidiSettings.InitMappings();

            //if (device is null)
            //{
            //    settings.KnownCarts.Add(settings.LastCart with { });
            //}
            //else
            //{
            //    settings.KnownCarts.Remove(device);
            //    settings.KnownCarts.Add(settings.LastCart with { });
            //}
            //var currentCart = settings.KnownCarts.FirstOrDefault(x => x.DeviceHash == deviceHash);
            //settings.LastCart = currentCart;

            //SaveSettings(settings);
        }

        public static string GetFileNameSafeHash(string stringToHash)
        {
            using (var md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(stringToHash);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return string.Concat(hashBytes.Select(b => b.ToString("X2")));
            }
        }
    }
}
