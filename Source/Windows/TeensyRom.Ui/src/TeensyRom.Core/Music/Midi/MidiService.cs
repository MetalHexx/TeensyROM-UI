using NAudio;
using NAudio.Midi;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Navigation;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Music.Midi
{
    public class MidiService : IMidiService
    {
        public IObservable<MidiEvent> MidiEvents => _midiEvents.AsObservable();
        private readonly Subject<MidiEvent> _midiEvents = new();
        private readonly ISettingsService _settingsService;
        private readonly IAlertService _alert;
        private readonly ILoggingService _log;
        private List<MidiMapping> _registeredMappings = [];
        private List<MidiDevice> _registeredDevices = [];
        private readonly List<IDisposable> _midiSubscriptions = [];
        private readonly List<MidiIn> _midiIns = [];
        private readonly List<MidiDevice> _midiDevices = [];

        public MidiService(ISettingsService settingsService, IAlertService alertService, ILoggingService log)
        {
            _settingsService = settingsService;
            _alert = alertService;
            _log = log;

            _settingsService.Settings                
                .Where(s => s.LastCart is not null && s.LastCart.MidiSettings is not null)
                .Select(s => s.LastCart!.MidiSettings)
                .Subscribe(EngageMidi);
        }

        public void EngageMidi(MidiSettings midiSettings)
        { 
            DisengageMidi();

            if (midiSettings.MidiEnabled is false) return;

            var devices = GetMidiDevices().ToDictionary(d => d.Name);

            var mappings = midiSettings.Mappings;

            _registeredMappings = mappings
                .Where(m => devices.Any(d => d.Value.Name == m.Device.Name || m.Device.ProductName == d.Value.ProductName))
                .Select(m =>
                {
                    var device = devices[m.Device.Name];

                    if(device is null)
                    {
                        device = devices[m.Device.ProductName];
                    }
                    m.Device.Id = device.Id;
                    return m;
                })
                .ToList();

            _registeredDevices = _registeredMappings
                .DistinctBy(m => m.Device.Id)
                .Select(d => d.Device)
                .Cast<MidiDevice>()
                .ToList();

            var missingDevices = mappings
                .Where(m => !devices.ContainsKey(m.Device.Name))
                .Where(m => !string.IsNullOrWhiteSpace(m.Device.Name))
                .Distinct()
                .ToList();

            if (missingDevices.Count > 0)
            {
                _alert.Publish($"Some configured MIDI devices were not found.");
                _alert.Publish($"Check Settings: Missing MIDI devices will show up blank.");

                foreach (var d in missingDevices)
                {
                    _log.InternalError($"MIDI device '{d.Device.Name}' not found.");
                }
            }
            if (_registeredMappings.Count == 0)
            {
                return;
            }

            foreach (var device in _registeredDevices)
            {
                var midiIn = TryGetMidiIn(device.Id);

                if (midiIn is null) continue;

                _midiIns.Add(midiIn);

                var midiObservable = Observable.FromEventPattern<MidiInMessageEventArgs>(
                    h => midiIn.MessageReceived += h,
                    h => midiIn.MessageReceived -= h);

                var deviceMappings = _registeredMappings
                    .Where(m => m.Device.Id == device.Id);

                var ccMappings = deviceMappings
                    .Where(m => m is CCMapping);

                var ccSubscription = midiObservable
                    .Select(e => e.EventArgs.MidiEvent)
                    .OfType<ControlChangeEvent>()
                    .Select(cc =>
                    (
                            CCEvent: cc,
                            Mapping: ccMappings.FirstOrDefault(m => m.MidiChannel == cc.Channel 
                            && m.NoteOrCC == (int)cc.Controller 
                            && (m.FilterValue is null || m.FilterValue == cc.ControllerValue))
                    ))
                    .Where(ccMapping => ccMapping.Mapping is not null)
                    .Subscribe(ccMapping =>
                    {
                        Debug.WriteLine($"[MIDI IN --- Device: {ccMapping.Mapping!.Device.Name}Channel: {ccMapping.CCEvent.Channel} CC: {ccMapping.CCEvent.Controller} Value: {ccMapping.CCEvent.ControllerValue}");

                        _midiEvents.OnNext(new MidiEvent
                        (
                            ccMapping.Mapping!.DJEventType,
                            MidiEventType.ControlChange,
                            ccMapping.Mapping,
                            ccMapping.CCEvent.ControllerValue
                        ));
                    });

                _midiSubscriptions.Add(ccSubscription);

                var noteMappings = deviceMappings
                    .Where(m => m is NoteMapping and not DualNoteMapping)
                    .Cast<NoteMapping>();

                var dualNoteMappings = deviceMappings
                    .OfType<DualNoteMapping>();

                var noteMidiEvents = midiObservable
                   .Select(e => e.EventArgs.MidiEvent)
                   .Where(e => e.CommandCode is MidiCommandCode.NoteOn or MidiCommandCode.NoteOff)
                   .Cast<NoteEvent>();

                var noteMappingEvents = noteMidiEvents
                   .Select(noteEvent => 
                   (
                        NoteEvent: noteEvent,
                        Mapping: MatchMapping(noteEvent, noteMappings)
                   ));

                var dualNoteMappingEvents = noteMidiEvents
                   .Select(noteEvent => 
                   (
                        NoteEvent: noteEvent,
                        Mapping: MatchMapping(noteEvent, dualNoteMappings)
                   ));

                var combinedNoteMappings = noteMappingEvents
                    .Merge(dualNoteMappingEvents)
                    .Where(eventMapping => eventMapping.Mapping is not null);

                var noteSubscription = combinedNoteMappings
                    .Subscribe(eventMapping =>
                    {
                        Debug.WriteLine($"[MIDI IN --- Device: {eventMapping.Mapping!.Device.Name}Channel: {eventMapping.NoteEvent.Channel} Note: {eventMapping.NoteEvent.NoteNumber} {eventMapping.NoteEvent.CommandCode}");

                        _midiEvents.OnNext(new MidiEvent
                        (
                            eventMapping.Mapping!.DJEventType,
                            eventMapping.NoteEvent.CommandCode == MidiCommandCode.NoteOn
                            ? MidiEventType.NoteOn
                            : MidiEventType.NoteOff,
                            eventMapping.Mapping,
                            eventMapping.NoteEvent.NoteNumber
                        ));
                    });

                _midiSubscriptions.Add(noteSubscription);

                midiIn.Start();
            }
        }

        private static NoteMapping? MatchMapping(NoteEvent noteEvent, IEnumerable<NoteMapping> noteMappings)
        {
            return noteMappings.FirstOrDefault
             (
                 m => m.MidiChannel == noteEvent.Channel && m.NoteOrCC == noteEvent.NoteNumber
                 &&
                 m.NoteEvent.EqualsMidiCommand(noteEvent.CommandCode)
                 &&
                 (m.FilterValue is null || m.FilterValue == noteEvent.Velocity)
             );
        }

        private static NoteMapping? MatchMapping(NoteEvent noteEvent, IEnumerable<DualNoteMapping> dualNoteMappings)
        {
            return dualNoteMappings
                .Cast<DualNoteMapping>()
                .FirstOrDefault
                 (
                     m => m.MidiChannel == noteEvent.Channel && m.NoteOrCC == noteEvent.NoteNumber
                     &&
                     (
                         m.NoteEvent.EqualsMidiCommand(noteEvent.CommandCode)
                         ||
                         m.NoteEvent2.EqualsMidiCommand(noteEvent.CommandCode)
                     )
                     &&
                     (m.FilterValue is null || m.FilterValue == noteEvent.Velocity)
                 );
        }

        public void DisengageMidi()
        {
            _midiIns.ForEach(DisposeMidiIn);
            _midiSubscriptions.ForEach(DisposeMidiIn);
            _midiSubscriptions.Clear();
            _registeredMappings.Clear();
            _registeredDevices.Clear();            
        }

        public async Task<MidiResult?> GetFirstMidiEvent(MidiEventType targetEventType)
        {
            var settings = _settingsService.GetSettings();

            if (settings.LastCart is null) return null;

            DisengageMidi();

            var devices = GetMidiDevices();

            if (!devices.Any())
            {
                _alert.Publish("No MIDI devices found.");
                throw new InvalidOperationException("No MIDI devices found.");
            }

            var midiInList = new List<MidiIn>();
            var errorThrottle = new Subject<string>();

            try
            {
                errorThrottle
                    .Throttle(TimeSpan.FromMilliseconds(500))
                    .Subscribe(_alert.Publish);

                var midiEventObservable = devices
                    .Select(d =>
                    {
                        var midiIn = TryGetMidiIn(d.Id);

                        if (midiIn is null) return null;

                        return new { Device = d, MidiIn = midiIn };
                    })
                    .Where(deviceAndMidiIn => deviceAndMidiIn is not null)
                    .Select(deviceAndMidiIn =>
                    {
                        midiInList.Add(deviceAndMidiIn!.MidiIn);

                        var observable = Observable.FromEventPattern<MidiInMessageEventArgs>(
                                h => deviceAndMidiIn.MidiIn.MessageReceived += h,
                                h => deviceAndMidiIn.MidiIn.MessageReceived -= h)
                            .Select(e => new { Event = e.EventArgs.MidiEvent, Device = deviceAndMidiIn.Device });

                        deviceAndMidiIn.MidiIn.Start();
                        return observable;
                    })
                    .Merge();

                return await midiEventObservable
                    .Select(result => new
                    {
                        Result = result,
                        IsValid = targetEventType switch
                        {
                            MidiEventType.ControlChange => result.Event is ControlChangeEvent,
                            MidiEventType.NoteOn or MidiEventType.NoteOff or MidiEventType.NoteChange => result.Event is NoteOnEvent,
                            _ => false
                        }
                    })
                    .Do(x =>
                    {
                        if (!x.IsValid)
                        {
                            var expectedType = targetEventType == MidiEventType.ControlChange ? "a knob or slider (CC)" : "a key or pad (Note)";
                            errorThrottle.OnNext($"Please use {expectedType} to bind this control.");
                        }
                    })
                    .Where(x => x.IsValid)
                    .Select(x => x.Result)
                    .Take(1)
                    .Select(result => result.Event switch
                    {
                        ControlChangeEvent ccEvent => new MidiResult(result.Device, (int)ccEvent.Controller, ccEvent.Channel, ccEvent.ControllerValue),
                        NoteOnEvent noteOn => new MidiResult(result.Device, noteOn.NoteNumber, noteOn.Channel, noteOn.Velocity),
                        _ => throw new InvalidOperationException("Unhandled MIDI event type.")
                    });
            }
            finally
            {
                foreach (var midiIn in midiInList)
                {
                    midiIn.Stop();
                    midiIn.Dispose();
                }
                if (settings.LastCart.MidiSettings is not null) 
                {
                    EngageMidi(settings.LastCart.MidiSettings);
                }
            }            
        }

        public MidiIn? TryGetMidiIn(int deviceId) 
        {
            try
            {
                return new MidiIn(deviceId);
            }
            catch (MmException) { }
            
            return null;
        }

        public IEnumerable<MidiDevice> RefreshMidiDevices() 
        {
            _midiDevices.Clear();
            return GetMidiDevices();
        }

        public IEnumerable<MidiDevice> GetMidiDevices()
        {
            if (_midiDevices.Count != 0) return _midiDevices;

            var devices = new List<MidiDevice>();
            var nameCounts = new Dictionary<string, int>();

            for (int deviceId = 0; deviceId < MidiIn.NumberOfDevices; deviceId++)
            {
                var midiIn = TryGetMidiIn(deviceId);

                if (midiIn is null) continue;

                var capabilities = MidiIn.DeviceInfo(deviceId);
                string displayName = capabilities.ProductName;

                if (nameCounts.ContainsKey(displayName))
                {
                    nameCounts[displayName]++;
                    displayName = $"{displayName} ({nameCounts[displayName]})";
                }
                else
                {
                    nameCounts[displayName] = 1;
                }

                devices.Add(new MidiDevice
                {
                    Name = displayName,
                    Id = deviceId,
                    ManufacturerName = capabilities.Manufacturer.ToString(),
                    ProductId = capabilities.ProductId,
                    ProductName = capabilities.ProductName
                });
                DisposeMidiIn(midiIn);
            }
            _midiDevices.AddRange(devices.OrderBy(d => d.Name));

            return _midiDevices;
        }

        public void DisposeMidiIn(IDisposable midiIn) 
        {
            try
            {
                midiIn?.Dispose();
            }
            catch (Exception ex)
            {
                //TODO: not sure why this happens
            }
        }

        public void SendRandomMidiNotesToAllDevices()
        {
            var devices = GetMidiDevices();

            foreach (var device in devices)
            {
                try
                {
                    using (var midiOut = new MidiOut(device.Id))
                    {
                        var random = new Random();
                        for (int channel = 1; channel <= 3; channel++)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                int note = random.Next(60, 72);
                                midiOut.Send(MidiMessage.StartNote(note, 127, channel).RawData);
                                System.Threading.Thread.Sleep(100);
                                midiOut.Send(MidiMessage.StopNote(note, 0, channel).RawData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending MIDI to device {device.Name}: {ex.Message}");
                }
            }
        }

        public void SendRandomMidiNotes(int deviceId, int durationMs = 200, int count = 50)
        {
            using var midiOut = new MidiOut(deviceId);
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                int note = random.Next(60, 72); // C4 to B4
                midiOut.Send(MidiMessage.StartNote(note, 127, 1).RawData);
                Thread.Sleep(durationMs);
                midiOut.Send(MidiMessage.StopNote(note, 0, 1).RawData);
            }
        }

        public void ReceiveMidiInputLoop()
        {
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                try
                {
                    var midiIn = TryGetMidiIn(i);

                    if (midiIn is null) continue;

                    string deviceName = "Unknown Device";
                    try { deviceName = MidiIn.DeviceInfo(i).ProductName; } catch { }

                    midiIn.MessageReceived += (sender, e) =>
                    {
                        Debug.WriteLine($"[MIDI IN {deviceName}] {e.MidiEvent}");
                    };
                    midiIn.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error receiving MIDI from device index {i}: {ex.Message}");
                }
            }

            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
    }

    public static class NoteEventTypeExtensions
    {
        public static bool EqualsMidiCommand(this NoteEventType noteEventType, MidiCommandCode commandCode)
        {
            return (noteEventType, commandCode) switch
            {
                (NoteEventType.NoteOn, MidiCommandCode.NoteOn) => true,
                (NoteEventType.NoteOff, MidiCommandCode.NoteOff) => true,
                // Add more mappings if needed
                _ => false
            };
        }
    }
    public record MidiResult(MidiDevice Device, int CCOrNote, int Channel, int Value);

}