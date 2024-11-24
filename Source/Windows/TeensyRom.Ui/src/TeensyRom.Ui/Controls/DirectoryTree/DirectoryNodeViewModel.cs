using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using TeensyRom.Ui.Core.Storage.Entities;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using TeensyRom.Core.Common;
using ReactiveUI;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public class DirectoryNodeViewModel : ReactiveObject
    {
        [Reactive] public string Path { get; set; } = string.Empty;
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public bool IsExpanded { get; set; }
        public ObservableCollection<DirectoryNodeViewModel> Directories { get; set; } = new ObservableCollection<DirectoryNodeViewModel>();

        public void SelectDirectory(string directoryPath)
        {
            var normalizedTargetPath = directoryPath.RemoveLeadingAndTrailingSlash().ToLower();
            var normalizedCurrentPath = Path.RemoveLeadingAndTrailingSlash().ToLower();

            IsSelected = normalizedCurrentPath.Equals(normalizedTargetPath);

            if(Directories.Count == 0 && normalizedTargetPath.Contains(normalizedCurrentPath))
            {
                IsSelected = true;
                return;
            }

            foreach (var directory in Directories)
            {
                directory.SelectDirectory(directoryPath);
            }
        }

        public void Insert(IEnumerable<DirectoryItem> newDirectories)
        {
            if (!newDirectories.Any()) return;

            var parentPath = newDirectories.First().Path
                .GetUnixParentPath()
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            var currentPath = Path
                .RemoveLeadingAndTrailingSlash()
                .ToLower();

            if (currentPath.Equals(parentPath))
            {
                AddDirectories(newDirectories);  
                IsExpanded = true;
                return;
            }
            RecurseDirectories(newDirectories);
            return;
        }

        private void RecurseDirectories(IEnumerable<DirectoryItem> newDirectories)
        {
            foreach (var directory in Directories)
            {
                directory.Insert(newDirectories);
            }
        }

        private void AddDirectories(IEnumerable<DirectoryItem> directories)
        {
            foreach (var directory in directories)
            {
                var notDupe = !Directories.Any(d => d.Path == directory.Path);

                if (notDupe)
                {
                    Directories.Add(new DirectoryNodeViewModel()
                    {
                        Name = directory.Name,
                        Path = directory.Path
                    });
                }
            };
        }
    }
}