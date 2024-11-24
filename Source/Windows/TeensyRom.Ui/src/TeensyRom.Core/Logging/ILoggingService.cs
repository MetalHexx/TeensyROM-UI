namespace TeensyRom.Core.Logging
{
    public interface ILoggingService
    {
        IObservable<string> Logs { get; }

        void External(string message, bool newLine = true);
        void ExternalSuccess(string message, bool newLine = true);
        void ExternalError(string message, bool newLine = true);
        void Internal(string message, bool newLine = true);
        void InternalError(string message, bool newLine = true);
        void InternalSuccess(string message, bool newLine = true);
        void Log(string message, string hExColor, bool newLine = true);
    }
}
