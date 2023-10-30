using System.Reactive;
using TeensyRom.Core.Settings.Entities;

namespace TeensyRom.Core.Settings.Services
{
    public interface ISettingsService
    {
        IObservable<TeensySettings> Settings { get; }
        bool SaveSettings(TeensySettings settings);
    }
}