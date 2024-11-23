using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Cli.Core.Storage.Entities;

namespace TeensyRom.Cli.Core.Storage.Services
{
    public class LaunchHistory : ILaunchHistory
    {
        private readonly List<Tuple<int, ILaunchableItem>> _history = [];
        private int _currentIndex = -1;        
        public bool CurrentIsNew { get; private set; }

        public void Add(ILaunchableItem fileItem) 
        {
            if (_currentIndex < _history.Count - 1) 
            {
                ClearForwardHistory();
            }
            _currentIndex = _history.Count;
            _history.Add(new(_currentIndex, fileItem));
            CurrentIsNew = true;
        }

        public void Remove(ILaunchableItem fileItem)
        {
            int index = _history.FindIndex(tuple => tuple.Item2 == fileItem);    
            
            if (index == -1) return;            
            
            _history.RemoveAt(index);
            
            if (index <= _currentIndex)
            {
                _currentIndex--;
                CurrentIsNew = false;
            }
        }
        public void Clear() => _history.Clear();

        public void ClearForwardHistory() => _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);

        public ILaunchableItem? GetPrevious(params TeensyFileType[] types)
        {
            var filteredList = _history;

            if (types.Any()) 
            {
                filteredList = FilterByTypes(types);
            }
            int currentFilteredIndex = filteredList.FindLastIndex(tuple => tuple.Item1 < _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            CurrentIsNew = false;
            return filteredList[currentFilteredIndex].Item2;
        }

        public ILaunchableItem? GetNext(params TeensyFileType[] types)
        {
            var filteredList = _history;

            if (types.Any())
            {
                filteredList = FilterByTypes(types);
            }
            int currentFilteredIndex = filteredList.FindIndex(tuple => tuple.Item1 > _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            CurrentIsNew = false;
            return filteredList[currentFilteredIndex].Item2;
        }

        private List<Tuple<int, ILaunchableItem>> FilterByTypes(TeensyFileType[] types) => _history
            .Where(tuple => types.Contains(tuple.Item2.FileType))
            .ToList();
    }
}
