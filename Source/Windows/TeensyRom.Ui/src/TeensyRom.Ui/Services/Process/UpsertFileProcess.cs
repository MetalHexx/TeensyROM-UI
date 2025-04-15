using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Ui.Services.Process
{
    public interface IUpsertFileProcess
    {
        Task UpsertFile(ILaunchableItem file);
    }
    public class UpsertFileProcess(ICachedStorageService storage) : IUpsertFileProcess
    {     
        public async Task UpsertFile(ILaunchableItem file) => await storage.UpsertFiles([file]);
    }
}