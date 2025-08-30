using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Controls.DirectoryTree
{
    public abstract class DirectoryTreeState : IDirectoryTreeState
    {
        public IObservable<DirectoryNodeViewModel> DirectoryTree => _directoryTree.AsQbservable();
        private readonly BehaviorSubject<DirectoryNodeViewModel> _directoryTree = new(new DirectoryNodeViewModel());

        public void ResetDirectoryTree(DirectoryPath rootPath)
        {
            var dirItem = new DirectoryNodeViewModel
            {
                Name = "Fake Root",  //TODO: Fake root required since UI view binds to enumerable -- design could use improvement
                Path = new DirectoryPath("Fake Root"),
                Directories =
                [
                    new DirectoryNodeViewModel
                    {
                        Name = rootPath.DirectoryName,
                        Path = rootPath,
                        Directories = []
                    }
                ]
            };
            _directoryTree.OnNext(dirItem);
        }

        public void SelectDirectory(DirectoryPath path)
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
