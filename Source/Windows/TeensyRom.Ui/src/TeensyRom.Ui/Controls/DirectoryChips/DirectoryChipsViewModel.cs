using DynamicData;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.XPath;
using TeensyRom.Core.Common;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Ui.Controls.DirectoryChips
{

    public class DirectoryChipsViewModel : ReactiveObject
    {
        public ObservableCollection<string> PathItems { get; set; } = [];
        public ReactiveCommand<string, Unit> PathItemClickCommand { get; set; }
        public ReactiveCommand<string, Unit> PinCommand { get; set; }
        private string _currentPath = string.Empty;

        public DirectoryChipsViewModel(IObservable<string> path, string basePath, Action<string> onClick, Action<string> onPin)
        {
            var root = StorageConstants.Remote_Path_Root;

            var pathItems = path
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Do(path => _currentPath = path)
                .Select(p => basePath == root ? p : p.Replace(basePath, ""))
                .Select(path => path.ToPathArray()
                    .Select(item => item == root ? root : $"/{item}"))
                .Subscribe(pathList => 
                {
                    PathItems.Clear();

                    if(pathList.Count() == 0)
                    {
                        PathItems.Add(basePath);
                        return;
                    }
                    if(basePath == root)
                    {
                        PathItems.Add("/root");
                    }
                    PathItems.AddRange(pathList.Where(p => p != StorageConstants.Remote_Path_Root));
                });

            PathItemClickCommand = ReactiveCommand.Create<string>(item =>
            {
                var index = PathItems.IndexOf(item);

                var path = string.Join("", PathItems.Take(index + 1));

                if (path == _currentPath) return;
                
                var newPath = ReplaceRoot(path);
                onClick(newPath);
            });   

            PinCommand = ReactiveCommand.Create<string>(_ =>
            {   
                onPin(_currentPath);
            });


        }
        private string ReplaceRoot(string path)
        {
            var newPath = path;

            if(path == "/root")
            {
                return "/";
            }
            if (path.StartsWith("/root"))
            {
                newPath = path.Replace("/root", "");
            }
            return newPath;
        }
    }
}
