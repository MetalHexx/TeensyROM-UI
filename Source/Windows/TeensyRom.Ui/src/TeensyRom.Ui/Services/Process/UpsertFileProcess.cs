using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Ui.Services.Process
{
    public interface IUpsertFileProcess
    {
        void UpsertFile(ILaunchableItem file);
    }
    public class UpsertFileProcess(ICachedStorageService storage) : IUpsertFileProcess
    {     
        public void UpsertFile(ILaunchableItem file) => storage.UpsertFiles([file]);
    }
}