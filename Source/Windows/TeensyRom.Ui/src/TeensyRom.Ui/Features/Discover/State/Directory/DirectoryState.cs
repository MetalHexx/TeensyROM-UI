using DynamicData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Features.Discover.State.Directory
{
    public class DirectoryState
    {
        public ObservableCollection<StorageItem> DirectoryContent { get; private set; } = new();
        public DirectoryPath CurrentPath => _currentPath ?? new DirectoryPath(string.Empty);
        public int CurrentPage { get; private set; } = 1;
        public int TotalPages { get; private set; } = 1;
        public int PageSize { get; private set; } = 100;
        public bool PagingEnabled { get; private set; } = false;
        private int _skip => (CurrentPage - 1) * PageSize;
        private List<StorageItem> _fullDirectory = [];
        private DirectoryPath? _currentPath;

        public void LoadDirectory(List<StorageItem> fullDirectory, DirectoryPath? path = null, FilePath? filePathToSelect = null)
        {
            if (path is not null && !path.Equals(_currentPath))
            {
                CurrentPage = 1;
                _currentPath = path ?? new DirectoryPath(string.Empty);
            }
            _fullDirectory = fullDirectory;

            var directoryItems = new ObservableCollection<StorageItem>();

            var skip = _skip;

            if (filePathToSelect is not null)
            {
                var fileToSelect = fullDirectory.FirstOrDefault(i => i.Equals(filePathToSelect));

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

        public LaunchableItem? SelectFirst()
        {
            var firstItem = DirectoryContent
                .OfType<LaunchableItem>()
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