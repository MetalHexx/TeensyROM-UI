using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class MidiSettingsViewModel :ReactiveObject
    {
        [Reactive] public bool MidiEnabled { get; set; }
        [Reactive] public List<MidiMappingViewModel> Mappings { get; set; } = [];
        [Reactive] public List<NoteMappingViewModel> NoteMappings { get; set; } = [];
        [Reactive] public List<DualNoteMappingViewModel> DualNoteMappings { get; set; } = [];
        [Reactive] public List<CCMappingViewModel> CCMappings { get; set; } = [];
        public ReactiveCommand<Unit, Unit> RefreshMidiDevicesCommand { get; set; }

        private IMidiService _midiService;
        private IAlertService _alert;

        public MidiSettingsViewModel(MidiSettingsViewModel s, IMidiService midiService, IAlertService alert)
        {
            _alert = alert;
            _midiService = midiService;
            MidiEnabled = s.MidiEnabled;
            MapMappings(s.Mappings);
            RefreshMidiDevices();

            RefreshMidiDevicesCommand = ReactiveCommand.Create(RefreshMidiDevices);
        }

        public MidiSettingsViewModel(MidiSettings s, IMidiService midiService, IAlertService alert)
        {
            _alert = alert;
            _midiService = midiService;
            MidiEnabled = s.MidiEnabled;            
            MapMappings(s.Mappings);
            RefreshMidiDevices();
            RefreshMidiDevicesCommand = ReactiveCommand.Create(RefreshMidiDevices);
        }

        private void RefreshMidiDevices()
        {
            var engaged = _midiService.IsMidiEngaged;

            if (engaged) _midiService.DisengageMidi();

            var devices = _midiService
                .GetMidiDevices()
                .Select(d => new MidiDeviceViewModel(d)).ToList();

            foreach (var mapping in Mappings)
            {
                mapping.AvailableDevices = devices;
                mapping.Device = devices.FirstOrDefault(d => d.Name == mapping.Device?.UnboundName) ?? mapping.Device;
            }

            if (engaged) _midiService.EngageMidi();
        }

        private void MapMappings(List<MidiMapping> mappings) 
        {
            foreach (var mapping in mappings)
            {
                MidiMappingViewModel? viewModel = mapping switch 
                {
                    DualNoteMapping dualNoteMapping => new DualNoteMappingViewModel(dualNoteMapping, _midiService, _alert),
                    NoteMapping noteMapping => new NoteMappingViewModel(noteMapping, _midiService, _alert),
                    CCMapping ccMapping => new CCMappingViewModel(ccMapping, _midiService, _alert),                    
                    _ => null
                };
                if (viewModel != null)
                {
                    Mappings.Add(viewModel);
                }
            }
            OrganizeMappings();
        }

        private void MapMappings(List<MidiMappingViewModel> mappings)
        {
            foreach (var mapping in mappings)
            {
                Mappings.Add(mapping);
            }
            OrganizeMappings();
        }

        private void OrganizeMappings() 
        {
            NoteMappings.Clear();

            NoteMappings.AddRange(Mappings
                .Where(m => m is NoteMappingViewModel and not DualNoteMappingViewModel)
                .Cast<NoteMappingViewModel>());

            DualNoteMappings.AddRange(Mappings
                .OfType<DualNoteMappingViewModel>());

            CCMappings.AddRange(Mappings
                .OfType<CCMappingViewModel>());
        }

        public MidiSettings ToMidiSettings()
        {
            return new MidiSettings
            {
                MidiEnabled = MidiEnabled,
                Mappings = Mappings.Select(m => m.ToMidiMapping()).ToList()
            };
        }
    }
}