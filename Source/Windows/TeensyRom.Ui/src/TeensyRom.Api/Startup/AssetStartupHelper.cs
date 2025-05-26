using TeensyRom.Core.Assets;
using TeensyRom.Core.Games;
using TeensyRom.Core.Music;
using TeensyRom.Core.Common;

namespace TeensyRom.Api.Startup
{
    public static class AssetStartupHelper
    {
        /// <summary>
        /// Unpacks required assets for the TeensyROM application.
        /// </summary>
        public static void UnpackAssets()
        {
            AssetHelper.UnpackAssets(GameConstants.Game_Image_Local_Path, "OneLoad64.zip");
            AssetHelper.UnpackAssets(MusicConstants.Musician_Image_Local_Path, "Composers.zip");
            AssetHelper.UnpackAssets(AssetConstants.VicePath, "vice-bins.zip");
        }
    }
}