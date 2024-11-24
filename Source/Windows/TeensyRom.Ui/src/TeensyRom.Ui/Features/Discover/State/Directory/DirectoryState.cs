using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Discover.State.Directory
{
    public class DirectoryState
    {
        public ObservableCollection<IStorageItem> DirectoryContent { get; private set; } = new();
        public string CurrentPath => _currentPath ?? string.Empty;
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int PageSize { get; private set; } = 100;
        public bool PagingEnabled { get; private set; } = false;
        private int _skip => (CurrentPage - 1) * PageSize;
        private List<IStorageItem> _fullDirectory = [];
        private string? _currentPath;

        public void LoadDirectory(List<IStorageItem> fullDirectory, string? path = null, string? filePathToSelect = null)
        {
            if (path is not null && path != _currentPath)
            {
                CurrentPage = 1;
                _currentPath = path ?? string.Empty;
            }
            _fullDirectory = fullDirectory;

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

        public ILaunchableItem? SelectFirst()
        {
            var firstItem = DirectoryContent
                .Where(item => item is ILaunchableItem)
                .Cast<ILaunchableItem>()
                .FirstOrDefault();

            if (firstItem is not null)
            {
                firstItem.IsSelected = true;
            }
            return firstItem;
        }

        public void GoToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
            LoadDirectory(_fullDirectory);
        }

        public void GoToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
            LoadDirectory(_fullDirectory);
        }

        public void SetPageSize(int size)
        {
            PageSize = size;
            CurrentPage = 1;
            LoadDirectory(_fullDirectory);
        }
    }
}