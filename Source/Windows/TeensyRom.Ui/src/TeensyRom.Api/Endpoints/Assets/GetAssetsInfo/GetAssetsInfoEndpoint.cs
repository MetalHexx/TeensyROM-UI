//using RadEndpoints;
//using System.Reflection;
//using TeensyRom.Core.Assets;
//using TeensyRom.Core.Common;
//using TeensyRom.Core.Games;
//using TeensyRom.Core.Music;

//namespace TeensyRom.Api.Endpoints.Assets.GetAssetsInfo
//{
//    public class GetAssetsInfoEndpoint : RadEndpointWithoutRequest<GetAssetsInfoResponse>
//    {
//        public override void Configure()
//        {
//            Get("/assets/info")
//                .Produces<GetAssetsInfoResponse>(StatusCodes.Status200OK)
//                .WithName("GetAssetsInfo")
//                .WithSummary("Get Asset URLs for Frontend")
//                .WithTags("Assets")
//                .WithDescription(
//                    "Provides complete asset URLs for frontend applications to serve images in the UI.\n\n" +
//                    "**Frontend Integration:**\n" +
//                    "- Returns complete URLs ready for use in image src attributes\n" +
//                    "- Designed for Angular/React/Vue applications to display TeensyROM assets\n" +
//                    "- No need to construct URLs - use the provided complete URLs directly\n\n" +
//                    "**Available Asset Categories:**\n" +
//                    "- **Game Images**: General game-related images and artwork\n" +
//                    "- **Loading Screens**: Game loading screen images for visual previews\n" +
//                    "- **Screenshots**: Game screenshot images for thumbnails and galleries\n" +
//                    "- **Composer Images**: Musician/composer profile images\n" +
//                    "- **Other Images**: Logos, hardware images, and miscellaneous assets\n\n" +
//                    "**Usage Example:**\n" +
//                    "```typescript\n" +
//                    "const response = await fetch('/assets/info');\n" +
//                    "const assets = await response.json();\n" +
//                    "// Use directly in img src:\n" +
//                    "<img src={`${assets.gameScreenshotsUrl}my-game.png`} />\n" +
//                    "```"
//                );
//        }

//        public override Task Handle(CancellationToken ct)
//        {
//            // Use the same path resolution as AssetHelper to ensure consistency
//            var assetsPath = Path.Combine(Assembly.GetExecutingAssembly().GetPath(), "Assets");
//            var assetsExist = Directory.Exists(assetsPath);
            
//            // Build base URL
//            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            
//            // Convert backslash paths to forward slash URL paths and create complete URLs
//            var gameImagesPath = "/" + GameConstants.Game_Image_Local_Path.Replace('\\', '/') + "/";
//            var composerImagesPath = "/" + MusicConstants.Musician_Image_Local_Path.Replace('\\', '/') + "/";
//            var otherImagesPath = "/" + AssetConstants.OtherImagePath.Replace('\\', '/') + "/";
            
//            Response = new GetAssetsInfoResponse
//            {
//                AssetsAvailable = assetsExist,
//                GameImagesUrl = baseUrl + gameImagesPath,
//                GameLoadingScreensUrl = baseUrl + gameImagesPath + GameConstants.Loading_Screen_Sub_Path + "/",
//                GameScreenshotsUrl = baseUrl + gameImagesPath + GameConstants.Screenshots_Sub_Path + "/",
//                ComposerImagesUrl = baseUrl + composerImagesPath,
//                OtherImagesUrl = baseUrl + otherImagesPath,
//                BaseUrl = baseUrl,
//                ExampleUrls = new[]
//                {
//                    $"{baseUrl}{gameImagesPath}{GameConstants.Loading_Screen_Sub_Path}/Ghosts'n Goblins.png",
//                    $"{baseUrl}{gameImagesPath}{GameConstants.Screenshots_Sub_Path}/Congo Bongo.png",
//                    $"{baseUrl}{composerImagesPath}musicians_z_zyron.jpg",
//                    $"{baseUrl}{otherImagesPath}Sensorium.png"
//                }
//            };
            
//            Send();
//            return Task.CompletedTask;
//        }
//    }
//}