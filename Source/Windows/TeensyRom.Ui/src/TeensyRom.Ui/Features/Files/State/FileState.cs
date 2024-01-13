using DynamicData;
using MaterialDesignThemes.Wpf;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Commands.File.LaunchFile;
using TeensyRom.Core.Common;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Files.State
{
    public class FileState : IFileState, IDisposable
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<int> CurrentPage => _currentPage.AsObservable();
        public IObservable<int> TotalPages => _totalPages.AsObservable();
        public IObservable<int> PageSize => _pageSize.AsObservable();
        public IObservable<bool> PagingEnabled => _pagingEnabled.AsObservable();

        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly BehaviorSubject<StorageCacheItem?> _currentDirectory = new(null);
        private FileItem? _currentFile = null;

        private string _currentPath = string.Empty;
        private readonly BehaviorSubject<int> _currentPage = new(1);
        private readonly BehaviorSubject<int> _totalPages = new(1);
        private readonly BehaviorSubject<int> _pageSize = new(250);
        private readonly BehaviorSubject<bool> _pagingEnabled = new(false);

        private readonly ICachedStorageService _storageService;
        private readonly ISettingsService _settingsService;
        private readonly IMediator _mediator;
        private readonly ISnackbarService _alert;
        private TeensySettings _settings = new();
        private IDisposable _settingsSubscription;
        private int _skip => (_currentPage.Value - 1) * _pageSize.Value;

        public FileState(ICachedStorageService storageService, ISettingsService settingsService, IMediator mediator, ISnackbarService alert)
        {
            _storageService = storageService;
            _settingsService = settingsService;
            _mediator = mediator;
            _alert = alert;
            _settingsSubscription = _settingsService.Settings.Subscribe(settings => OnSettingsChanged(settings));

        }
        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            _directoryContent.OnNext([]);
            ResetDirectoryTree();
        }
        private void ResetDirectoryTree()
        {
            var musicLibraryPath = _settings.GetLibraryPath(TeensyLibraryType.Music);

            var dirItem = new DirectoryNodeViewModel
            {
                Name = "Fake Root",  //TODO: Fake root required since UI view binds to enumerable -- design could use improvement
                Path = "Fake Root",
                Directories =
                [
                    new DirectoryNodeViewModel
                    {
                        Name = "/",
                        Directories = []
                    }
                ]
            };
            _directoryTree.OnNext(dirItem);
        }

        public async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            if(_currentPath != path)
            {
                _currentPath = path;                   
                _currentPage.OnNext(1);
            }

            var directoryResult = await _storageService.GetDirectory(path);

            if (directoryResult is null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
                _directoryTree.Value.SelectDirectory(path);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var items = new List<StorageItem>();
            items.AddRange(directoryResult.Directories);
            items.AddRange(directoryResult.Files);


            var directoryItems = new ObservableCollection<StorageItem>();

            var skip = _skip;
            
            if (filePathToSelect is not null)
            {
                var fileToSelect = items.FirstOrDefault(i => i.Path == filePathToSelect);
                if (fileToSelect != null)
                {
                    items.Where(items => items is FileItem)
                         .ToList()
                         .ForEach(i => i.IsSelected = false);                    

                    fileToSelect.IsSelected = true;
                    var selectedList = items.Select(f => $"{f.IsSelected} - {f.Name}").ToList();
                    int fileIndex = items.IndexOf(fileToSelect);
                    _currentPage.OnNext((int)Math.Ceiling((double)(fileIndex + 1) / _pageSize.Value));
                    skip = (_currentPage.Value - 1) * _pageSize.Value;
                }
            }

            directoryItems.AddRange(items.Skip(skip).Take(_pageSize.Value));

            _totalPages.OnNext((int)Math.Ceiling((double)items.Count / _pageSize.Value));
            _pagingEnabled.OnNext(_totalPages.Value > 1);
            _directoryContent.OnNext(directoryItems);
            _currentDirectory.OnNext(directoryResult);
            _directoryTree.OnNext(_directoryTree.Value);

            return;
        }

        public async Task RefreshDirectory(bool bustCache = true)
        {
            if (_currentDirectory.Value is null) return;

            if (bustCache) _storageService.ClearCache(_currentDirectory.Value.Path);

            await LoadDirectory(_currentDirectory.Value.Path);
        }

        public async Task StoreFiles(IEnumerable<FileCopyItem> files)
        {
            var commonParent = GetCommonBasePath(files.Select(i => i.Path));
            var directoryOnly = files.All(i => i.InSubdirectory);

            foreach (var file in files)
            {
                var fileInfo = new TeensyFileInfo(file.Path);

                var finalPath = directoryOnly
                    ? Directory.GetParent(commonParent)?.FullName
                    : commonParent;

                var relativePath = fileInfo.FullPath
                    .Replace(finalPath!, string.Empty)
                    .ToUnixPath()
                    .GetUnixParentPath();
                
                fileInfo.TargetPath = _currentDirectory.Value!.Path.UnixPathCombine(relativePath);
                await _storageService.SaveFile(fileInfo);
            }
            await RefreshDirectory(bustCache: false);
        }

        private static string GetCommonBasePath(IEnumerable<string> directories)
        {
            if (!directories.Any()) return string.Empty;

            string commonPath = directories.First();

            foreach (string path in directories)
            {
                while (!path.StartsWith(commonPath, StringComparison.OrdinalIgnoreCase))
                {
                    commonPath = commonPath.Substring(0, commonPath.LastIndexOf(Path.DirectorySeparatorChar));
                }
            }
            return commonPath;
        }

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }

        public Task LaunchFile(FileItem file) => _mediator.Send(new LaunchFileCommand { Path = file.Path });

        public async Task SaveFavorite(FileItem file)
        {
            var favFile = await _storageService.SaveFavorite(file);
            var parentDir = favFile?.Path.GetUnixParentPath();

            if (parentDir is null) return;

            var directoryResult = await _storageService.GetDirectory(parentDir);

            if (directoryResult is null) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var favParentPath = favFile?.Path.GetUnixParentPath().GetUnixParentPath().RemoveLeadingAndTrailingSlash();
            var currentPath = _currentDirectory.Value?.Path.RemoveLeadingAndTrailingSlash();

            if (favParentPath == currentPath) 
            {
                await RefreshDirectory(bustCache: false);
            }
        }

        public async Task DeleteFile(FileItem file)
        {
            await _storageService.DeleteFile(file, _settings.TargetType);
            await RefreshDirectory(bustCache: false);
        }

        public async Task PlayRandom()
        {
            var file = _storageService.GetRandomFile();

            if (file is null)
            {
                _alert.Enqueue("Random search requires visiting at least one directory with files in it first.  Try the cache button next to the dice for best results.");
                return;
            }

            await LoadDirectory(file.Path.GetUnixParentPath(), file.Path);
            await LaunchFile(file);
        }

        public Task NextPage()
        {
            if (_currentPage.Value == _totalPages.Value)
            {
                return Task.CompletedTask;
            }
            _currentPage.OnNext(_currentPage.Value + 1);
            return LoadDirectory(_currentPath);
        }

        public Task PreviousPage()
        {
            if(_currentPage.Value > 1)
            {
                _currentPage.OnNext(_currentPage.Value - 1);
            }
            return LoadDirectory(_currentPath);
        }

        public Task SetPageSize(int pageSize)
        {
            _pageSize.OnNext(pageSize);
            _currentPage.OnNext(1);
            return LoadDirectory(_currentPath);
        }

        public async Task CacheAll()
        {          
            await _storageService.CacheAll();
        }

        public Unit SearchFiles(string searchText)
        {
            var searchResult = _storageService.SearchFiles(searchText);

            if (searchResult is null) return Unit.Default;

            _directoryContent.OnNext(new ObservableCollection<StorageItem>(searchResult));
            return Unit.Default;
        }

        public Task ClearSearch()
        {
            return LoadDirectory(_currentDirectory.Value?.Path ?? "/");
        }
    }
}