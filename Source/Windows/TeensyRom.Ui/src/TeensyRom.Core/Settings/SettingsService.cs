using Newtonsoft.Json;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Logging;
using Formatting = Newtonsoft.Json.Formatting;

namespace TeensyRom.Core.Settings
{
    public class SettingsService : ISettingsService
    {
        public IObservable<TeensySettings> Settings => _settings.AsObservable();

        private BehaviorSubject<TeensySettings> _settings;

        private const string _settingsFilePath = "Settings.json";
        private readonly ILoggingService _logService;

        public SettingsService(ILoggingService logService)
        {
            _logService = logService;
            _settings = new BehaviorSubject<TeensySettings>(GetSettings());
        }

        private TeensySettings GetSettings()
        {
            try
            {
                using var stream = File.Open(_settingsFilePath, FileMode.Open, FileAccess.Read);
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                var settings = JsonConvert.DeserializeObject<TeensySettings>(content, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                });

                ValidateAndLogSettings(settings);

                return settings;
            }
            catch
            {
                var settings = new TeensySettings();
                settings.InitializeDefaults();
                return settings;
            }
        }
        public bool SaveSettings(TeensySettings settings)
        {
            if (!ValidateAndLogSettings(settings)) return false;

            _logService.Log($"Settings saved successfully.");
            _settings.OnNext(settings);

            File.WriteAllText(_settingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
            return true;
        }

        public bool ValidateAndLogSettings(TeensySettings settings)
        {
            if (!Directory.Exists(settings.WatchDirectoryLocation))
            {
                _logService.Log($"The watch directory '{settings.WatchDirectoryLocation}' was not found.  Please go create it.");
                return false;
            }
            return true;
        }
    }
}