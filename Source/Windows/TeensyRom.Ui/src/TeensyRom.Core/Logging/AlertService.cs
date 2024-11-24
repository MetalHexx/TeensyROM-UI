using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace TeensyRom.Core.Logging
{
    public class AlertService : IAlertService
    {
        public IObservable<string> CommandErrors => _commandErrors.AsObservable();
        private readonly Subject<string> _commandErrors = new();
        public void Publish(string message)
        {
            _commandErrors.OnNext(message);
        }

        public void PublishError(string message)
        {
            _commandErrors.OnNext(message);
        }
    }
}
