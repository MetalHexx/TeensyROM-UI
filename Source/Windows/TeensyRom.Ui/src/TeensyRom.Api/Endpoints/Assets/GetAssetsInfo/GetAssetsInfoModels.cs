namespace TeensyRom.Api.Endpoints.Assets.GetAssetsInfo
{
    /// <summary>
    /// Response containing complete asset URLs for frontend consumption
    /// </summary>
    public class GetAssetsInfoResponse
    {
        /// <summary>
        /// Indicates whether the assets directory exists and is available
        /// </summary>
        public bool AssetsAvailable { get; set; }
        
        /// <summary>
        /// Complete URL for game images (for frontend image src attributes)
        /// </summary>
        public string GameImagesUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Complete URL for game loading screen images (for frontend image src attributes)
        /// </summary>
        public string GameLoadingScreensUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Complete URL for game screenshot images (for frontend image src attributes)
        /// </summary>
        public string GameScreenshotsUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Complete URL for composer images (for frontend image src attributes)
        /// </summary>
        public string ComposerImagesUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Complete URL for other miscellaneous images like logos and hardware images (for frontend image src attributes)
        /// </summary>
        public string OtherImagesUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Base URL of the API server
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;
        
        /// <summary>
        /// Example complete URLs showing how to access specific assets in frontend applications
        /// </summary>
        public string[] ExampleUrls { get; set; } = [];
    }
}