using System.Management;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Settings
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

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                _currentSettings = JsonSerializer.Deserialize<TeensySettings>(content, options);
            }
            if (_currentSettings is null)
            {
                _currentSettings = InitDefaultSettings();
                WriteSettings(_currentSettings);
            }
            ValidateAndLogSettings(_currentSettings);

            return MapNewSettings(_currentSettings);          
        }

        public void SetCart(string comPort)
        {
            var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{comPort}'");
            var serialDevices = searcher.Get();                

            var settings = _settings.Value with { };

            var pnpDeviceId = searcher.Get()
                .Cast<ManagementObject>()
                .Select(mo => mo["PNPDeviceID"]?.ToString())
                .FirstOrDefault(pnp => !string.IsNullOrEmpty(pnp));

            if (string.IsNullOrEmpty(pnpDeviceId))
            {
                throw new Exception($"Unable to find PNPDeviceID for {comPort}");
            }
            var deviceHash = GetFileNameSafeHash(pnpDeviceId);

            var device = settings.KnownCarts
                .FirstOrDefault(RemoteMachineInfo => RemoteMachineInfo.DeviceHash == deviceHash);

            settings = settings with
            {
                LastCart = device is null ? new KnownCart(deviceHash, pnpDeviceId, comPort, $"TeensyROM #{settings.KnownCarts.Count() + 1}", new()) : device
            };

            settings.LastCart.MidiSettings.InitMappings();

            if (device is null)
            {
                settings.KnownCarts.Add(settings.LastCart with { });
            }
            else 
            {
                settings.KnownCarts.Remove(device);
                settings.KnownCarts.Add(settings.LastCart with { });
            }
            var currentCart = settings.KnownCarts.FirstOrDefault(x => x.DeviceHash == deviceHash);
            settings.LastCart = currentCart;

            SaveSettings(settings);          
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

        public static void Main()
        {
            string deviceId = @"USB\VID_16C0&PID_0489&MI_00\9&1FB47159&0&0000";
            string safeFileName = GetFileNameSafeHash(deviceId);
            Console.WriteLine("Safe file name: " + safeFileName);
        }

        private TeensySettings InitDefaultSettings()
        {
            var settings = new TeensySettings();
            settings.InitializeDefaults();            
            return settings;
        }
        public bool SaveSettings(TeensySettings settings)
        {
            if (!ValidateAndLogSettings(settings)) return false;

            _log.InternalSuccess($"Settings saved successfully.");
            _currentSettings = MapNewSettings(settings);
            WriteSettings(_currentSettings);
            Task.Run(() => _settings.OnNext(_currentSettings));            
            return true;
        }

        private TeensySettings MapNewSettings(TeensySettings settings) 
        {
            return settings with
            {
                LastCart = settings.LastCart is null ? null : settings.LastCart with { }
            };
        }

        private void WriteSettings(TeensySettings settings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
            }
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            File.WriteAllText(_settingsFilePath, JsonSerializer.Serialize(settings, options));

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
    }
}