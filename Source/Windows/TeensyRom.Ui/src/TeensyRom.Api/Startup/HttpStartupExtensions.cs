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
                    });
            });
            return services;
        }
    }
}