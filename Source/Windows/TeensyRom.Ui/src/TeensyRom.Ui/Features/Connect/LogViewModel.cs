using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Features.Connect
{
    public class LogViewModel : ReactiveObject
    {
        public ObservableCollection<string> Logs { get; } = [];

        private int _logLimit = 500;
        private int _lineCount = 0;

        public void AddLog(string log)
        {
            if (_lineCount >= _logLimit)
            {
                var logToRemove = Logs[0];
                _lineCount--;
                _lineCount -= logToRemove.Count(c => c == '\n');
                _lineCount -= logToRemove.Count(c => c == '\r');                
                Logs.RemoveAt(0);
            }
            Logs.Add(log);
            _lineCount++;
            _lineCount += log.Count(c => c == '\n');
            _lineCount += log.Count(c => c == '\r');
        }

        public void Clear()
        {
            Logs.Clear();
            _lineCount = 0;
        }
    }
}
