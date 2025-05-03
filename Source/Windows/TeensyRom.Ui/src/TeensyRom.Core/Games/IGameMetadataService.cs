using TeensyRom.Core.Entities.Storage;

namespace TeensyRom.Core.Games
{
    public interface IGameMetadataService
    {
        GameItem EnrichGame(GameItem game);
    }
}