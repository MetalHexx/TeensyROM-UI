using DynamicData;
using MediatR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Music;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Files.State
{
    public class FileState : IFileState, IDisposable
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<ObservableCollection<StorageItem>> DirectoryContent => _directoryContent.AsObservable();
        public IObservable<bool> DirectoryLoading => _directoryLoading.AsObservable();

        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new());
        private readonly Subject<ObservableCollection<StorageItem>> _directoryContent = new();
        private readonly Subject<bool> _directoryLoading = new();
        private readonly BehaviorSubject<StorageCacheItem?> _currentDirectory = new(null);

        private readonly ICachedStorageService _storageService;
        private readonly ISettingsService _settingsService;
        private TeensySettings _settings = new();

        public FileState(ICachedStorageService storageService, ISettingsService settingsService)
        {
            _storageService = storageService;
            _settingsService = settingsService;
            _settingsService.Settings.Subscribe(settings => OnSettingsChanged(settings));

        }
        private void OnSettingsChanged(TeensySettings settings)
        {
            _settings = settings;
            _directoryContent.OnNext(new ObservableCollection<StorageItem>());
            ResetDirectoryTree();
        }

        private void ResetDirectoryTree()
        {
            var dirItem = new DirectoryNodeViewModel
            {
                Name = "",  //The view only maps to the directory enumerable at the top level.  So we create a fake root here.
                Path = "",
                Directories = new()
                {
                    new DirectoryNodeViewModel
                    {
                        Name = _settings.TargetRootPath,
                        Path = _settings.TargetRootPath
                    }
                }
            };
            _directoryTree.OnNext(dirItem);
        }


        public async Task LoadDirectory(string path)
        {
            _directoryLoading.OnNext(true);
            var directoryResult = await _storageService.GetDirectory(path);

            if (directoryResult is null)
            {
                _directoryLoading.OnNext(false);
                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
                _directoryTree.Value.SelectDirectory(path);
            });

            _directoryTree.OnNext(_directoryTree.Value);

            var directoryItems = new ObservableCollection<StorageItem>();
            directoryItems.AddRange(directoryResult.Directories);
            directoryItems.AddRange(directoryResult.Files);

            _directoryContent.OnNext(directoryItems);
            _currentDirectory.OnNext(directoryResult);
            _directoryTree.OnNext(_directoryTree.Value);
            _directoryLoading.OnNext(false);

            return;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
