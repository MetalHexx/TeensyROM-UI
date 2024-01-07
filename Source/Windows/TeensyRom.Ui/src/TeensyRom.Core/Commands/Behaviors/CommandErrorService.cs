using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Core.Commands.Behaviors
{
    public interface ICommandErrorService
    {
        IObservable<string> CommandErrors { get; }

        void PublishError(string error);
    }

    public class CommandErrorService : ICommandErrorService
    {
        public IObservable<string> CommandErrors => _commandErrors.AsObservable();
        private readonly Subject<string> _commandErrors = new();
        public void PublishError(string error)
        {
            _commandErrors.OnNext(error);
        }
    }
}
