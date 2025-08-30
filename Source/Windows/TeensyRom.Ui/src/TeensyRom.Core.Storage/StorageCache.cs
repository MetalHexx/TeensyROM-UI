using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text.RegularExpressions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Entities.Storage;
using System.Reflection;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Core.Storage
{
    public class StorageCache : BaseStorageCache, IDisposable
    {
        private ISettingsService _settingsService;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription;
        private string _usbCacheFileName = string.Empty;
        private string _sdCacheFileName = string.Empty;
        
        private List<string> _bannedFolders = [];
        private List<string> _bannedFiles = [];
        
        protected override string CacheFilePath => _settings.StorageType is TeensyStorageType.SD
            ? _sdCacheFileName
            : _usbCacheFileName;

        protected override List<string> BannedFiles => _bannedFiles;
        protected override List<string> BannedDirectories => _bannedFolders;

        public StorageCache() { }

        public StorageCache(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _settingsSubscription = _settingsService.Settings
                .Where(s => s is not null && s.LastCart is not null)
                .Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings newSettings)
        {
            var previousSettings = _settings == null
                ? null
                : _settings with { };

            _settings = newSettings with { };

            _usbCacheFileName = Path.Combine(
                Assembly.GetExecutingAssembly().GetPath(),
                StorageHelper.Usb_Cache_File_Relative_Path,
                $"{StorageHelper.Usb_Cache_File_Name}{_settings.LastCart.DeviceHash}{StorageHelper.Cache_File_Extension}");

            _sdCacheFileName = Path.Combine(
                Assembly.GetExecutingAssembly().GetPath(),
                StorageHelper.Sd_Cache_File_Relative_Path,
                $"{StorageHelper.Sd_Cache_File_Name}{_settings.LastCart.DeviceHash}{StorageHelper.Cache_File_Extension}");

            if (previousSettings is null || _settings.StorageType != previousSettings.StorageType || _settings.LastCart.DeviceHash != previousSettings.LastCart?.DeviceHash)
            {
                _bannedFolders = _settings.BannedDirectories.ToList();
                _bannedFiles = _settings.BannedFiles.ToList();
                ReadFromDisk();
                _storageReady.OnNext(Unit.Default);
            }
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}