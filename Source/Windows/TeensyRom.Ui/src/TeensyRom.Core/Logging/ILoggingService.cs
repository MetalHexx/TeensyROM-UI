namespace TeensyRom.Core.Logging
{
    public interface ILoggingService
    {
        IObservable<string> Logs { get; }
        void Log(string message);
    }
}
