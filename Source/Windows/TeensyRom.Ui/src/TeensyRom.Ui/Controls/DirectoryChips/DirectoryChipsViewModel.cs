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
        private string _currentPath = string.Empty;

        public DirectoryChipsViewModel(IObservable<string> path, string basePath, Action<string> onClick, Action onCopy) //  /some/directory/path/yay
        {
            var pathItems = path
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Do(path => _currentPath = path)
                .Select(p => p.Replace(basePath, ""))
                .Select(path => path.ToPathArray().Select(item => $"/{item}"))
                .Subscribe(pathList => 
                {
                    PathItems.Clear();
                    PathItems.Add(basePath);
                    PathItems.AddRange(pathList);
                });

            PathItemClickCommand = ReactiveCommand.Create<string>(item =>
            {
                var index = PathItems.IndexOf(item);
                var path = string.Join("", PathItems.Take(index + 1));

                if (path == _currentPath) return;
                
                onClick(path);
            });   
            CopyCommand = ReactiveCommand.Create<string>(_ => 
            {
                Clipboard.SetText(_currentPath);
                onCopy();
            });
        }
    }
}
