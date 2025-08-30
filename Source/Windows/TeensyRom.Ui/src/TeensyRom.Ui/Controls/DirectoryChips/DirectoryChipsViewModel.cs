using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using TeensyRom.Core.Common;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.ValueObjects;

namespace TeensyRom.Ui.Controls.DirectoryChips
{

    public class DirectoryChipsViewModel : ReactiveObject
    {
        [ObservableAsProperty] public bool PinEnabled { get; } = false;
        public ObservableCollection<string> PathItems { get; set; } = [];
        public ReactiveCommand<string, Unit> PathItemClickCommand { get; set; }
        public ReactiveCommand<string, Unit> PinCommand { get; set; }
        private DirectoryPath _currentPath = new DirectoryPath(string.Empty);

        public DirectoryChipsViewModel(IObservable<DirectoryPath> path, IObservable<DirectoryPath> pinnedDirectory, DirectoryPath basePath, Action<DirectoryPath> onClick, Action<DirectoryPath> onPin)
        {
            var root = new DirectoryPath(StorageHelper.Remote_Path_Root);

            pinnedDirectory
                .CombineLatest(path, (pinned, path) => !pinned.IsEmpty && pinned == path && pinned.Value != StorageHelper.Remote_Path_Root)
                .ToPropertyEx(this, vm => vm.PinEnabled);

            var pathItems = path
                .Where(path => !path.IsEmpty)
                .Do(path => _currentPath = path)
                .Select(p => basePath.Equals(root) ? p : new DirectoryPath(p.Value.Replace(basePath.Value, "")))
                .Select(path => path.Value.ToPathArray()
                    .Select(item => item == root.Value ? root.Value : $"/{item}"))
                .Subscribe(pathList => 
                {
                    PathItems.Clear();

                    if(pathList.Count() == 0)
                    {
                        PathItems.Add(basePath.Value);
                        return;
                    }
                    if(basePath.Equals(root))
                    {
                        PathItems.Add("/root");
                    }
                    PathItems.AddRange(pathList.Where(p => p != StorageHelper.Remote_Path_Root));
                });

            PathItemClickCommand = ReactiveCommand.Create<string>(item =>
            {
                var index = PathItems.IndexOf(item);

                var path = new DirectoryPath(string.Join("", PathItems.Take(index + 1)));

                if (path.Equals(_currentPath)) return;
                
                var newPath = ReplaceRoot(path);
                onClick(newPath);
            });   

            PinCommand = ReactiveCommand.Create<string>(_ =>
            {   
                onPin(_currentPath);
            });


        }
        private DirectoryPath ReplaceRoot(DirectoryPath path)
        {
            var newPath = path;
            var rootPath = new DirectoryPath(StorageHelper.Remote_Path_Root);

            if (path.Equals(rootPath))
            {
                return rootPath;
            }
            if (path.Value.StartsWith("/root"))
            {
                newPath = new DirectoryPath(path.Value.Replace("/root", ""));
            }
            return newPath;
        }
    }
}
