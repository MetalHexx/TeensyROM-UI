using System.Reactive.Linq;
using System.Reactive.Subjects;
using TeensyRom.Core.Settings;

namespace TeensyRom.Api.Services
{
    public class SettingsService : ISettingsService
    {
        private BehaviorSubject<TeensySettings> _settings = new(new TeensySettings());
        public IObservable<TeensySettings> Settings => _settings.AsObservable();

        public TeensySettings GetSettings()
        {
            return _settings.Value;
        }

        public bool SaveSettings(TeensySettings settings)
        {
            _settings.OnNext(settings);
            return true;
        }

        public void SetCart(string comPort)
        {
            
        }
    }
}
