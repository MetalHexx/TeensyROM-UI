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
    public class SimpleStorageCache : BaseStorageCache
    {
        private string _cacheFilePath = string.Empty;
        private List<string> _bannedDirectories = [];
        private List<string> _bannedFiles = [];
        
        protected override string CacheFilePath => _cacheFilePath;
        protected override List<string> BannedFiles => _bannedFiles;
        protected override List<string> BannedDirectories => _bannedDirectories;

        public SimpleStorageCache(CartStorage cartStorage, StorageSettings settings) 
        {
            if(cartStorage.Type == TeensyStorageType.SD)
            {
                _cacheFilePath = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Sd_Cache_File_Relative_Path,
                    $"{StorageHelper.Sd_Cache_File_Name}{cartStorage.DeviceId}{StorageHelper.Cache_File_Extension}");
            }
            else
            {
                _cacheFilePath = Path.Combine(
                    Assembly.GetExecutingAssembly().GetPath(),
                    StorageHelper.Usb_Cache_File_Relative_Path,
                    $"{StorageHelper.Usb_Cache_File_Name}{cartStorage.DeviceId}{StorageHelper.Cache_File_Extension}");
            }
            _bannedDirectories = settings.BannedDirectories;
            _bannedFiles = settings.BannedFiles;
            ReadFromDisk();
            _storageReady.OnNext(Unit.Default);
        }
    }
}