using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using TeensyRom.Core.Common;

namespace TeensyRom.Core.Logging
{
    public abstract class LoggingService : ILoggingService
    {
        public IObservable<string> Logs => _logs.AsObservable();
        protected Subject<string> _logs = new();
        private string _logPath => Path.Combine(Assembly.GetExecutingAssembly().GetPath(), LogConstants.LogPath).GetOsFriendlyPath();

        protected LoggingService()
        {
            if (File.Exists(_logPath))
            {
                File.Delete(_logPath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(_logPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
            }
            File.Create(_logPath).Close();

        }

        public virtual void Internal(string message, bool newLine = true) => Log(message, LogConstants.InternalColor, newLine);
        public virtual void InternalSuccess(string message, bool newLine = true) => Log(message, LogConstants.InternalSuccessColor, newLine);
        public virtual void InternalError(string message, bool newLine = true) => Log(message, LogConstants.InternalErrorColor, newLine);
        public virtual void External(string message, bool newLine = true) => Log($"[TR]: {message}", LogConstants.ExternalColor, newLine);
        public virtual void ExternalSuccess(string message, bool newLine = true) => Log($"[TR]: {message}", LogConstants.ExternalSuccessColor, newLine);
        public virtual void ExternalError(string message, bool newLine = true) => Log($"[TR]: {message}", LogConstants.ExternalErrorColor, newLine);

        private static readonly object _logFileLock = new object();

        public virtual void Log(string message, string hExColor, bool newLine = true)
        {
            lock (_logFileLock)
            {
                File.AppendAllText(_logPath, message + Environment.NewLine);
            }
        }
    }
}