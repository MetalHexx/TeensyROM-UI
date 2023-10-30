using Newtonsoft.Json;
using System.Reactive.Linq;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Serial.Services;
using TeensyRom.Core.Settings.Entities;
using TeensyRom.Core.Settings.Services;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class TeensyDirectoryService : ITeensyDirectoryService, IDisposable
    {
        private readonly ITeensyObservableSerialPort _teensyPort;
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _logService;
        private IDisposable _settingsSubscription;
        private TeensySettings _settings = new();

        public TeensyDirectoryService(ITeensyObservableSerialPort teensyPort, ISettingsService settingsService, ILoggingService logService)
        {
            _teensyPort = teensyPort;
            _settingsService = settingsService;
            _logService = logService;

            _settingsSubscription = _settingsService.Settings
                .Subscribe(settings => _settings = settings);
        }

        public DirectoryContent? GetDirectoryContent(string path)
        {
            DirectoryContent directoryContent = new();
            uint take = 5;
            uint skip = 0;

            var hasMorePages = true;
            while (hasMorePages)
            {

                var page = _teensyPort.GetDirectoryContent(path, _settings.TargetType, skip, take);

                if (page is null)
                {
                    _logService.Log("There was an error.  Received a null result from the request");
                    return null;
                }
                directoryContent.Add(page);
                skip += (uint)page.TotalCount;
                hasMorePages = page.TotalCount == take;
            }
            directoryContent.Directories = directoryContent.Directories
                .OrderBy(d => d.Name)
                .ToList();

            directoryContent.Files = directoryContent.Files
                .OrderBy(d => d.Name)
                .ToList();

            _logService.Log($"Received the following directory contents:");
            _logService.Log(JsonConvert.SerializeObject(directoryContent, new JsonSerializerSettings { Formatting = Formatting.Indented }));
            return directoryContent;
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}
