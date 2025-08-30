using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TeensyRom.Core.Entities.Storage
{
    public abstract class StorageItem : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }

        private bool _isSelected;

        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        private bool _isFavorite;
        public bool IsFavorite
        {
            get => _isFavorite;
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    OnPropertyChanged(nameof(IsFavorite));
                }
            }
        }

        private bool _isCompatible = true;
        public bool IsCompatible
        {
            get => _isCompatible;
            set
            {
                if (_isCompatible != value)
                {
                    _isCompatible = value;
                    OnPropertyChanged(nameof(IsCompatible));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
