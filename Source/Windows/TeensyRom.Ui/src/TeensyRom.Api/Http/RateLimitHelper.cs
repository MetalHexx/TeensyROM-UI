using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace TeensyRom.Api.Http
{
    /// <summary>
    /// 
    /// These rate limiters help prevent fatal serial states should the user decide to spam their browser refresh.
    /// 
    /// <remarks>
    /// 
    /// Originally, this was developed because if the api receives a cancellation in the middle of the serial discovery 
    /// phaase, it was causing a bad state.  
    /// 
    /// Rather than create a complex solution on the backend, this hack is a much
    /// simpler and elegant solution for this sort of edge case.
    /// 
    /// </remarks>
    /// </summary>
    public static class RateLimitHelper
    {
        public const string FindDevicesRateLimiter = "FindDevicesRateLimiterPolicy";
        public static IServiceCollection AddStrictRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter(FindDevicesRateLimiter, config =>
                {
                    config.PermitLimit = 1;
                    config.Window = TimeSpan.FromSeconds(5);
                    config.QueueProcessingOrder = QueueProcessingOrder.NewestFirst;
                    config.QueueLimit = 1;
                });
            });
            return services;
        }
    }
}
