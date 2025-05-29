namespace TeensyRom.Api.Startup
{
    public static class HttpStartupExtensions
    {
        /// <summary>
        /// Adds the CORS policy for allowing the Angular development server.
        /// </summary>
        public static IServiceCollection AddUiCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularDevServer",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200") // Angular dev server
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();

                        builder.WithOrigins("http://localhost:3000") // Front-end integration tests
                               .AllowAnyMethod()
                               .AllowAnyHeader()
                               .AllowCredentials();
                    });
            });
            return services;
        }

        /// <summary>
        /// Applies the CORS policy for the Angular development server.
        /// </summary>
        public static WebApplication UseUiCors(this WebApplication app) 
        {
            app.UseCors("AllowAngularDevServer");
            return app;
        }
    }
}