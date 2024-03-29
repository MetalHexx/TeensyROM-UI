using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Services
{
    public interface IProgressService
    {
        IObservable<bool> InProgress { get; }
        void EnableProgress();
        void DisableProgress();
    }
    public class ProgressService : IProgressService
    {
        public IObservable<bool> InProgress => _inProgress.AsObservable();
        private readonly BehaviorSubject<bool> _inProgress = new(false);

        public void EnableProgress()
        {
            _inProgress.OnNext(true);
        }

        public void DisableProgress() 
        {
            _inProgress.OnNext(false);
        }
    }
}
