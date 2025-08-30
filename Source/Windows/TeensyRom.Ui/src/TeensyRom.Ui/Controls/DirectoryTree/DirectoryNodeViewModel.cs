using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Linq;
using TeensyRom.Core.Entities.Storage;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using TeensyRom.Core.Common;
using ReactiveUI;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public class DirectoryNodeViewModel : ReactiveObject
    {
        [Reactive] public DirectoryPath Path { get; set; } = new DirectoryPath(string.Empty);
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public bool IsSelected { get; set; }
        [Reactive] public bool IsExpanded { get; set; }
        public ObservableCollection<DirectoryNodeViewModel> Directories { get; set; } = new ObservableCollection<DirectoryNodeViewModel>();

        public void SelectDirectory(DirectoryPath targetPath)
        {
            IsSelected = targetPath.Equals(Path);

            if (Directories.Count == 0)
            {
                bool isParentOfTarget = targetPath.Value.StartsWith(Path.Value) 
                    && Path.Value != "/" 
                    && targetPath.Value != Path.Value;
                
                if (isParentOfTarget)
                {
                    IsSelected = true;
                    return;
                }
            }
            foreach (var directory in Directories)
            {
                directory.SelectDirectory(targetPath);
            }
        }

        public void Insert(IEnumerable<DirectoryItem> newDirectories)
        {
            if (!newDirectories.Any()) return;

            var parentPath = newDirectories.First().Path.ParentPath;

            if (Path.Equals(parentPath))
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