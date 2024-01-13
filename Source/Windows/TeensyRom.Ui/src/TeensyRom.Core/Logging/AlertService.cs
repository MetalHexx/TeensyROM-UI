using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Logging
{

    public class AlertService : IAlertService
    {
        public IObservable<string> CommandErrors => _commandErrors.AsObservable();
        private readonly Subject<string> _commandErrors = new();
        public void Publish(string error)
        {
            _commandErrors.OnNext(error);
        }
    }
}
