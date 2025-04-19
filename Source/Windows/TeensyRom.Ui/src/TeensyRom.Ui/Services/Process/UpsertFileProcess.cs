using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Storage.Entities;
using TeensyRom.Core.Storage.Services;

namespace TeensyRom.Ui.Services.Process
{
    public interface IUpsertFileProcess
    {
        Task ReorderFiles(IEnumerable<IFileItem> files);
        Task UpsertFile(IFileItem file);
    }
    public class UpsertFileProcess(ICachedStorageService storage) : IUpsertFileProcess
    {     
        public async Task UpsertFile(IFileItem file) => await storage.UpsertFiles([file]);
        public async Task ReorderFiles(IEnumerable<IFileItem> files) 
        {
            var customFiles = files.Select((f, index) =>
            {
                f.Custom = f.Custom is not null ? f.Custom : new PlaylistItem
                {
                    FilePath = f.Path
                };
                f.Custom.Order = index;
                return f;
            });
            await storage.UpsertFiles(customFiles);
        }
    }
}