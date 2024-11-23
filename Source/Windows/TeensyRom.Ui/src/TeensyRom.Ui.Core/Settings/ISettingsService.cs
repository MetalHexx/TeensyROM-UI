using System.Reactive;

namespace TeensyRom.Ui.Core.Settings
{
    public interface ISettingsService
    {
        IObservable<TeensySettings> Settings { get; }
        bool SaveSettings(TeensySettings settings);
        TeensySettings GetSettings();
    }
}