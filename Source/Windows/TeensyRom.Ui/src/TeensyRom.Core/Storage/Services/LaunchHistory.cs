using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Storage.Services
{
    public class LaunchHistory : ILaunchHistory
    {
        private readonly List<Tuple<int, FileItem>> _history = [];
        private int _currentIndex = -1;

        public void Add(FileItem fileItem) 
        {
            if (_currentIndex < _history.Count - 1) 
            {
                _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
            }
            _currentIndex = _history.Count;
            _history.Add(new(_currentIndex, fileItem));
        }
        public void Clear() => _history.Clear();

        public FileItem? GetPrevious(params TeensyFileType[] types)
        {
            var filteredList = FilterByTypes(types);

            int currentFilteredIndex = filteredList.FindLastIndex(tuple => tuple.Item1 < _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            return filteredList[currentFilteredIndex].Item2;
        }

        public FileItem? GetNext(params TeensyFileType[] types)
        {
            var filteredList = FilterByTypes(types);

            int currentFilteredIndex = filteredList.FindIndex(tuple => tuple.Item1 > _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            return filteredList[currentFilteredIndex].Item2;
        }

        private List<Tuple<int, FileItem>> FilterByTypes(TeensyFileType[] types) => _history
            .Where(tuple => types.Contains(tuple.Item2.FileType))
            .ToList();
    }
}
