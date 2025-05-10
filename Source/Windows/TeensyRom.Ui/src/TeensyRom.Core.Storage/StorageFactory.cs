using MediatR;
using TeensyRom.Core.Abstractions;
using TeensyRom.Core.Entities.Storage;
using TeensyRom.Core.Games;
using TeensyRom.Core.Logging;
using TeensyRom.Core.Music.Sid;

namespace TeensyRom.Core.Storage
{
    public class StorageFactory(IMediator mediator, IGameMetadataService gameMetadata, ISidMetadataService sidMetadata, ILoggingService log, IAlertService alert) : IStorageFactory
    {
        public IStorageService Create(CartStorage cartStorage)
        {
            var settings = new StorageSettings
            {
                CartStorage = cartStorage,
            };
            var storageCache = new SimpleStorageCache(cartStorage, settings);
            var storageService = new StorageService(storageCache, settings, mediator, alert, log, sidMetadata, gameMetadata);
            return storageService;
        }
    }
}
