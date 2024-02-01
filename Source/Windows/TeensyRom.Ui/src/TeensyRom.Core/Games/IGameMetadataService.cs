using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Core.Games
{
    public interface IGameMetadataService
    {
        void EnrichGame(GameItem game);
        void GetGameScreens(GameItem game);
    }
}