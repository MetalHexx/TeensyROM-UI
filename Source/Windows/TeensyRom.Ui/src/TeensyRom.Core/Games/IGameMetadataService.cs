using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Games
{
    public interface IGameMetadataService
    {
        GameItem EnrichGame(GameItem game);
    }
}