using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace TeensyRom.Api.Http
{
    public static class EndpointHelper
    {
        public const string FindDevicesRateLimiter = "FindDevicesRateLimiterPolicy";
        public const string GetLogsRateLimiter = "GetLogsRateLimiterPolicy";
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
                options.AddFixedWindowLimiter(GetLogsRateLimiter, config =>
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
