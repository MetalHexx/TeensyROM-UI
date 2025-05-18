using TeensyRom.Core.Storage;

namespace TeensyRom.Api.Endpoints.Files.GetDirectory
{
    public class GetDirectoryMapper : IRadMapper<GetDirectoryResponse, IStorageCacheItem>
    {
        public GetDirectoryResponse FromEntity(IStorageCacheItem e) => new GetDirectoryResponse
        {
            StorageItem = StorageCacheViewModel.FromCache(e),
        };
    }
}