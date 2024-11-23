namespace TeensyRom.Ui.Core.Logging
{
    public interface IAlertService
    {
        IObservable<string> CommandErrors { get; }

        void Publish(string error);
    }
}
