using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using TeensyRom.Core.Settings;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Ui.Services;

namespace TeensyRom.Ui.Controls.CornerToolbar
{
    public class CornerToolbarViewModel : ReactiveObject
    {
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; set; }
        public ReactiveCommand<Unit, Unit> PlayRandomCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        public CornerToolbarViewModel(Func<Task> cacheAllFunc, Func<Task> launchRandomFunc, Func<bool, Task> refreshDirFunc, IDialogService dialog, TeensyStorageType storageType)
        {
            RefreshCommand = ReactiveCommand.CreateFromTask<Unit>(
                execute: _ => refreshDirFunc(true),
                outputScheduler: RxApp.MainThreadScheduler);

            PlayRandomCommand = ReactiveCommand.CreateFromTask(
                execute: launchRandomFunc,
                outputScheduler: RxApp.MainThreadScheduler);

            CacheAllCommand = ReactiveCommand.CreateFromTask(
                execute: async _ =>
                {
                    var confirm = await dialog.ShowConfirmation($"Cache All \r\rCreates a local index of all the files stored on your {storageType} storage.  This will enable rich discovery of music and programs though search and file launch randomization.\r\rThis may take a few minutes if you have thousands of files. Proceed?");
                    if (!confirm) return;

                    await cacheAllFunc();
                },
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}
