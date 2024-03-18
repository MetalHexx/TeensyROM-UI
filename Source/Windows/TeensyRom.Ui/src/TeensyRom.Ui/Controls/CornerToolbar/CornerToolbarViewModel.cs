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
        public ReactiveCommand<Unit, Unit> CacheAllCommand { get; set; }

        public CornerToolbarViewModel(Func<Task> cacheAllFunc, IDialogService dialog, TeensyStorageType storageType)
        {
            CacheAllCommand = ReactiveCommand.CreateFromTask(
                execute: async _ =>
                {
                    var confirm = await dialog.ShowConfirmation($"This will create a local index of all the files stored on your {storageType} to enable search and file launch randomization across your entire collection.\r\rThis may take a few minutes if you have thousands of files. Proceed?");
                    if (!confirm) return;

                    await cacheAllFunc();
                },
                outputScheduler: RxApp.MainThreadScheduler);
        }
    }
}
