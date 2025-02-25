using ReactiveUI.Fody.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TeensyRom.Core.Music.Midi;

namespace TeensyRom.Ui.Features.Settings
{
    public class MidiSettingsViewModel
    {
        [Reactive] public MidiMappingViewModel PlayPause { get; set; }
        [Reactive] public MidiMappingViewModel Stop { get; set; }
        [Reactive] public MidiMappingViewModel Next { get; set; }
        [Reactive] public MidiMappingViewModel Previous { get; set; }
        [Reactive] public MidiMappingViewModel Seek { get; set; }
        [Reactive] public MidiMappingViewModel FastForward { get; set; }
        [Reactive] public MidiMappingViewModel NudgeForward { get; set; }
        [Reactive] public MidiMappingViewModel NudgeBackward { get; set; }
        [Reactive] public MidiMappingViewModel CurrentSpeed { get; set; }
        [Reactive] public MidiMappingViewModel CurrentSpeedFine { get; set; }
        [Reactive] public MidiMappingViewModel SetSpeedPlus50 { get; set; }
        [Reactive] public MidiMappingViewModel SetSpeedMinus50 { get; set; }
        [Reactive] public MidiMappingViewModel HomeSpeed { get; set; }
        [Reactive] public MidiMappingViewModel Voice1Toggle { get; set; }
        [Reactive] public MidiMappingViewModel Voice2Toggle { get; set; }
        [Reactive] public MidiMappingViewModel Voice3Toggle { get; set; }
        [Reactive] public MidiMappingViewModel Voice1Kill { get; set; }
        [Reactive] public MidiMappingViewModel Voice2Kill { get; set; }
        [Reactive] public MidiMappingViewModel Voice3Kill { get; set; }
        [Reactive] public MidiMappingViewModel Mode { get; set; }
        [Reactive] public bool SnapToSpeed { get; set; } = false;
        [Reactive] public bool SnapToSeek { get; set; } = false;

        public MidiSettingsViewModel(MidiSettingsViewModel s, List<MidiDeviceViewModel> devices)
        {
            PlayPause = new MidiMappingViewModel(s.PlayPause);
            Stop = new MidiMappingViewModel(s.Stop);
            Next = new MidiMappingViewModel(s.Next);
            Previous = new MidiMappingViewModel(s.Previous);
            Seek = new MidiMappingViewModel(s.Seek);
            FastForward = new MidiMappingViewModel(s.FastForward);
            NudgeForward = new MidiMappingViewModel(s.NudgeForward);
            NudgeBackward = new MidiMappingViewModel(s.NudgeBackward);
            CurrentSpeed = new MidiMappingViewModel(s.CurrentSpeed);
            CurrentSpeedFine = new MidiMappingViewModel(s.CurrentSpeedFine);
            SetSpeedPlus50 = new MidiMappingViewModel(s.SetSpeedPlus50);
            SetSpeedMinus50 = new MidiMappingViewModel(s.SetSpeedMinus50);
            HomeSpeed = new MidiMappingViewModel(s.HomeSpeed);
            Voice1Toggle = new MidiMappingViewModel(s.Voice1Toggle);
            Voice2Toggle = new MidiMappingViewModel(s.Voice2Toggle);
            Voice3Toggle = new MidiMappingViewModel(s.Voice3Toggle);
            Voice1Kill = new MidiMappingViewModel(s.Voice1Kill);
            Voice2Kill = new MidiMappingViewModel(s.Voice2Kill);
            Voice3Kill = new MidiMappingViewModel(s.Voice3Kill);
            Mode = new MidiMappingViewModel(s.Mode);
            SnapToSpeed = s.SnapToSpeed;
            SnapToSeek = s.SnapToSeek;

            CurrentSpeed.Device = devices.FirstOrDefault(d => d.Name == CurrentSpeed.Device?.UnboundName) ?? CurrentSpeed.Device;
            CurrentSpeedFine.Device = devices.FirstOrDefault(d => d.Name == CurrentSpeedFine.Device?.UnboundName) ?? CurrentSpeedFine.Device;
            FastForward.Device = devices.FirstOrDefault(d => d.Name == FastForward.Device?.UnboundName) ?? FastForward.Device;
            HomeSpeed.Device = devices.FirstOrDefault(d => d.Name == HomeSpeed.Device?.UnboundName) ?? HomeSpeed.Device;
            Next.Device = devices.FirstOrDefault(d => d.Name == Next.Device?.UnboundName) ?? Next.Device;
            NudgeBackward.Device = devices.FirstOrDefault(d => d.Name == NudgeBackward.Device?.UnboundName) ?? NudgeBackward.Device;
            NudgeForward.Device = devices.FirstOrDefault(d => d.Name == NudgeForward.Device?.UnboundName) ?? NudgeForward.Device;
            PlayPause.Device = devices.FirstOrDefault(d => d.Name == PlayPause.Device?.UnboundName) ?? PlayPause.Device;
            Previous.Device = devices.FirstOrDefault(d => d.Name == Previous.Device?.UnboundName) ?? Previous.Device;
            Seek.Device = devices.FirstOrDefault(d => d.Name == Seek.Device?.UnboundName) ?? Seek.Device;
            SetSpeedMinus50.Device = devices.FirstOrDefault(d => d.Name == SetSpeedMinus50.Device?.UnboundName) ?? SetSpeedMinus50.Device;
            SetSpeedPlus50.Device = devices.FirstOrDefault(d => d.Name == SetSpeedPlus50.Device?.UnboundName) ?? SetSpeedPlus50.Device;
            Stop.Device = devices.FirstOrDefault(d => d.Name == Stop.Device?.UnboundName) ?? Stop.Device;
            Voice1Toggle.Device = devices.FirstOrDefault(d => d.Name == Voice1Toggle.Device?.UnboundName) ?? Voice1Toggle.Device;
            Voice2Toggle.Device = devices.FirstOrDefault(d => d.Name == Voice2Toggle.Device?.UnboundName) ?? Voice2Toggle.Device;
            Voice3Toggle.Device = devices.FirstOrDefault(d => d.Name == Voice3Toggle.Device?.UnboundName) ?? Voice3Toggle.Device;
            Voice1Kill.Device = devices.FirstOrDefault(d => d.Name == Voice1Kill.Device?.UnboundName) ?? Voice1Kill.Device;
            Voice2Kill.Device = devices.FirstOrDefault(d => d.Name == Voice2Kill.Device?.UnboundName) ?? Voice2Kill.Device;
            Voice3Kill.Device = devices.FirstOrDefault(d => d.Name == Voice3Kill.Device?.UnboundName) ?? Voice3Kill.Device;
            Mode.Device = devices.FirstOrDefault(d => d.Name == Mode.Device?.UnboundName) ?? Mode.Device;
        }
        public MidiSettingsViewModel(MidiSettings s)
        {
            PlayPause = new MidiMappingViewModel(s.PlayPause);
            Stop = new MidiMappingViewModel(s.Stop);
            Next = new MidiMappingViewModel(s.Next);
            Previous = new MidiMappingViewModel(s.Previous);
            Seek = new MidiMappingViewModel(s.Seek);
            FastForward = new MidiMappingViewModel(s.FastForward);
            NudgeForward = new MidiMappingViewModel(s.NudgeForward);
            NudgeBackward = new MidiMappingViewModel(s.NudgeBackward);
            CurrentSpeed = new MidiMappingViewModel(s.CurrentSpeed);
            CurrentSpeedFine = new MidiMappingViewModel(s.CurrentSpeedFine);
            SetSpeedPlus50 = new MidiMappingViewModel(s.SetSpeedPlus50);
            SetSpeedMinus50 = new MidiMappingViewModel(s.SetSpeedMinus50);
            HomeSpeed = new MidiMappingViewModel(s.HomeSpeed);
            Voice1Toggle = new MidiMappingViewModel(s.Voice1Toggle);
            Voice2Toggle = new MidiMappingViewModel(s.Voice2Toggle);
            Voice3Toggle = new MidiMappingViewModel(s.Voice3Toggle);
            Voice1Kill = new MidiMappingViewModel(s.Voice1Kill);
            Voice2Kill = new MidiMappingViewModel(s.Voice2Kill);
            Voice3Kill = new MidiMappingViewModel(s.Voice3Kill);
            Mode = new MidiMappingViewModel(s.Mode);
            SnapToSpeed = s.SnapToSpeed;
            SnapToSeek = s.SnapToSeek;
        }

