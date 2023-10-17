using System.Reactive;

namespace TeensyRom.Core.Settings
{
    public interface ISettingsService
    {
        IObservable<TeensySettings> Settings { get; }
        IObservable<string> Logs { get; }
        Unit SaveSettings(TeensySettings settings);        
    }
}