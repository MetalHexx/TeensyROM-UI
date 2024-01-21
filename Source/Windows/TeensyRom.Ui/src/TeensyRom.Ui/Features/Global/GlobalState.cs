using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Serial.State;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Features.Files.State;
using TeensyRom.Ui.Features.Music.State;

namespace TeensyRom.Ui.Features.Global
{
    public interface IGlobalState
    {
        IObservable<FileItem> FileViewLaunched { get; }
        IObservable<bool> SerialConnected { get; }
    }

    public class GlobalState(IFileState fileState, ISerialStateContext serialContext) : IGlobalState
    {
        public IObservable<FileItem> FileViewLaunched => fileState.FileViewLaunch;

        public IObservable<bool> SerialConnected => serialContext.CurrentState
            .Select(state => state is SerialBusyState or SerialConnectedState);

    }
}
