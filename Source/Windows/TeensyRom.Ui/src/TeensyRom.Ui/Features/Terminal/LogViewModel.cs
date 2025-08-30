using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.Collections.Concurrent;

namespace TeensyRom.Ui.Features.Terminal
{
    public class LogViewModel : ReactiveObject
    {
        public ObservableCollection<string> Logs { get; }

        // Reduced log limit to match RichTextBindingBehavior's MaxInitialLogs
        private readonly int _logLimit = 250;
        private int _lineCount = 0;
        private readonly ConcurrentQueue<string> _logQueue = new();
        private readonly DispatcherTimer _updateTimer;
        private DateTime _lastUiUpdate = DateTime.Now;
        
        // Fixed aggressive throttling
        private readonly TimeSpan _updateInterval = TimeSpan.FromMilliseconds(1000);
        
        // Simple batch size management - reduced for better performance
        private const int MaxLogsPerUpdate = 10; // Reduced from 20 to 10
        private const int MaxRemovalsPerUpdate = 25; // Reduced from 50 to 25

        public LogViewModel()
        {
            Logs = new ObservableCollection<string>();
            
            // Use a fixed aggressive timer interval to ensure UI responsiveness
            _updateTimer = new DispatcherTimer
            {
                Interval = _updateInterval
            };
            _updateTimer.Tick += UpdateUI_Tick;
            _updateTimer.Start();
        }

        private void UpdateUI_Tick(object? sender, EventArgs e)
        {
            if (_logQueue.Count > 0 && (DateTime.Now - _lastUiUpdate) > _updateInterval)
            {
                ProcessUiUpdate();
                _lastUiUpdate = DateTime.Now;
            }
        }

        public void AddLog(string log)
        {
            // Simply queue the log - processing happens on timer tick
            _logQueue.Enqueue(log);
        }
        private void ProcessUiUpdate()
        {
            // Dequeue logs up to the maximum per update
            List<string> logsToAdd = new();
            int count = 0;
            while (count < MaxLogsPerUpdate && _logQueue.TryDequeue(out var log))
            {
                logsToAdd.Add(log);
                count++;
            }
            
            if (logsToAdd.Count == 0) return;

            // Calculate line count for the new logs
            int linesToAdd = 0;
            foreach (var log in logsToAdd)
            {
                linesToAdd++;
                linesToAdd += log.Count(c => c == '\n');
                linesToAdd += log.Count(c => c == '\r');
            }
            
            // Process on UI thread with fixed small batches
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                try
                {
                    // Be more aggressive with trimming to prevent large collections
                    if (_lineCount + linesToAdd > _logLimit + 500) // Reduced margin from 1000 to 500
                    {
                        int linesToRemove = (_lineCount + linesToAdd) - _logLimit;
                        int removals = 0;
                        int linesRemoved = 0;
                        
                        // Remove just enough to get back under the limit
                        while (removals < MaxRemovalsPerUpdate && Logs.Count > 0 && linesRemoved < linesToRemove)
                        {
                            var logToRemove = Logs[0];
                            int logLines = 1 + logToRemove.Count(c => c == '\n') + logToRemove.Count(c => c == '\r');
                            
                            _lineCount -= logLines;
                            linesRemoved += logLines;
                            Logs.RemoveAt(0);
                            removals++;
                        }
                    }
                    
                    // Add logs in a simple way
                    foreach (var log in logsToAdd)
                    {
                        Logs.Add(log);
                    }
                    
                    // Update line count
                    _lineCount += linesToAdd;
                    
                    // If queue is still large, add a simplified message
                    if (_logQueue.Count > 50)  // Reduced threshold from 100 to 50
                    {
                        Logs.Add($"[{_logQueue.Count} logs queued...]");
                    }
                }
                catch (Exception ex)
                {
                    // Prevent crashes
                    System.Diagnostics.Debug.WriteLine($"Error updating logs: {ex.Message}");
                }
            });
        }

        public void Clear()
        {
            // Clear the queue
            while (_logQueue.TryDequeue(out _)) { }
            
            // Clear the UI
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Logs.Clear();
                _lineCount = 0;
            });
        }
    }
}
