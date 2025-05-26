using System.Reflection;
using Scalar.AspNetCore;

namespace TeensyRom.Api.Startup
{
    public static class OpenApiStartupExt
    {
        /// <summary>
        /// Adds and configures OpenAPI/Scalar with XML comments and custom UI options.
        /// </summary>
        public static IServiceCollection AddApiDocs(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddOpenApi();
            return services;
        }

        /// <summary>
        /// Maps OpenAPI and Scalar endpoints with custom UI options.
        /// </summary>
        public static WebApplication MapApiDocs(this WebApplication app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options =>
            {
                options
                    .WithTitle("TeensyROM API")
                    .WithTheme(ScalarTheme.Laserwave);
            });
            return app;
        }
    }
}