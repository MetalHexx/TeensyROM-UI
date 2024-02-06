using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;
using TeensyRom.Ui.Controls.DirectoryTree;

namespace TeensyRom.Ui.Features.Common.State
{
    public class PlayerDirectoryState(IDirectoryTreeState _tree) 
    {
        public string CurrentPath { get; private set; } = string.Empty;
        public ObservableCollection<StorageItem> DirectoryContent { get; private set; } = new();
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int PageSize { get; private set; } = 100;
        public bool PagingEnabled { get; private set; } = false;
        private int _skip => (CurrentPage - 1) * PageSize;
        private StorageCacheItem? _cacheItem;

        public void ResetDirectoryTree(string rootPath) => _tree.ResetDirectoryTree(rootPath);

        public void SetSearchResults(IEnumerable<StorageItem> items)
        {
            DirectoryContent = new ObservableCollection<StorageItem>(items);
        }

        public void LoadDirectory()
        {
            LoadDirectory(_cacheItem);
        }

        public void LoadDirectory(StorageCacheItem cacheItem, string? filePathToSelect = null)
        {
            _cacheItem = cacheItem;

            if (CurrentPath != cacheItem.Path)
            {
                CurrentPath = cacheItem.Path;
                CurrentPage = 1;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                _tree.Insert(cacheItem.Directories);
                _tree.SelectDirectory(cacheItem.Path);
            });

            var fullDirectory = new List<StorageItem>();
            fullDirectory.AddRange(cacheItem.Directories);
            fullDirectory.AddRange(cacheItem.Files);

            LoadDirectory(fullDirectory, filePathToSelect);
        }

        public void LoadDirectory(List<StorageItem> fullDirectory, string? filePathToSelect = null)
        {
            var directoryItems = new ObservableCollection<StorageItem>();

            var skip = _skip;

            if (filePathToSelect is not null)
            {
                var fileToSelect = fullDirectory.FirstOrDefault(i => i.Path == filePathToSelect);

                if (fileToSelect != null)
                {
                    fullDirectory.Where(items => items is FileItem)
                         .ToList()
                         .ForEach(i => i.IsSelected = false);

                    fileToSelect.IsSelected = true;

                    CurrentPage = (int)Math.Ceiling((double)(fullDirectory.IndexOf(fileToSelect) + 1) / PageSize);

                    skip = (CurrentPage - 1) * PageSize;
                }
            }

            directoryItems.AddRange(fullDirectory.Skip(skip).Take(PageSize));

            TotalPages = (int)Math.Ceiling((double)fullDirectory.Count / PageSize);
            PagingEnabled = TotalPages > 1;
            DirectoryContent = directoryItems;
        }

        public void ClearSelection()
        {
            DirectoryContent.ToList().ForEach(i => i.IsSelected = false);
        }

        public void UpdateDirectory(StorageCacheItem? directoryResult)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _tree.Insert(directoryResult!.Directories);
            });
        }

        public void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
            LoadDirectory();
        }

        public void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
            LoadDirectory();
        }

        public void SetPageSize(int size)
        {
            PageSize = size;
            CurrentPage = 1;
            LoadDirectory();
        }
    }
}