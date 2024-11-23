namespace TeensyRom.Cli.Core.Logging
{
    public interface IAlertService
    {
        IObservable<string> CommandErrors { get; }

        void Publish(string message);
        void PublishError(string message);
    }
}
