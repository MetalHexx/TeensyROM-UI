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

namespace TeensyRom.Ui.Controls.DirectoryChips
{

    public class DirectoryChipsViewModel : ReactiveObject
    {
        public ObservableCollection<string> PathItems { get; set; } = [];
        public ReactiveCommand<string, Unit> PathItemClickCommand { get; set; }
        public ReactiveCommand<string, Unit> CopyCommand { get; set; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        private string _currentPath = string.Empty;

        public DirectoryChipsViewModel(IObservable<string> path, string basePath, Action<string> onClick, Action onCopy, Func<bool, Task> onRefresh)
        {
            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(
                execute: _ => onRefresh(true),
                outputScheduler: RxApp.MainThreadScheduler);

            var pathItems = path
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Do(path => _currentPath = path)
                .Select(p => basePath == "/" ? p : p.Replace(basePath, ""))
                .Select(path => path.ToPathArray().Select(item => $"/{item}"))
                .Subscribe(pathList => 
                {
                    PathItems.Clear();

                    if(pathList.Count() == 0)
                    {
                        PathItems.Add(basePath);
                        return;
                    }
                    if(basePath == "/")
                    {
                        PathItems.Add("/root");
                    }
                    PathItems.AddRange(pathList);
                });

            PathItemClickCommand = ReactiveCommand.Create<string>(item =>
            {
                var index = PathItems.IndexOf(item);

                var path = string.Join("", PathItems.Take(index + 1));

                if (path == _currentPath) return;
                
                var newPath = ReplaceRoot(path);
                onClick(newPath);
            });   
            CopyCommand = ReactiveCommand.Create<string>(_ => 
            {
                Clipboard.SetText(_currentPath);
                onCopy();
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
