using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public abstract class DirectoryTreeState : IDirectoryTreeState
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsQbservable();
        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new DirectoryNodeViewModel());

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

        public void SelectDirectory(string path)
        {
            _directoryTree.Value.SelectDirectory(path);
            _directoryTree.OnNext(_directoryTree.Value);
        }

        public void Insert(IEnumerable<DirectoryItem> newDirectories)
        {
            _directoryTree.Value.Insert(newDirectories);
            _directoryTree.OnNext(_directoryTree.Value);
        }
    }
}
