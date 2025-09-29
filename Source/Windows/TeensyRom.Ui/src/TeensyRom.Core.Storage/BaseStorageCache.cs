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
    /// <summary>
    /// Base class for storage cache implementations that contains common functionality
    /// </summary>
    public abstract class BaseStorageCache : Dictionary<string, IStorageCacheItem>, IStorageCache
    {
        public IObservable<Unit> StorageReady => _storageReady.AsObservable();
        protected Subject<Unit> _storageReady = new();

        protected Dictionary<string, List<FileItem>>? _fileIdCache;
        protected abstract string CacheFilePath { get; }
        protected abstract List<string> BannedFiles { get; }
        protected abstract List<string> BannedDirectories { get; }

        // Shared lock for thread-safety
        private readonly ReaderWriterLockSlim _ioLock = new(LockRecursionPolicy.SupportsRecursion);

        protected void InvalidateIdCache() => _fileIdCache = null;

        public void UpsertDirectory(DirectoryPath path, IStorageCacheItem directory)
        {
            if (IsBannedFolder(path)) return;

            DeleteDirectory(path);
            Insert(path, directory);
            InvalidateIdCache();
        }

        public void UpsertFile(FileItem fileItem)
        {
            if (IsBannedFile(fileItem.Path.Value)) return;

            IStorageCacheItem parentStorageItem = null!;

            if (fileItem.Path.Directory.IsRoot)
            {
                parentStorageItem = GetByDirPath(fileItem.Path.Directory)!;
            }
            else 
            {
                EnsureParents(fileItem.Path.Directory);
                parentStorageItem = GetByDirPath(fileItem.Path.Directory)!;
            }
            parentStorageItem!.UpsertFile(fileItem);
            UpsertDirectory(parentStorageItem.Path, parentStorageItem);
            InvalidateIdCache();
        }

        public virtual void EnsureParents(DirectoryPath parentPath)
        {
            var parentDirectory = GetByDirPath(parentPath);

            if (parentDirectory is not null) return;

            parentDirectory = new StorageCacheItem
            {
                Path = parentPath,
                Directories = [],
                Files = []
            };
            Insert(parentDirectory.Path, parentDirectory);

            if (parentPath.ParentPath is null) return;

            EnsureParents(parentPath.ParentPath);

            var grandParent = GetByDirPath(parentPath.ParentPath) 
                ?? throw new Exception("Directory should not be null since it was just ensured");

            grandParent.InsertSubdirectory(new DirectoryItem(parentDirectory.Path));
        }

        protected void Insert(DirectoryPath path, IStorageCacheItem cacheItem)
        {
            if (IsBannedFolder(path)) return;

            cacheItem = CleanBadFilesAndFolders(cacheItem);
            TryAdd(path.Value, cacheItem);
        }

        public void DeleteDirectory(DirectoryPath path)
        {
            var dir = GetByDirPath(path);

            if (dir is null) return;

            Remove(path.Value);
            InvalidateIdCache();
        }

        public void DeleteDirectoryWithChildren(DirectoryPath path)
        {
            var currentDir = GetByDirPath(path);

            if (currentDir is null) return;

            foreach (var directory in currentDir.Directories)
            {
                if (!path.Equals(directory.Path)) 
                {
                    DeleteDirectoryWithChildren(directory.Path);
                }
            }
            DeleteDirectory(currentDir.Path);
        }

        public void ClearCache()
        {
            Clear();
            InvalidateIdCache();

            if (!File.Exists(CacheFilePath)) return;

            File.Delete(CacheFilePath);
        }

        public virtual IStorageCacheItem? GetByDirPath(DirectoryPath path)
        {
            if (IsBannedFolder(path)) return null;

            if (!TryGetValue(path.Value, out var item)) return null;

            return item;
        }

        public FileItem? GetFileByPath(FilePath filePath)
        {
            TryGetValue(filePath.Directory.Value, out IStorageCacheItem? dir);

            if (dir is not null)
            {
                return dir.Files.FirstOrDefault(f => f.Path.Equals(filePath));
            }
            return null;
        }

        public List<FileItem> GetFileByName(string name)
        {
            return this.SelectMany(c => c.Value.Files)
                .Where(f => f.Name.Equals(name))
                .ToList();
        }

        public void DeleteFile(FilePath path)
        {
            var parentDir = GetByDirPath(path.Directory);

            if (parentDir is null) return;

            parentDir.DeleteFile(path);
            InvalidateIdCache();
        }

        protected Func<KeyValuePair<string, IStorageCacheItem>, bool> FileExcludeFilter(IEnumerable<DirectoryPath> excludePaths) =>
            kvp => !excludePaths
                .Select(p => p.Value)
                .Any(excludePath => kvp.Key.Contains(excludePath));

        public IEnumerable<LaunchableItem> Search(string searchText, IEnumerable<DirectoryPath> excludePaths, List<string> stopSearchWords, SearchWeights searchWeights, params TeensyFileType[] fileTypes)
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
            searchTerms.RemoveAll(term => stopSearchWords.Contains(term.ToLower()));

            searchTerms.AddRange(quotedMatches);

            var requiredTerms = searchTerms
                .Where(term => term.StartsWith("+"))
                .Select(term => term.Substring(1))
                .ToList();

            searchTerms = searchTerms.Select(term => term.TrimStart('+')).ToList();

            return this
                .Where(FileExcludeFilter(excludePaths))
                .SelectMany(c => c.Value.Files)
                .OfType<LaunchableItem>()
                .Where(f => fileTypes.Contains(f.FileType) &&
                            requiredTerms.All(requiredTerm =>
                                f.Title.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Name.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Creator.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Path.Value.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase) ||
                                f.Description.Contains(requiredTerm, StringComparison.OrdinalIgnoreCase)))
                .Select(file => new
                {
                    File = file,
                    Score = searchTerms.Sum(term =>
                        (file.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Title : 0) +
                        (file.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.FileName : 0) +
                        (file.Creator.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Creator : 0) +
                        (file.Path.Value.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.FilePath : 0) +
                        (file.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Description : 0))
                })
                .Where(result => result.Score > 0)
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.File.Title)
                .Select(result => result.File);
        }

        public LaunchableItem? GetRandomFile(StorageScope scope, DirectoryPath scopePath, IEnumerable<DirectoryPath> excludePaths, params TeensyFileType[] fileTypes)
        {
            if (fileTypes.Length == 0)
            {
                fileTypes = TeensyFileTypeExtensions.GetLaunchFileTypes();
            }
            var selection = this
                .Where(FileExcludeFilter(excludePaths))
                .SelectMany(c => c.Value.Files)
                .Where(f => fileTypes.Contains(f.FileType))
                .Where(f => scope switch
                {
                    StorageScope.DirDeep => f.Path.Value.StartsWith(scopePath.ToString()),
                    StorageScope.DirShallow => f.Path.Directory.Equals(scopePath),
                    StorageScope.Storage => f.Path.Value.StartsWith(scopePath.ToString()),
                    _ => true
                })
                .OfType<LaunchableItem>()
                .ToArray();

            if (selection.Length == 0) return null;

            return selection[new Random().Next(selection.Length)];
        }

        public void EnsurePlaylists() 
        {
            var playlistItems = GetPlaylistFiles();

            foreach (var p in playlistItems)
            {
                var parent = FindParentFile(p);
                if (parent is not null)
                {
                    MapParentMetadata(p, parent);
                }
            }
        }

        protected static void MapParentMetadata(LaunchableItem itemToMap, FileItem parent)
        {
            // ParentPath is now computed from Path.Directory, no need to assign it
            itemToMap.MetadataSource = parent.MetadataSource;
            itemToMap.MetadataSourcePath = parent.MetadataSourcePath;
            itemToMap.Meta1 = parent.Meta1;
            itemToMap.Meta2 = parent.Meta1;
            itemToMap.Creator = parent.Creator;
            itemToMap.ReleaseInfo = parent.ReleaseInfo;
            itemToMap.Description = parent.Description;
            itemToMap.ShareUrl = parent.ShareUrl;
            itemToMap.Title = parent.Title;
        }

        public void EnsureFavorites()
        {
            List<LaunchableItem> favsToFavorite = GetFavoriteFiles();
            var fileIdMap = GetOrBuildFileIdCache();
            
            foreach (var f in favsToFavorite)
            {
                f.IsFavorite = true;
                
                var parent = FindParentFile(f);

                if (parent is not null) 
                {
                    MapParentMetadata(f, parent);
                    parent.IsFavorite = true;
                    UpsertFile(parent);
                    UpsertFile(f);
                }
            }
            
            var allSiblings = FindSiblings(favsToFavorite);
            
            foreach (var s in allSiblings)
            {
                s.IsFavorite = true;
                UpsertFile(s);
            }
        }

        protected Dictionary<string, List<FileItem>> GetOrBuildFileIdCache()
        {
            if (_fileIdCache != null)
            {
                return _fileIdCache;
            }

            var newCache = new Dictionary<string, List<FileItem>>();
            
            foreach (var item in this.SelectMany(c => c.Value.Files))
            {
                if (!newCache.TryGetValue(item.Id, out var fileList))
                {
                    fileList = [];
                    newCache[item.Id] = fileList;
                }
                fileList.Add(item);
            }

            _fileIdCache = newCache;
            return _fileIdCache;
        }

        public FileItem? FindParentFile(FileItem file)
        {
            var fileIdMap = GetOrBuildFileIdCache();
            List<DirectoryPath> excludePaths = [..StorageHelper.FavoritePaths, new DirectoryPath(StorageHelper.Playlist_Path)];

            if (!fileIdMap.TryGetValue(file.Id, out var candidates))
            {
                return null;
            }

            return candidates.FirstOrDefault(f => 
                !excludePaths.Any(ep => f.Path.Value.Contains(ep.Value)));
        }

        public List<FileItem> FindSiblings(FileItem item) 
        {
            var fileIdMap = GetOrBuildFileIdCache();
            
            if (!fileIdMap.TryGetValue(item.Id, out var candidates))
            {
                return [];
            }
            
            return candidates
                .Where(f => (StorageHelper.FavoritePaths.Any(p => f.Path.Contains(p)) || 
                            f.Path.Value.StartsWith(StorageHelper.Playlist_Path)) &&
                            !f.Path.Equals(item.Path))
                .ToList();
        }
        
        public List<FileItem> FindSiblings(List<LaunchableItem> files)
        {
            var fileIdMap = GetOrBuildFileIdCache();
            HashSet<string> processedPaths = new HashSet<string>();
            List<FileItem> result = new List<FileItem>();
            
            foreach (var file in files)
            {
                if (!fileIdMap.TryGetValue(file.Id, out var candidates))
                {
                    continue;
                }
                
                foreach (var candidate in candidates)
                {
                    if ((StorageHelper.FavoritePaths.Any(p => candidate.Path.Contains(p)) || 
                         candidate.Path.Value.StartsWith(StorageHelper.Playlist_Path)) &&
                        !processedPaths.Contains(candidate.Path.Value) &&
                        !files.Any(f => f.Path.Value == candidate.Path.Value))
                    {
                        processedPaths.Add(candidate.Path.Value);
                        result.Add(candidate);
                    }
                }
            }
            
            return result;
        }

        public List<LaunchableItem> GetFavoriteFiles()
        {
            List<LaunchableItem> favs = [];

            foreach (var target in StorageHelper.FavoritePaths)
            {
                favs.AddRange(this
                    .Where(c => c.Key.RemoveLeadingAndTrailingSlash().Contains(target.Value.RemoveLeadingAndTrailingSlash()))
                    .SelectMany(c => c.Value.Files)
                    .ToList()
                    .OfType<LaunchableItem>());
            }
            return favs;
        }

        public List<LaunchableItem> GetPlaylistFiles() 
        {
            return this.Where(f => f.Key.Contains(StorageHelper.Playlist_Path))
                .SelectMany(c => c.Value.Files)
                .OfType<LaunchableItem>()
                .ToList();
        }

        /// <summary>
        /// Reads the cache data from disk.
        /// </summary>
        public virtual void ReadFromDisk()
        {
            _ioLock.EnterWriteLock();
            try
            {
                if (!File.Exists(CacheFilePath))
                {
                    Clear();
                    InvalidateIdCache();
                    return;
                }

                using var stream = File.Open(CacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(stream);
                var content = reader.ReadToEnd();

                var cacheFromDisk = StorageCacheItemSerializer.Deserialize<Dictionary<string, IStorageCacheItem>>(content);

                if (cacheFromDisk is null) return;

                Clear();
                InvalidateIdCache();

                foreach (var item in cacheFromDisk)
                {
                    TryAdd(item.Key, item.Value);
                }
            }
            finally
            {
                _ioLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Writes the cache data to disk.
        /// </summary>
        public virtual void WriteToDisk()
        {
            _ioLock.EnterReadLock();
            try
            {
                var directory = Path.GetDirectoryName(CacheFilePath);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                File.WriteAllText(CacheFilePath, StorageCacheItemSerializer.Serialize(this as Dictionary<string, IStorageCacheItem>));
            }
            finally
            {
                _ioLock.ExitReadLock();
            }
        }

        public int GetCacheSize() => this.Aggregate(0, (acc, item) => acc + item.Value.Files.Count);

        protected bool IsBannedFolder(DirectoryPath folder)
        {
            if (folder.Equals(StorageHelper.Remote_Path_Root)) return false;

            return BannedDirectories.Any(banned => folder.Equals(banned));
        }

        protected virtual bool IsBannedFile(string fileName)
        {
            return BannedFiles.Any(banned => 
                banned.RemoveLeadingAndTrailingSlash().Equals(
                    fileName.RemoveLeadingAndTrailingSlash(), 
                    StringComparison.OrdinalIgnoreCase));
        }

        protected IStorageCacheItem CleanBadFilesAndFolders(IStorageCacheItem cacheItem)
        {
            cacheItem.Directories = cacheItem.Directories
                .Where(d => !IsBannedFolder(d.Path))
                .ToList();

            cacheItem.Files = cacheItem.Files
                .Where(f => !IsBannedFile(f.Name))
                .ToList();

            return cacheItem;
        }
    }
}