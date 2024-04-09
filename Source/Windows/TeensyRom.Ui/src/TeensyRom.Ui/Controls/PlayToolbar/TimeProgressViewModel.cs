using System;

namespace TeensyRom.Ui.Controls.PlayToolbar
{
    public class TimeProgressViewModel(TimeSpan totalTime, TimeSpan currentTime)
    {
        public string TotalTime { get; set; } = totalTime.ToString(@"m\:ss");
        public string CurrentTime { get; set; } = currentTime.ToString(@"m\:ss");
        public double Percentage { get; set; } = (double)currentTime.TotalSeconds / (double)totalTime.TotalSeconds;
    }
}