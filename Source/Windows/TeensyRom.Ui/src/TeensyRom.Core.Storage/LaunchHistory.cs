using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Storage
{
    public class LaunchHistory : ILaunchHistory
    {
        private readonly List<Tuple<int, LaunchableItem>> _history = [];
        private int _currentIndex = -1;
        public bool CurrentIsNew { get; private set; }

        public void Add(LaunchableItem fileItem)
        {
            if (_currentIndex < _history.Count - 1)
            {
                ClearForwardHistory();
            }
            _currentIndex = _history.Count;
            _history.Add(new(_currentIndex, fileItem));
            CurrentIsNew = true;
        }

        public void Remove(LaunchableItem fileItem)
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

        public LaunchableItem? GetPrevious(params TeensyFileType[] types)
        {
            var filteredList = FilterByTypes(types);

            int currentFilteredIndex = filteredList.FindLastIndex(tuple => tuple.Item1 < _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            CurrentIsNew = false;
            return filteredList[currentFilteredIndex].Item2;
        }

        public LaunchableItem? GetNext(params TeensyFileType[] types)
        {
            var filteredList = FilterByTypes(types);

            int currentFilteredIndex = filteredList.FindIndex(tuple => tuple.Item1 > _currentIndex);

            if (currentFilteredIndex == -1) return null;

            _currentIndex = filteredList[currentFilteredIndex].Item1;
            CurrentIsNew = false;
            return filteredList[currentFilteredIndex].Item2;
        }

        private List<Tuple<int, LaunchableItem>> FilterByTypes(TeensyFileType[] types) => _history
            .Where(tuple => types.Contains(tuple.Item2.FileType))
            .ToList();
    }
}
