namespace TeensyRom.Core.Logging
{
    public interface ILoggingService
    {
        IObservable<string> Logs { get; }

        void External(string message);
        void ExternalSuccess(string message);
        void ExternalError(string message);
        void Internal(string message);
        void InternalError(string message);
        void InternalSuccess(string message);
    }
}
