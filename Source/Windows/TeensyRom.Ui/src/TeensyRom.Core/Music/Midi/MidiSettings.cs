namespace TeensyRom.Core.Music.Midi
{
    public class MidiSettings
    {
        public bool MidiEnabled { get; set; }

        public List<MidiMapping> Mappings { get; set; } = [];

        public void InitMappings()
        {
            List<MidiMapping> possibleMappings =
            [
                new NoteMapping() { DJEventType = DJEventType.IncreaseCurrentSpeed, DisplayName = "Increase Speed %", AmountEnabled = true, Amount = 1.0 },
                new NoteMapping() { DJEventType = DJEventType.DecreaseCurrentSpeed, DisplayName = "Decrease Speed %", AmountEnabled = true, Amount = -1.0 },
                new NoteMapping() { DJEventType = DJEventType.IncreaseCurrentSpeedFine, DisplayName = "Increase Speed Fine %", AmountEnabled = true, Amount = 0.01 },
                new NoteMapping() { DJEventType = DJEventType.DecreaseCurrentSpeedFine, DisplayName = "Decrease Speed Fine %", AmountEnabled = true, Amount = -0.01 },
                new CCMapping()   { DJEventType = DJEventType.CurrentSpeed, DisplayName = "Inc/Dec Speed %", CCType = CCType.Relative1, AmountEnabled = true, Amount = 0.1 },
                new CCMapping()   { DJEventType = DJEventType.CurrentSpeedFine, DisplayName = "Inc/Dec Speed Fine %", CCType = CCType.Relative1, AmountEnabled = true, Amount = 0.01 },
                new NoteMapping() { DJEventType = DJEventType.SpeedPlus50Toggle, DisplayName = "Toggle Speed +50%" },
                new NoteMapping() { DJEventType = DJEventType.HomeSpeedToggle, DisplayName = "Toggle Home Speed" },
                new NoteMapping() { DJEventType = DJEventType.SpeedMinus50Toggle, DisplayName = "Toggle Speed -50%" },                
                new DualNoteMapping() { DJEventType = DJEventType.NudgeForward, DisplayName = "Nudge Forward %", AmountEnabled = true, Amount = 5.0 },
                new DualNoteMapping() { DJEventType = DJEventType.NudgeBackward, DisplayName = "Nudge Backward %", AmountEnabled = true, Amount = -5.0 },
                new NoteMapping() { DJEventType = DJEventType.Voice1Toggle, DisplayName = "Toggle Voice 1" },
                new NoteMapping() { DJEventType = DJEventType.Voice2Toggle, DisplayName = "Toggle Voice 2" },
                new NoteMapping() { DJEventType = DJEventType.Voice3Toggle, DisplayName = "Toggle Voice 3" },
                new DualNoteMapping() { DJEventType = DJEventType.Voice1Kill, DisplayName = "Kill Voice 1" },
                new DualNoteMapping() { DJEventType = DJEventType.Voice2Kill, DisplayName = "Kill Voice 2" },
                new DualNoteMapping() { DJEventType = DJEventType.Voice3Kill, DisplayName = "Kill Voice 3" },
                new CCMapping()   { DJEventType = DJEventType.Seek, DisplayName = "Seek / Scrub", CCType = CCType.Relative1, AmountEnabled = true, Amount = 0.005 },
                new NoteMapping() { DJEventType = DJEventType.Mode, DisplayName = "Toggle Shuffle Mode" },
                new NoteMapping() { DJEventType = DJEventType.PlayPause, DisplayName = "Play / Pause" },
                new NoteMapping() { DJEventType = DJEventType.FastForward, DisplayName = "Fast Forward" },
                new NoteMapping() { DJEventType = DJEventType.Previous, DisplayName = "Previous" },
                new NoteMapping() { DJEventType = DJEventType.Next, DisplayName = "Next" },
                new NoteMapping() { DJEventType = DJEventType.Restart, DisplayName = "Restart Song" },
                new NoteMapping() { DJEventType = DJEventType.SeekForward, DisplayName = "Seek Forward", AmountEnabled = true, Amount = 0.005  },
                new NoteMapping() { DJEventType = DJEventType.SeekBackward, DisplayName = "Seek Backward", AmountEnabled = true, Amount = -0.005  }
            ];

            foreach (MidiMapping mapping in possibleMappings)
            {
                MidiMapping? existingMapping = Mappings.FirstOrDefault(m => m.DJEventType == mapping.DJEventType);

                if (existingMapping is null)
                {
                    Mappings.Add(mapping);
                }
            }
        }
    }
}