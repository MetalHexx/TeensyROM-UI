using DynamicData;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Files.State
{
    public class FileDirectoryState(ICachedStorageService _storageService)
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsObservable();
        public IObservable<int> CurrentPage => _currentPage.AsObservable();
        public IObservable<int> TotalPages => _totalPages.AsObservable();
        public IObservable<int> PageSize => _pageSize.AsObservable();
        public IObservable<bool> PagingEnabled => _pagingEnabled.AsObservable();
        public IObservable<ObservableCollection<IStorageItem>> DirectoryContent => _directoryContent.AsObservable();

        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new());
        private readonly BehaviorSubject<int> _currentPage = new(1);
        private readonly BehaviorSubject<int> _totalPages = new(1);
        private readonly BehaviorSubject<int> _pageSize = new(250);
        private readonly BehaviorSubject<bool> _pagingEnabled = new(false);
        private int _skip => (_currentPage.Value - 1) * _pageSize.Value;
        private List<IStorageItem> _directoryItems = [];
        private readonly Subject<ObservableCollection<IStorageItem>> _directoryContent = new();
        private string _currentDirectoryPath = string.Empty;

        public void ResetDirectoryTree(string rootPath)
        {
            var dirItem = new DirectoryNodeViewModel
            {
                Name = "Fake Root",  //TODO: Fake root required since UI view binds to enumerable -- design could use improvement
                Path = "Fake Root",
                Directories =
                [
                    new DirectoryNodeViewModel
                    {
                        Name = rootPath,
                        Path = rootPath,
                        Directories = []
                    }
                ]
            };
            _directoryTree.OnNext(dirItem);
        }

        public void SetSearchResults(IEnumerable<IStorageItem> items)
        {
            _directoryContent.OnNext(new ObservableCollection<IStorageItem>(items));
        }

        public Task ClearSearchResults() => LoadDirectory(_currentDirectoryPath ?? "/");

        public async Task RefreshDirectory(bool bustCache = true)
        {
            if (string.IsNullOrWhiteSpace(_currentDirectoryPath)) return;

            if (bustCache) _storageService.ClearCache(_currentDirectoryPath);

            await LoadDirectory(_currentDirectoryPath);
        }

        public string GetCurrentPath() => _currentDirectoryPath;

        public async Task LoadDirectory(string path, string? filePathToSelect = null)
        {
            if (_currentDirectoryPath != path)
            {
                _currentDirectoryPath = path;
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

            var fullDirectory = new List<IStorageItem>();
            fullDirectory.AddRange(directoryResult.Directories);
            fullDirectory.AddRange(directoryResult.Files.ToList());


            var directoryItems = new ObservableCollection<IStorageItem>();

            var skip = _skip;

            if (filePathToSelect is not null)
            {
                var fileToSelect = fullDirectory.FirstOrDefault(i => i.Path == filePathToSelect);

                if (fileToSelect != null)
                {
                    fullDirectory.Where(items => items is IFileItem)
                         .ToList()
                         .ForEach(i => i.IsSelected = false);

                    fileToSelect.IsSelected = true;

                    _currentPage.OnNext((int)Math.Ceiling((double)(fullDirectory.IndexOf(fileToSelect) + 1) / _pageSize.Value));

                    skip = (_currentPage.Value - 1) * _pageSize.Value;
                }
            }

            directoryItems.AddRange(fullDirectory.Skip(skip).Take(_pageSize.Value));

            _totalPages.OnNext((int)Math.Ceiling((double)fullDirectory.Count / _pageSize.Value));
            _pagingEnabled.OnNext(_totalPages.Value > 1);
            _directoryContent.OnNext(directoryItems);
            _directoryTree.OnNext(_directoryTree.Value);

            return;
        }

        public void UpdateDirectory(StorageCacheItem directoryResult)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _directoryTree.Value.Insert(directoryResult.Directories);
            });
            _directoryTree.OnNext(_directoryTree.Value);
        }

        public Task GoToNextPage()
        {
            if (_currentPage.Value < _totalPages.Value)
            {
                _currentPage.OnNext(_currentPage.Value + 1);
            }
            return LoadDirectory(_currentDirectoryPath);
        }

        public Task GoToPreviousPage()
        {
            if (_currentPage.Value > 1)
            {
                _currentPage.OnNext(_currentPage.Value - 1);
            }
            return LoadDirectory(_currentDirectoryPath);
        }

        public Task SetPageSize(int size)
        {
            _pageSize.OnNext(size);
            _currentPage.OnNext(1);
            return LoadDirectory(_currentDirectoryPath);
        }
    }
}