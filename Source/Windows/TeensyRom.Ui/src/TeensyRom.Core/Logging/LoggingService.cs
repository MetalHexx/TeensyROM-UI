﻿using System.Drawing;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using TeensyRom.Core.Common;
using TeensyRom.Core.Games;

namespace TeensyRom.Core.Logging
{
    public class LoggingService : ILoggingService
    {
        public IObservable<string> Logs => _logs.AsObservable();
        protected Subject<string> _logs = new();

        private string _logPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.LogPath);

        public LoggingService()
        {
            if(File.Exists(_logPath))
            {
                File.Delete(_logPath);
            } 
            if(!Directory.Exists(Path.GetDirectoryName(_logPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            }
            File.Create(_logPath).Close();
        }

        private static readonly object _logFileLock = new object();
        public void Log(string message, string hExColor)
        {
            lock (_logFileLock)
            {
                File.AppendAllText(_logPath, message + Environment.NewLine);
            }

            var sb = new StringBuilder();

            _logs.OnNext(message
                .SplitAtCarriageReturn()
                .Select(line => line.WithColor(hExColor))
                .Aggregate(sb, (acc, line) => acc.AppendWithLimit(line))
                .ToString()
                .DropLastNewLine());
        }

        public void Internal(string message) => Log(message, LogConstants.InternalColor);
        public void InternalSuccess(string message) => Log(message, LogConstants.InternalSuccessColor);
        public void InternalError(string message) => Log(message, LogConstants.InternalErrorColor);
        public void External(string message) => Log($"[TR]: {message}", LogConstants.ExternalColor);
        public void ExternalSuccess(string message) => Log($"[TR]: {message}", LogConstants.ExternalSuccessColor);
        public void ExternalError(string message) => Log($"[TR]: {message}", LogConstants.ExternalErrorColor);
    }
}