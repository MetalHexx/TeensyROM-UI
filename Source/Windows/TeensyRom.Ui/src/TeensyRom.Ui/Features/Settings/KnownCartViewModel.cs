using ReactiveUI.Fody.Helpers;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Features.Settings
{
    public class KnownCartViewModel
    {
        public string DeviceHash { get; set; } = string.Empty;
        public string PnpDeviceId { get; set; } = string.Empty;
        public string ComPort { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IFileItem? LastFile { get; set; } = null;
        [Reactive] public MidiSettingsViewModel MidiSettings { get; set; }
        public KnownCartViewModel(KnownCart cart, IMidiService midiService, IAlertService alert)
        {
            DeviceHash = cart.DeviceHash;
            PnpDeviceId = cart.PnpDeviceId;
            ComPort = cart.ComPort;
            Name = cart.Name;
            MidiSettings = new MidiSettingsViewModel(cart.MidiSettings ?? new(), midiService, alert);
        }

        public KnownCartViewModel(KnownCartViewModel cart, IMidiService midiService, IAlertService alert)
        {
            DeviceHash = cart.DeviceHash;
            PnpDeviceId = cart.PnpDeviceId;
            ComPort = cart.ComPort;
            Name = cart.Name;
            MidiSettings = new MidiSettingsViewModel(cart.MidiSettings, midiService, alert);
        }
    }
}