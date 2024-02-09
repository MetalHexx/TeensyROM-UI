using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TeensyRom.Ui.Helpers
{
    /// <summary>
    /// Base class for adding Notify Property changed functionality
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName ?? string.Empty);
            return true;
        }
    }
}