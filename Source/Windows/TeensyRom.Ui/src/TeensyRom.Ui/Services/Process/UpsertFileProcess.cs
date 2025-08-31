using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Storage;

namespace TeensyRom.Ui.Services.Process
{
    public interface IUpsertFileProcess
    {
        Task ReorderFiles(IEnumerable<FileItem> files);
        Task UpsertFile(FileItem file);
    }
    public class UpsertFileProcess(ICachedStorageService storage) : IUpsertFileProcess
    {     
        public async Task UpsertFile(FileItem file) => await storage.UpsertFiles([file]);
        public async Task ReorderFiles(IEnumerable<FileItem> files) 
        {
            var customFiles = files.Select((f, index) =>
            {
                if (f.Custom is null)
                {
                    f.Custom = new PlaylistItem
                    {
                        FilePath = f.Path
                    };
                }
                f.Custom.Order = index;
                return f;
            });
            await storage.UpsertFiles(customFiles);
        }
    }
}