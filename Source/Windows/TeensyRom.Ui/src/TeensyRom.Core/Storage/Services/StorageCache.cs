using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace TeensyRom.Core.Storage.Services
{
    /// <summary>
    /// - random automatically puts you into shuffle mode
    /// - clicking next in shuffle mode will be added to a history list to remember your previous track
    /// - hitting previous in shuffle mode will now bring to the previous item in the history list
    /// - hitting previous in shuffle mode when you're at the beginning of the history will do nothing.
    /// - clicking a song in your current directory will pull you out of shuffle mode and clear history.Clicking next or previous will go to the next or previous song in the current directory.
    /// </summary>
    public class StorageCache : Dictionary<string, StorageCacheItem>, IStorageCache
    {
        private List<string> _bannedFolders = [];
        private List<string> _bannedFiles = [];

        public StorageCache() { }
        public StorageCache(List<string> bannedFolders, List<string> bannedFiles)
        {
            _bannedFolders = bannedFolders;
            _bannedFiles = bannedFiles;
        }
        public void SetBanLists(List<string> bannedFolders, List<string> bannedFiles)
        {
            _bannedFolders = bannedFolders;
            _bannedFiles = bannedFiles;
        }
        public void UpsertDirectory(string path, StorageCacheItem directory)
        {
            if(IsBannedFolder(path)) return;

            DeleteDirectory(path);
            Insert(path, directory);
        }

        public void UpsertFile(IFileItem fileItem)
        {
            if (IsBannedFile(fileItem.Path)) return;

            var fileParentDir = EnsureParents(fileItem.Path);

            fileParentDir!.UpsertFile(fileItem);
            UpsertDirectory(fileParentDir.Path, fileParentDir);
        }

        public StorageCacheItem EnsureParents(string path)
        {
            var parentPath = CleanPath(path.GetUnixParentPath());
            var fileParentDir = GetByDirPath(parentPath);

            if (fileParentDir is null)
            {
                fileParentDir = new StorageCacheItem
                {
                    Path = parentPath,
                    Directories = [],
                    Files = []
                };
                Insert(fileParentDir.Path, fileParentDir);
            }
            if (string.IsNullOrWhiteSpace(parentPath)) return fileParentDir;

            var grandParent = EnsureParents(parentPath);

            grandParent.InsertSubdirectory(new DirectoryItem
            {
                Name = fileParentDir.Path.GetLastDirectoryFromPath(),
                Path = fileParentDir.Path,
            });
            return fileParentDir;
        }

        private void Insert(string path, StorageCacheItem cacheItem)
        {
            if (IsBannedFolder(path)) return;

            cacheItem = CleanBadFilesAndFolders(cacheItem);
            var cleanPath = CleanPath(path);
            TryAdd(cleanPath, cacheItem);
        }

        public void DeleteDirectory(string path)
        {
            var cleanPath = CleanPath(path);
            var dir = GetByDirPath(cleanPath);

            if (dir is null) return;

            Remove(cleanPath);
        }

        public void DeleteDirectoryWithChildren(string path)
        {
            var currentDir = GetByDirPath(path);

            if (currentDir is null) return;

            foreach (var directory in currentDir.Directories)
            {
                DeleteDirectoryWithChildren(directory.Path);
            }
            DeleteDirectory(currentDir.Path);
        }

        public StorageCacheItem? GetByDirPath(string path)
        {
            var cleanPath = CleanPath(path);

            if (!TryGetValue(cleanPath, out var item)) return null;

            return item;
        }

        public IFileItem? GetFileByPath(string filePath)
        {
            var parentPath = CleanPath(filePath.GetUnixParentPath());
            TryGetValue(parentPath, out StorageCacheItem? dir);

            if (dir is not null)
            {
                return dir.Files.FirstOrDefault(f => f.Path.Equals(filePath));
            }
            return null;
        }

        public List<IFileItem> GetFileByName(string name)
        {
            return this.SelectMany(c => c.Value.Files)
                .Where(f => f.Name.Equals(name))
                .ToList();
        }

        public void DeleteFile(string path)
        {
            var cleanPath = CleanPath(path);
            var parentPath = cleanPath.GetUnixParentPath();
            var parentDir = GetByDirPath(parentPath);

            if (parentDir is null) return;

            parentDir.DeleteFile(path);
        }

        private Func<KeyValuePair<string, StorageCacheItem>, bool> NotFavoriteFilter(List<string> favPaths) =>
            kvp => !favPaths
                .Select(p => p.RemoveLeadingAndTrailingSlash())
                .Any(favPath => kvp.Key.Contains(favPath));

        public IEnumerable<ILaunchableItem> Search(string searchText, List<string> favPaths, List<string> stopSearchWords, SearchWeights searchWeights, params TeensyFileType[] fileTypes)
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
                .Where(NotFavoriteFilter(favPaths))
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
                        (file.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Title : 0) +
                        (file.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.FileName : 0) +
                        (file.Creator.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Creator : 0) +
                        (file.Path.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.FilePath : 0) +
                        (file.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ? searchWeights.Description : 0))
                })
                .Where(result => result.Score > 0)
                .OrderByDescending(result => result.Score)
                .ThenBy(result => result.File.Title)
                .Select(result => result.File);
        }

        public ILaunchableItem? GetRandomFile(StorageScope scope, string scopePath, params TeensyFileType[] fileTypes)
        {
            scopePath = $"{scopePath.RemoveLeadingAndTrailingSlash().EnsureUnixPathEnding()}";

            if (fileTypes.Length == 0)
            {
                fileTypes = TeensyFileTypeExtensions.GetLaunchFileTypes();
            }
            var selection = this
                .SelectMany(c => c.Value.Files)
                .Where(f => fileTypes.Contains(f.FileType))
                .Where(f => scope switch
                {
                    StorageScope.DirDeep => f.Path
                        .RemoveLeadingAndTrailingSlash()
                        .StartsWith(scopePath.RemoveLeadingAndTrailingSlash()),

                    StorageScope.DirShallow => f.Path
                        .GetUnixParentPath()
                        .RemoveLeadingAndTrailingSlash()
                        .EnsureUnixPathEnding() == scopePath
                            .RemoveLeadingAndTrailingSlash()
                            .EnsureUnixPathEnding(),

                    _ => true
                })
                .OfType<ILaunchableItem>()
                .ToArray();

            if (selection.Length == 0) return null;

            return selection[new Random().Next(selection.Length - 1)];
        }

        public void EnsureFavorites(List<string> favPaths)
        {
            List<ILaunchableItem> favsToFavorite = GetFavoriteItemsFromCache(favPaths);

            favsToFavorite.ForEach(fav => fav.IsFavorite = true);

            this
                .Where(NotFavoriteFilter(favPaths))
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
                    UpsertFile(parentFile);
                    UpsertFile(fav);
                });
        }

        public List<ILaunchableItem> GetFavoriteItemsFromCache(List<string> favPaths)
        {
            List<ILaunchableItem> favs = [];

            foreach (var target in favPaths)
            {
                favs.AddRange(this
                    .Where(c => c.Key.RemoveLeadingAndTrailingSlash().Contains(target.RemoveLeadingAndTrailingSlash()))
                    .SelectMany(c => c.Value.Files)
                    .ToList()
                    .Cast<ILaunchableItem>());
            }
            return favs;
        }

        private static string CleanPath(string path) => path
            .RemoveLeadingAndTrailingSlash();

        private bool IsBannedFolder(string folder) 
        {
            if (folder == StorageConstants.Remote_Path_Root) return false;

            return _bannedFolders.Any(b => b.RemoveLeadingAndTrailingSlash().Equals(folder.RemoveLeadingAndTrailingSlash()));
        }
        private bool IsBannedFile(string fileName) => _bannedFiles.Any(b => b.RemoveLeadingAndTrailingSlash().Equals(fileName.RemoveLeadingAndTrailingSlash()));

        private StorageCacheItem CleanBadFilesAndFolders(StorageCacheItem cacheItem)
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