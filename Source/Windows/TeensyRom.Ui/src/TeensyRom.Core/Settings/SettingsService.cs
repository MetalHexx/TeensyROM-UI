using Newtonsoft.Json;
using System.Management;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;
using Formatting = Newtonsoft.Json.Formatting;

namespace TeensyRom.Core.Settings
{
    public class SettingsService : ISettingsService
    {
        public IObservable<TeensySettings> Settings => _settings.AsObservable();

        private BehaviorSubject<TeensySettings> _settings;
        private string _settingsFilePath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), SettingsConstants.SettingsPath);

        private readonly ILoggingService _log;

        public SettingsService(ILoggingService log)
        {
            _log = log;
            _settings = new BehaviorSubject<TeensySettings>(GetSettings());
        }

        public TeensySettings GetSettings()
        {
            var settings = _settings?.Value;

            if (settings is not null) return settings;

            if (File.Exists(_settingsFilePath))
            {
                using var stream = File.Open(_settingsFilePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                settings = JsonConvert.DeserializeObject<TeensySettings>(content, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                });
            }
            if (settings is null)
            {
                settings = InitDefaultSettings();
                WriteSettings(settings);
            }
            ValidateAndLogSettings(settings);

            return settings;          
        }

        public void SetCart(string comPort)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_SerialPort WHERE DeviceID = '{comPort}'");
                var serialDevices = searcher.Get();

                var settings = _settings.Value with { };

                var pnpDeviceId = string.Empty;

                foreach (var s in serialDevices)
                {
                    foreach (PropertyData property in s.Properties)
                    {
                        if (property.Name == "DeviceID" && property.Value?.ToString() == comPort)
                        {
                            pnpDeviceId = s.Properties["PNPDeviceID"]?.Value?.ToString();
                            break;
                        }
                    }
                    if(!string.IsNullOrEmpty(pnpDeviceId)) break;
                }
                if (string.IsNullOrEmpty(pnpDeviceId))
                {
                    throw new Exception($"Unable to find PNPDeviceID for {comPort}");
                }
                var deviceHash = GetFileNameSafeHash(pnpDeviceId);

                var device = settings.KnownCarts
                    .FirstOrDefault(RemoteMachineInfo => RemoteMachineInfo.DeviceHash == deviceHash);

                settings = settings with
                {
                    LastCart = device is null ? new KnownCart(deviceHash, pnpDeviceId, comPort, $"TeensyROM #{settings.KnownCarts.Count() + 1}") : device
                };

                if (device is null)
                {
                    settings.KnownCarts.Add(settings.LastCart with { });
                }
                else 
                {
                    settings.KnownCarts.Remove(device);
                    settings.KnownCarts.Add(settings.LastCart with { });
                }
                SaveSettings(settings);                
            }
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
            _settings.OnNext(settings with { });
            WriteSettings(settings);
            return true;
        }

        private void WriteSettings(TeensySettings settings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(_settingsFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
            }
            File.WriteAllText(_settingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
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