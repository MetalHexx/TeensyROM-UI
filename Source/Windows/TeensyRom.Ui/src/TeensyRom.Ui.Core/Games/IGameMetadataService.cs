using TeensyRom.Core.Storage.Entities;

namespace TeensyRom.Ui.Core.Games
{
    public interface IGameMetadataService
    {
        GameItem EnrichGame(GameItem game);
    }
}