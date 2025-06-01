namespace TeensyRom.Core.Logging
{
    public interface ILoggingService
    {
        IObservable<string> Logs { get; }

        void External(string message, string? deviceId = null);
        void ExternalSuccess(string message, string? deviceId = null);
        void ExternalError(string message, string? deviceId = null);
        void Internal(string message, string? deviceId = null);
        void InternalError(string message, string? deviceId = null);
        void InternalWarning(string message, string? deviceId = null);
        void InternalSuccess(string message, string? deviceId = null);
    }
}
