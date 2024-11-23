using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Ui.Core.Settings;
using TeensyRom.Ui.Core.Storage.Entities;

namespace TeensyRom.Ui.Controls.StorageSelector
{
    public class StorageSelectorViewModel : ReactiveObject, IDisposable
    {
        [Reactive] public List<TeensyStorageType> Storage { get; set; } = [TeensyStorageType.SD, TeensyStorageType.USB];
        [Reactive] public TeensyStorageType SelectedStorage { get; set; }
        private TeensySettings _settings = null!;
        private IDisposable _settingsSubscription = null!;

        public StorageSelectorViewModel(ISettingsService settingsService)
        {
            _settingsSubscription = settingsService.Settings
                .Where(s => s is not null)
                .Select(s => s with { })
                .Subscribe(s =>
                {
                    _settings = s;
                    SelectedStorage = s.StorageType;
                });

            this.WhenAnyValue(x => x.SelectedStorage)
                .Skip(1)
                .Subscribe(storageType =>
                {
                    _settings.StorageType = storageType;
                    settingsService.SaveSettings(_settings);
                });
        }

        public void Dispose()
        {
            _settingsSubscription.Dispose();
        }
    }
}
