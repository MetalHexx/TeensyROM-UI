using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;
using TeensyRom.Api.Endpoints.Serial.GetLogs;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public class LoggingService : IQueuedChannelLogger
    {
        private readonly ConcurrentQueue<string> _channelQueue = new();
        private readonly ConcurrentQueue<string> _fileQueue = new();
        private CancellationTokenSource? _fileLoggingCancellationSource;
        private CancellationTokenSource? _clientLoggingCancellationSource;
        private Task? _logStreamTask;
        private readonly string _logPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.LogPath);
        private readonly string _logFilename = $"{LogConstants.LogFileName}{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{LogConstants.LogFileExtention}";
        private readonly ILogStream _logStream;
        private const int _batchSize = 20;

        public void External(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External: {message}" : message);
        public void ExternalError(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External Error: {message}" : message);
        public void ExternalSuccess(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External Success: {message}" : message);
        public void Internal(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal: {message}" : message);
        public void InternalError(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Error: {message}" : message);
        public void InternalWarning(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Warning: {message}" : message);
        public void InternalSuccess(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Success: {message}" : message);
        

        public LoggingService(ILogStream logStream)
        {
            StartFileLogging();
            _logStream = logStream;
        }
        private void StartFileLogging()
        {
            if (_fileLoggingCancellationSource is not null) return; 

            _logPath.EnsureLocalPath();

            _fileLoggingCancellationSource = new CancellationTokenSource();

            Task.Factory.StartNew(ProcessFileLogs,
                _fileLoggingCancellationSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void StartLogStream()
        {
            _channelQueue.Clear();
            _clientLoggingCancellationSource ??= new CancellationTokenSource();
            _logStreamTask = StartLogStreaming(_clientLoggingCancellationSource.Token);

            _logStreamTask.ContinueWith(task =>
            {
                if (task.IsFaulted && task.Exception is not null)
                {
                    foreach (var ex in task.Exception.Flatten().InnerExceptions)
                    {
                        WriteLog($"Log streaming faulted: {ex}");
                    }
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public void StopLogStream()
        {
            _clientLoggingCancellationSource?.Cancel();
            _clientLoggingCancellationSource = null;
            _channelQueue.Clear();
        }

        private void ProcessFileLogs()
        {
            if (_fileLoggingCancellationSource is null)
            {
                Debug.WriteLine("File Logging Cancellation Source is not initialized.");
                return;
            }

            const int batchSize = 100;
            const int maxIntervalMs = 5000;
            
            var logBatch = new List<string>(batchSize);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            try
            {
                while (!_fileLoggingCancellationSource.Token.IsCancellationRequested)
                {
                    bool dequeued = false;
                    
                    while (logBatch.Count < batchSize && _fileQueue.TryDequeue(out var message))
                    {
                        logBatch.Add(message);
                        dequeued = true;
                    }
                    
                    bool reachedBatchSize = logBatch.Count >= batchSize;
                    bool reachedTimeThreshold = stopwatch.ElapsedMilliseconds >= maxIntervalMs && logBatch.Count > 0;
                    
                    if (reachedBatchSize || reachedTimeThreshold)
                    {
                        WriteLogsToFile(logBatch);
                        logBatch.Clear();
                        stopwatch.Restart();
                    }
                    
                    if (!dequeued)
                    {
                        Task.Delay(10, _fileLoggingCancellationSource.Token)
                            .Wait(_fileLoggingCancellationSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                if (logBatch.Count > 0)
                {
                    WriteLogsToFile(logBatch);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in file processing: {ex.Message}");
            }
        }

        private void WriteLogsToFile(List<string> logs)
        {
            var logFilePath = Path.Combine(_logPath, _logFilename);

            try
            {
                Directory.CreateDirectory(_logPath);                
                File.AppendAllLines(logFilePath, logs);                
                Debug.WriteLine($"Wrote {logs.Count} logs to file: {logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to log file {logFilePath}: {ex.Message}");
            }
        }               

        private void WriteLog(string message)
        {
            Debug.WriteLine(message);
                
            if(_clientLoggingCancellationSource is not null)
            {
                _channelQueue.Enqueue(message);
            }
            _fileQueue.Enqueue(message);
        }

        /// <summary>
        /// Send out the logs through the log stream.  If the operation is cancelled, it will 
        /// drain stop the logs.
        /// </summary>
        public Task StartLogStreaming(CancellationToken ct)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (!ct.IsCancellationRequested || !_channelQueue.IsEmpty)
                {
                    var logBatch = new List<string>();
                    var currentCount = 0;

                    while (currentCount < _batchSize && _channelQueue.TryDequeue(out var log))
                    {
                        if (string.IsNullOrWhiteSpace(log)) continue;

                        try
                        {
                            await _logStream.Push(log, ct);
                        }
                        catch (OperationCanceledException)
                        {
                            //Expected when the cancellation token is triggered
                        }
                        catch (Exception ex)
                        {
                            WriteLog(ex.ToString());
                            break;
                        }
                        currentCount++;
                    }
                    if (ct.IsCancellationRequested && _channelQueue.IsEmpty) break;

                    try
                    {
                        await Task.Delay(100, ct);
                    }
                    catch (TaskCanceledException)
                    {
                        // Expected when the cancellation token is triggered
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Error in log streaming: {ex}");
                        break;
                    }
                }

            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
        }

        public IObservable<string> Logs => throw new NotImplementedException();
    }
}