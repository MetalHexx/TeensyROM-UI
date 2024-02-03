using DynamicData;
using MaterialDesignThemes.Wpf;
using MediatR;
using ReactiveUI;
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
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;
using TeensyRom.Ui.Features.Common.State;
using TeensyRom.Ui.Features.Files.DirectoryContent;
using TeensyRom.Ui.Features.NavigationHost;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Features.Files.State
{
    public class FileState : IFileState, IDisposable
    {
        public IObservable<FileItem> FileLaunched => _programLaunched.AsObservable();
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryState.DirectoryTree;
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryState.DirectoryContent;
        public IObservable<int> CurrentPage => _directoryState.CurrentPage;
        public IObservable<int> TotalPages => _directoryState.TotalPages;
        public IObservable<bool> PagingEnabled => _directoryState.PagingEnabled;

        private Subject<FileItem> _programLaunched = new();

        private readonly DirectoryState _directoryState;
        private readonly ICachedStorageService _storageService;
        private readonly ISettingsService _settingsService;
        private readonly IMediator _mediator;
        private readonly ISnackbarService _alert;
        private TeensySettings _settings;
        private IDisposable _settingsSubscription;
        

        public FileState(ICachedStorageService storage, ISettingsService settingsService, IMediator mediator, ISnackbarService alert, ISerialStateContext serialContext, INavigationService nav)
        {
            _directoryState = new DirectoryState(storage);
            _storageService = storage;
            _settingsService = settingsService;
            _mediator = mediator;
            _alert = alert;

            _settingsSubscription = _settingsService.Settings
                .Do(settings => _settings = settings)
                .CombineLatest(serialContext.CurrentState, nav.SelectedNavigationView, (settings, serial, navView) => (settings, serial, navView))
                .Where(state => state.serial is SerialConnectedState)
                .Where(state => state.navView?.Type == NavigationLocation.Files)
                .Select(state => (state.settings.TargetRootPath, state.settings.TargetType))
                .DistinctUntilChanged()
                .Do(state => _directoryState.ResetDirectoryTree(_settings!.GetLibraryPath(TeensyLibraryType.Music)))
                .Select(storage => storage.TargetRootPath)                             
                .Subscribe(async path => await _directoryState.LoadDirectory(path)); 
        }
        
        public Task RefreshDirectory(bool bustCache = true) => _directoryState.RefreshDirectory(bustCache);
        public Task LoadDirectory(string path, string? filePathToSelect = null) => _directoryState.LoadDirectory(path, filePathToSelect);
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
                
                fileInfo.TargetPath = _directoryState
                    .GetCurrentPath()
                    .UnixPathCombine(relativePath);

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

        public async Task LaunchFile(FileItem file)
        {
            _programLaunched.OnNext(file.Clone());
            var result = await _mediator.Send(new LaunchFileCommand { Path = file.Path });

            if(result.LaunchResult == LaunchFileResultType.SidError)
            {
                _storageService.MarkIncompatible(file);
                _alert.Enqueue($"{file.Name} is currently unsupported (see logs).");
            }
        }

        public async Task SaveFavorite(FileItem file)
        {
            var favFile = await _storageService.SaveFavorite(file);
            var parentDir = favFile?.Path.GetUnixParentPath();

            if (parentDir is null) return;

            var directoryResult = await _storageService.GetDirectory(parentDir);

            if (directoryResult is null) return;

            _directoryState.UpdateDirectory(directoryResult);

            var favParentPath = favFile?.Path
                .GetUnixParentPath()
                .GetUnixParentPath()
                .RemoveLeadingAndTrailingSlash();

            var currentPath = _directoryState
                .GetCurrentPath()
                .RemoveLeadingAndTrailingSlash();

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
            await _directoryState.LoadDirectory(file.Path.GetUnixParentPath(), file.Path);
            await LaunchFile(file);
        }

        public Unit SearchFiles(string searchText)
        {
            var searchResult = _storageService.SearchFiles(searchText);

            if (searchResult is null) return Unit.Default;

            _directoryState.SetSearchResults(searchResult);
            return Unit.Default;
        }

        public Task ClearSearch() => _directoryState.ClearSearchResults();
        public Task CacheAll() => _storageService.CacheAll();
        public Task NextPage() => _directoryState.GoToNextPage();
        public Task PreviousPage() => _directoryState.GoToPreviousPage();
        public Task SetPageSize(int pageSize) => _directoryState.SetPageSize(pageSize);

        public void Dispose()
        {
            _settingsSubscription?.Dispose();
        }
    }
}