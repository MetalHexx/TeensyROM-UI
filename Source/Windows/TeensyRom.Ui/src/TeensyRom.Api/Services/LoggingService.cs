using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services
{
    public class LoggingService : IQueuedChannelLogger
    {
        private readonly Channel<string> _logChannel;
        private readonly ConcurrentQueue<string> _channelQueue = new();
        private readonly ConcurrentQueue<string> _fileQueue = new();
        private Task? _channelProcessingTask;
        private CancellationTokenSource? _logChannelCanncellationSource;
        private CancellationTokenSource? _fileLoggingCancellationSource;
        private readonly string _logPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.LogPath);
        private readonly string _logFilename = $"{LogConstants.LogFileName}{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{LogConstants.LogFileExtention}";

        public ChannelReader<string> LogChannel => _logChannel.Reader;
        public void External(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External: {message}" : message);
        public void ExternalError(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External Error: {message}" : message);
        public void ExternalSuccess(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) External Success: {message}" : message);
        public void Internal(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal: {message}" : message);
        public void InternalError(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Error: {message}" : message);
        public void InternalWarning(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Warning: {message}" : message);
        public void InternalSuccess(string message, string? deviceId = null) => WriteLog(deviceId is not null ? $"({deviceId}) Internal Success: {message}" : message);
        

        public LoggingService()
        {
            _logChannel = Channel.CreateUnbounded<string>();
            //StartChannelLogging();
            StartFileLogging();
        }
        private void StartFileLogging()
        {
            _logPath.EnsureLocalPath();

            _fileLoggingCancellationSource = new CancellationTokenSource();

            Task.Factory.StartNew(ProcessFileLogs,
                _fileLoggingCancellationSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void StartChannelLogging()
        {
            if (_channelProcessingTask != null && !_channelProcessingTask.IsCompleted)
            {
                Debug.WriteLine("Channel queue is already running.");
                return;
            }
            _logChannelCanncellationSource = new CancellationTokenSource();

            _channelProcessingTask = Task.Factory.StartNew(ProcessChannelLogs,
               _logChannelCanncellationSource.Token,
               TaskCreationOptions.LongRunning,
               TaskScheduler.Default);
        }

        public void StopChannelQueue()
        {
            if (_logChannelCanncellationSource is null && _channelProcessingTask is null)
            {
                Debug.WriteLine("Log Channel is not running.");
                return;
            }
            try
            {
                _logChannelCanncellationSource!.Cancel();

                if (!_channelProcessingTask!.IsCompleted)
                {
                    Task.WaitAny([_channelProcessingTask], TimeSpan.FromSeconds(1));
                }

                while (_channelQueue.TryDequeue(out var message))
                {
                    _logChannel.Writer.TryWrite(message);
                }

                _logChannel.Writer.TryComplete();
                _logChannelCanncellationSource.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping Log Channel: {ex.Message}");
            }
            finally
            {
                _logChannelCanncellationSource = null;
                _channelProcessingTask = null;
            }
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

        private void ProcessChannelLogs()
        {
            if (_logChannelCanncellationSource is null)
            {
                Debug.WriteLine("Log Channel Cancellation Source is not initialized.");
                return;
            }
            try
            {
                while (!_logChannelCanncellationSource.Token.IsCancellationRequested)
                {
                    while (_channelQueue.TryDequeue(out var message))
                    {
                        if (!_logChannel.Writer.TryWrite(message))
                        {
                            Debug.WriteLine("Failed to write to channel");
                        }
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in log processing: {ex.Message}");
            }
        }        

        private void WriteLog(string message)
        {
            try
            {
                Debug.WriteLine(message);
                
                if(_channelProcessingTask is not null && !_channelProcessingTask.IsCompleted)
                {
                    _channelQueue.Enqueue(message);
                }
                _fileQueue.Enqueue(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error logging message: {ex.Message}");
            }
        }

        public IObservable<string> Logs => throw new NotImplementedException();
    }
}