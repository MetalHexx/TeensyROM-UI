using CsvHelper.Expressions;
using MediatR;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using TeensyRom.Core.Assets;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Commands.DeleteFile;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class CachedStorageService : ICachedStorageService
    {
        public IObservable<IEnumerable<IFileItem>> FilesAdded => _filesAdded.AsObservable();
        private Subject<IEnumerable<IFileItem>> _filesAdded = new();

        protected readonly ISettingsService _settingsService;
        private readonly IGameMetadataService _gameMetadata;
        private readonly ISidMetadataService _sidMetadata;
        private readonly IMediator _mediator;
        private readonly IAlertService _alert;
        private TeensySettings _settings = null!;
        private IDisposable? _settingsSubscription;
        private string _usbCacheFileName => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), StorageConstants.Usb_Cache_File_Path);
        private string _sdCacheFileName => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), StorageConstants.Sd_Cache_File_Path);
        private string CacheFilePath => _settings.StorageType is TeensyStorageType.SD
            ? _sdCacheFileName
            : _usbCacheFileName;

        private StorageCache _storageCache = null!;
        
        public CachedStorageService(ISettingsService settings, IGameMetadataService gameMetadata, ISidMetadataService sidMetadata, IMediator mediator, IAlertService alert)
        {
            _settingsService = settings;
            _gameMetadata = gameMetadata;
            _sidMetadata = sidMetadata;
            _mediator = mediator;
            _alert = alert;
            _settingsSubscription = _settingsService.Settings
                .Where(s => s is not null)
                .Subscribe(OnSettingsChanged);
        }

        private void OnSettingsChanged(TeensySettings newSettings)
        {   
            var previousSettings = _settings == null 
                ?  null 
                : _settings with { };
            
            _settings = newSettings;

            if (previousSettings is null || _settings.StorageType != previousSettings.StorageType)
            {
                LoadCache();
            }
            
        }
        
        public async Task<ILaunchableItem?> SaveFavorite(ILaunchableItem launchItem)
        {
            var favPath = _settings.GetFavoritePath(launchItem.FileType);

            var favCommand = new FavoriteFileCommand
            (
                storageType: _settings.StorageType, 
                sourcePath: launchItem.Path, 
                targetPath: favPath.UnixPathCombine(launchItem.Name)
            );

            var favoriteResult = await _mediator.Send(favCommand);

            if(!favoriteResult.IsSuccess)
            {
                _alert.Publish($"There was an error tagging {launchItem.Name} as favorite.");
                return null;
            }
            _alert.Publish($"{launchItem.Name} has been tagged as a favorite.");
            _alert.Publish($"A copy was placed in {favPath}.");

            var favItem = launchItem switch
            {
                SongItem s => s.Clone(),
                GameItem g => g.Clone(),
                HexItem h => h.Clone(),
                ImageItem image => image.Clone(),
                FileItem f => f.Clone(),                
                _ => throw new TeensyException("Unknown file type")
            };

            if (!favoriteResult.IsSuccess) return null;

            favItem.IsFavorite = true;
            favItem.Path = favPath.UnixPathCombine(favItem.Name);
            favItem.FavParentPath = launchItem.Path;

            launchItem.IsFavorite = true;            
            launchItem.FavChildPath = favItem.Path;

            _storageCache.UpsertFile(launchItem);
            _storageCache.UpsertFile(favItem);

            SaveCacheToDisk();

            return favItem as ILaunchableItem;
        }

        public async Task RemoveFavorite(ILaunchableItem file) 
        {
            IFileItem? sourceFile;
            IFileItem? favFile;

            if (string.IsNullOrWhiteSpace(file.FavChildPath) && string.IsNullOrWhiteSpace(file.FavParentPath)) 
            {
                _alert.Publish($"{file.Path} is an orphan.  Try re-indexing your files.");
                return;
            }

            if(string.IsNullOrWhiteSpace(file.FavChildPath))
            {
                favFile = file;
                sourceFile = _storageCache.GetFileByPath(file.FavParentPath);
            }
            else
            {
                sourceFile = file;
                favFile = _storageCache.GetFileByPath(file.FavChildPath);
            }
            if(sourceFile is not null)
            {
                sourceFile.IsFavorite = false;
                sourceFile.FavChildPath = string.Empty;
                _storageCache.UpsertFile(sourceFile);
            }
            if(favFile is null) return;

            favFile.IsFavorite = false; 

            _storageCache.DeleteFile(favFile.Path);            
            _alert.Publish($"{favFile.Path} was untagged as a favorite.");            
            await _mediator.Send(new DeleteFileCommand(_settings.StorageType, favFile.Path));
            favFile.Path = sourceFile?.Path ?? favFile.Path; //TODO: smell - this fixes a bug if favorite is re-favorited from play toolbar.
            return;
        }

        public void MarkIncompatible(ILaunchableItem launchItem)
        {
            launchItem.IsCompatible = false;
            _storageCache.UpsertFile(launchItem);

            SaveCacheToDisk();
        }

        public void ClearCache()
        {
            _storageCache.Clear();

            if (!File.Exists(CacheFilePath)) return;

            File.Delete(CacheFilePath);
        }
        public void ClearCache(string path) => _storageCache.DeleteDirectoryWithChildren(path);

        private void LoadCache()
        {
            if (!File.Exists(CacheFilePath)) 
            {
                _storageCache = new StorageCache(_settings.BannedDirectories, _settings.BannedFiles);
                SaveCacheToDisk();
                return;
            }
            LoadCacheFromDisk();
        }

        private void LoadCacheFromDisk()
        {
            using var stream = File.Open(CacheFilePath, FileMode.Open, FileAccess.Read);
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var cacheFromDisk = JsonConvert.DeserializeObject<StorageCache>(content, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            });
            cacheFromDisk?.SetBanLists(_settings.BannedDirectories, _settings.BannedFiles);

            if(cacheFromDisk is null) return;

            _storageCache = cacheFromDisk;
        }

        public void EnsureFavorites()
        {
            List<ILaunchableItem> favsToFavorite = GetFavoriteItemsFromCache();

            favsToFavorite.ForEach(fav => fav.IsFavorite = true);

            _storageCache
                .Where(NotFavoriteFilter)
                .SelectMany(c => c.Value.Files)                
                .Where(f => favsToFavorite.Any(fav => fav.Id == f.Id))
                .ToList()
                .ForEach(parentFile => 
                {                       
                    var fav = favsToFavorite.First(fav => fav.Id == parentFile.Id);
                    fav.FavParentPath = parentFile.Path;
                    fav.MetadataSourcePath = parentFile.MetadataSourcePath;
                    parentFile.IsFavorite = true;
                    parentFile.FavChildPath = fav.Path;
                    _storageCache.UpsertFile(parentFile);
                    _storageCache.UpsertFile(fav);
                });
        }

        public List<ILaunchableItem> GetFavoriteItemsFromCache()
        {
            List<ILaunchableItem> favs = [];

            foreach (var target in _settings.GetFavoritePaths())
            {
                favs.AddRange(_storageCache
                    .Where(c => c.Key.RemoveLeadingAndTrailingSlash().Contains(target.RemoveLeadingAndTrailingSlash()))
                    .SelectMany(c => c.Value.Files)
                    .ToList()
                    .Cast<ILaunchableItem>());
            }
            return favs;
        }

        private void SaveCacheToDisk()
        {
            if (!Directory.Exists(Path.GetDirectoryName(CacheFilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CacheFilePath)!);
            }

            File.WriteAllText(CacheFilePath, JsonConvert.SerializeObject(_storageCache, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            }));
        }
        public async Task<StorageCacheItem?> GetDirectory(string path)
        {
            var isBanned = _settings.BannedDirectories.Any(b => path.Contains(b));

            if(isBanned) return null;
            
            var cacheItem = _storageCache.GetByDirPath(path);

            if (cacheItem != null)
            {                
                return cacheItem;
            }

            var response = await _mediator.Send(new GetDirectoryCommand(_settings.StorageType, path));

            if (response.DirectoryContent is null) return null;  

            var filteredContent = FilterBannedItems(response.DirectoryContent);

            cacheItem = SaveDirectoryToCache(filteredContent);
            SaveCacheToDisk();
            return cacheItem;
        }

        private StorageCacheItem SaveDirectoryToCache(DirectoryContent dirContent)
        {
            StorageCacheItem? cacheItem;
            var files = MapAndOrderFiles(dirContent);
            var directories = MapAndOrderDirectories(dirContent);

            cacheItem = new StorageCacheItem
            {
                Path = dirContent.Path,
                Directories = directories.ToList(),
                Files = files
            };

            var favPaths = _settings.GetFavoritePaths();

            if (favPaths.Any(dirContent.Path.Contains)) FavCacheItems(cacheItem);

            _storageCache.UpsertDirectory(dirContent.Path, cacheItem);            
            return cacheItem;
        }

        private static void FavCacheItems(StorageCacheItem cacheItem) => cacheItem.Files.ForEach(f => f.IsFavorite = true);

        private static IEnumerable<DirectoryItem> MapAndOrderDirectories(DirectoryContent? directoryContent)
        {
            return directoryContent?.Directories
                .Select(d => new DirectoryItem
                {
                    Name = d.Name,
                    Path = d.Path
                })
                .OrderBy(d => d.Name)
                .ToList() ?? new List<DirectoryItem>();
        }

        private List<IFileItem> MapAndOrderFiles(DirectoryContent? directoryContent)
        {
            return directoryContent?.Files
                .Select(MapFile)
                .OrderBy(file => file.Name)
                .ToList() ?? [];
        }

        public IFileItem MapFile(IFileItem file) 
        {
            var mappedFile = file.MapFileItem();
            return mappedFile switch
            {
                SongItem s => _sidMetadata.EnrichSong(s),
                GameItem g => _gameMetadata.EnrichGame(g),
                HexItem h => h,
                ImageItem i => i,
                _ => file
            };
        }

        

        public async Task<SaveFilesResult> SaveFiles(IEnumerable<FileTransferItem> files)
        {
            List<IFileItem> addedFiles = new();
            SaveFilesResult saveResults = new();

            var result = await _mediator.Send(new SaveFilesCommand(files.ToList()));         

            foreach (var f in result.SuccessfulFiles)
            {
                var storageItem = f.ToFileItem();

                if (storageItem is SongItem song) _sidMetadata.EnrichSong(song);
                if (storageItem is GameItem game) _gameMetadata.EnrichGame(game);
                if (storageItem is FileItem file)
                {
                    _storageCache.UpsertFile(file);
                    addedFiles.Add(file);
                }
            }
            SaveCacheToDisk();
            _filesAdded.OnNext(addedFiles);
            return saveResults;
        }

        public async Task DeleteFile(IFileItem file, TeensyStorageType storageType)
        {
            await _mediator.Send(new DeleteFileCommand(storageType, file.Path));

            _storageCache.DeleteFile(file.Path);

            _storageCache
                .GetFileByName(file.Name)
                .ForEach(f => f.IsFavorite = false);
        }
        public void Dispose() => _settingsSubscription?.Dispose();

        public ILaunchableItem? GetRandomFile(params TeensyFileType[] fileTypes) 
        {
            if (fileTypes.Length == 0)
            {
                fileTypes = TeensyFileTypeExtensions.GetLaunchFileTypes();
            }
            var selection = _storageCache
                .SelectMany(c => c.Value.Files)                
                .Where(f => fileTypes.Contains(f.FileType))
                .OfType<ILaunchableItem>()                
                .ToArray();

            if (selection.Length == 0) return null;

            return selection[new Random().Next(selection.Length - 1)];
        }

        public IEnumerable<ILaunchableItem> Search(string searchText, params TeensyFileType[] fileTypes)
        {
            var quotedMatches = Regex
                .Matches(searchText, @"(\+?""([^""]+)"")|\+?\S+")
                .Cast<Match>()
                .Select(m => m.Groups[2].Success ? (m.Groups[1].Value.StartsWith("+") ? "+" : "") + m.Groups[2].Value : m.Groups[0].Value)
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();

            searchText = searchText.Replace("\"", "");
            searchText = searchText.Replace("+", "");

            foreach (var quotedMatch in quotedMatches)
            {
                var noPlusQuotedMatch = string.IsNullOrWhiteSpace(quotedMatch) 
                    ? string.Empty
                    : quotedMatch.Replace("+", "");

                searchText = string.IsNullOrWhiteSpace(searchText) 
                    ? string.Empty 
                    : searchText.Replace($"{noPlusQuotedMatch}", "");
            }

            var searchTerms = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            searchTerms.RemoveAll(term => _settings.SearchStopWords.Contains(term.ToLower()));

            searchTerms.AddRange(quotedMatches);

            var requiredTerms = searchTerms
                .Where(term => term.StartsWith("+"))
                .Select(term => term.Substring(1))
                .ToList();

            searchTerms = searchTerms.Select(term => term.TrimStart('+')).ToList();

            return _storageCache
                .Where(NotFavoriteFilter)
                .SelectMany(c => c.Value.Files)
                .OfType<ILaunchableItem>()
                .Where(f => fileTypes.Contains(f.FileType) &&
                            requiredTerms.All(requiredTerm =>
                                f.Title.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Name.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Creator.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Path.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Description.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase)))
                .Select(file => new
                {
                    File = file,
                    Score = searchTerms.Sum(term =>
                        (file.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Title : 0) +
                        (file.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.FileName : 0) +
                        (file.Creator.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Creator : 0) +
                        (file.Path.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.FilePath : 0) +
                        (file.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ? _settings.SearchWeights.Description : 0))
                })
                .Where(result => result.Score > 0)
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.File.Title)
                .Select(result => result.File);
        }

        Func<KeyValuePair<string, StorageCacheItem>, bool> NotFavoriteFilter => 
            kvp => !_settings
                .GetFavoritePaths()
                .Select(p => p.RemoveLeadingAndTrailingSlash())
                .Any(favPath => kvp.Key.Contains(favPath));

        public Task CacheAll() => CacheAll(StorageConstants.Remote_Path_Root);

        public async Task CacheAll(string path)
        {
            if (path == StorageConstants.Remote_Path_Root)
            {
                ClearCache();
            }
            else 
            {
                ClearCache(path);
            }
            _alert.Publish($"Refreshing cache for {path} and all nested directories.");
            var response = await _mediator.Send(new GetDirectoryRecursiveCommand(_settings.StorageType, path));

            if (!response.IsSuccess) return;

            _alert.Publish($"Enriching music and games.");

            await Task.Run(() =>
            {
                foreach (var directory in response.DirectoryContent)
                {
                    if(directory is null) continue;

                    var filteredContent = FilterBannedItems(directory);

                    if(filteredContent is null) continue;

                    SaveDirectoryToCache(filteredContent);
                }
                EnsureFavorites();
                SaveCacheToDisk();
            });
            _alert.Publish($"Indexing completed for {_settings.StorageType} storage.");
        }

        private DirectoryContent? FilterBannedItems(DirectoryContent directoryContent)
        {
            var pathBanned = _settings.BannedDirectories.Any(d => directoryContent.Path.Contains(d));
            if (pathBanned) return null;

            var filteredDirectories = directoryContent.Directories.Where(d => _settings.BannedDirectories.All(b => !d.Path.Contains(b)));
            var filteredFiles = directoryContent.Files.Where(f => _settings.BannedFiles.All(b => !f.Name.Contains(b)));

            return new DirectoryContent
            {
                Path = directoryContent.Path,
                Directories = filteredDirectories.ToList(),
                Files = filteredFiles.ToList()
            };
        }
    }
}