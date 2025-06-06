﻿using NAudio;
using NAudio.Midi;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Settings;

namespace TeensyRom.Core.Music.Midi
{
    public class MidiService : IMidiService, IDisposable
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
        private MidiSettings? _midiSettings;
        private bool _midiEngaged = false;

        public MidiService(ISettingsService settingsService, IAlertService alertService, ILoggingService log)
        {
            _settingsService = settingsService;
            _alert = alertService;
            _log = log;

            _settingsService.Settings                
                .Where(s => s.LastCart is not null && s.LastCart.MidiSettings is not null)
                .Where(s => MidiSettingsChanged(s.LastCart!.MidiSettings))
                .Select(s => s.LastCart!.MidiSettings)
                .Subscribe(EngageMidi);
        }

        public bool IsMidiEngaged => _midiEngaged;

        public void EngageMidi() 
        {
            if (_midiSettings is null) 
            {
                var midiSettings =  _settingsService.GetSettings().LastCart?.MidiSettings;

                if (midiSettings is null) 
                {
                    _alert.Publish("Engaging Midi Failed.  Midi Settings were not found.");
                    _log.InternalError("Engaging Midi Failed.  Midi Settings were not found.");
                    return;
                }
                _midiSettings = midiSettings;
            }
            EngageMidi(_midiSettings);
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
                        //Debug.WriteLine($"[MIDI IN --- Device: {eventMapping.Mapping!.Device.Name}Channel: {eventMapping.NoteEvent.Channel} Note: {eventMapping.NoteEvent.NoteNumber} {eventMapping.NoteEvent.CommandCode}");

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
            _midiEngaged = true;
        }

        private static NoteMapping? MatchMapping(NoteEvent noteEvent, IEnumerable<NoteMapping> noteMappings)
        {
            return noteMappings.FirstOrDefault
             (
                 m => m.MidiChannel == noteEvent.Channel && m.NoteOrCC == noteEvent.NoteNumber
                 &&
                 m.NoteEvent.EqualsMidiCommand(noteEvent.CommandCode)
                 &&
                 (m.RequiredVelocity is null || m.RequiredVelocity == noteEvent.Velocity)
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
                     (m.RequiredVelocity is null || m.RequiredVelocity == noteEvent.Velocity)
                 );
        }

        public void DisengageMidi()
        {
            if (!_midiEngaged) return;

            _midiIns.ForEach(DisposeMidiIn);
            _midiIns.Clear();
            _midiSubscriptions.ForEach(s => s?.Dispose());
            _midiSubscriptions.Clear();
            _registeredMappings.Clear();
            _registeredDevices.Clear();            
            _midiEngaged = false;
        }

        public async Task<MidiResult?> GetFirstMidiEvent(MidiEventType targetEventType)
        {
            var settings = _settingsService.GetSettings();
            if (settings.LastCart is null) return null;

            var shouldEngage = _midiEngaged;

            DisengageMidi();

            var devices = GetMidiDevices();

            if (!devices.Any())
            {
                _alert.Publish("No MIDI devices found.");
                throw new InvalidOperationException("No MIDI devices found.");
            }

            var midiInList = new List<MidiIn>();
            var tcs = new TaskCompletionSource<MidiResult>();
            var errorThrottleTimer = new System.Timers.Timer(500) { AutoReset = false };
            bool errorDisplayed = false;

            void ThrottleError(string message)
            {
                if (!errorDisplayed)
                {
                    errorDisplayed = true;
                    _alert.Publish(message);
                    errorThrottleTimer.Start();
                }
            }

            errorThrottleTimer.Elapsed += (_, _) => errorDisplayed = false;

            try
            {
                foreach (var device in devices)
                {
                    var midiIn = TryGetMidiIn(device.Id);
                    if (midiIn is null) continue;

                    midiIn.MessageReceived += (sender, args) =>
                    {
                        var midiEvent = args.MidiEvent;

                        bool isValid = targetEventType switch
                        {
                            MidiEventType.ControlChange => midiEvent is ControlChangeEvent,
                            MidiEventType.NoteOn or MidiEventType.NoteOff or MidiEventType.NoteChange => midiEvent is NoteOnEvent,
                            _ => false
                        };

                        if (!isValid)
                        {
                            var expected = targetEventType == MidiEventType.ControlChange ? "a knob or slider (CC)" : "a key or pad (Note)";
                            ThrottleError($"Please use {expected} to bind this control.");
                            return;
                        }

                        MidiResult result = midiEvent switch
                        {
                            ControlChangeEvent cc => new MidiResult(device, (int)cc.Controller, cc.Channel, cc.ControllerValue),
                            NoteOnEvent note => new MidiResult(device, note.NoteNumber, note.Channel, note.Velocity),
                            _ => throw new InvalidOperationException("Unhandled MIDI event type.")
                        };
                        tcs.TrySetResult(result);
                    };

                    midiInList.Add(midiIn);
                    midiIn.Start();
                }

                // Cancel after 30 seconds if nothing comes in
                var timeout = Task.Delay(TimeSpan.FromSeconds(30));
                var completedTask = await Task.WhenAny(tcs.Task, timeout);

                return completedTask == tcs.Task ? tcs.Task.Result : null;
            }
            finally
            {
                errorThrottleTimer.Dispose();

                midiInList.ForEach(DisposeMidiIn);

                if (shouldEngage == true)
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

        public IEnumerable<MidiDevice> GetMidiDevices()
        {
            var devices = new List<MidiDevice>();
            var nameCounts = new Dictionary<string, int>();

            for (int deviceId = 0; deviceId < MidiIn.NumberOfDevices; deviceId++)
            {
                var midiIn = TryGetMidiIn(deviceId);

                if (midiIn is null) continue;

                DisposeMidiIn(midiIn);

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
            }

            return devices.OrderBy(d => d.Name);
        }

        public void DisposeMidiIn(MidiIn midiIn) 
        {
            try
            {
                midiIn?.Stop();
                midiIn?.Dispose();
                midiIn = null!;
            }
            catch (AccessViolationException)
            {
                //TODO: Not sure why MMAudio crashes here.
            }
        }

        private bool MidiSettingsChanged(MidiSettings midiSettings)
        {
            if (_midiSettings is not null)
            {
                var previousSettings = JsonSerializer.Serialize(_midiSettings);
                var newSettings = JsonSerializer.Serialize(midiSettings);

                if (previousSettings == newSettings) return false;
            }
            _midiSettings = midiSettings;
            return true;
        }

        public void Dispose()
        {
            DisengageMidi();
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