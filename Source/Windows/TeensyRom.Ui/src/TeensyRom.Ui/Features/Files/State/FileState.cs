using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace TeensyRom.Ui.Features.Files.State
{
    public interface IFileState
    {
        IObservable<Unit> LoadDirectory(string path);
    }
    public class FileState : IFileState, IDisposable
    {
        public FileState()
        {
            
        }
        public IObservable<Unit> LoadDirectory(string path)
        {
            return null;
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