        public MidiSettings ToMidiSettings()
        {
            return new MidiSettings
            {
                PlayPause = PlayPause.ToMidiMapping(),
                Stop = Stop.ToMidiMapping(),
                Next = Next.ToMidiMapping(),
                Previous = Previous.ToMidiMapping(),
                Seek = Seek.ToMidiMapping(),
                FastForward = FastForward.ToMidiMapping(),
                NudgeForward = NudgeForward.ToMidiMapping(),
                NudgeBackward = NudgeBackward.ToMidiMapping(),
                CurrentSpeed = CurrentSpeed.ToMidiMapping(),
                CurrentSpeedFine = CurrentSpeedFine.ToMidiMapping(),
                SetSpeedPlus50 = SetSpeedPlus50.ToMidiMapping(),
                SetSpeedMinus50 = SetSpeedMinus50.ToMidiMapping(),
                HomeSpeed = HomeSpeed.ToMidiMapping(),
                Voice1Toggle = Voice1Toggle.ToMidiMapping(),
                Voice2Toggle = Voice2Toggle.ToMidiMapping(),
                Voice3Toggle = Voice3Toggle.ToMidiMapping(),
                Voice1Kill = Voice1Kill.ToMidiMapping(),
                Voice2Kill = Voice2Kill.ToMidiMapping(),
                Voice3Kill = Voice3Kill.ToMidiMapping(),
                Mode = Mode.ToMidiMapping(),
                SnapToSpeed = SnapToSpeed,
                SnapToSeek = SnapToSeek
            };
        }

        public IEnumerable<MidiMappingViewModel> GetAllMappings()
        {
            return new[]
            {
                PlayPause, Stop, Next, Previous, Seek, FastForward, NudgeForward, NudgeBackward,
                CurrentSpeed, CurrentSpeedFine, SetSpeedPlus50, SetSpeedMinus50, HomeSpeed,
                Voice1Toggle, Voice2Toggle, Voice3Toggle, Voice1Kill, Voice2Kill, Voice3Kill, Mode
            };
        }

    }
}