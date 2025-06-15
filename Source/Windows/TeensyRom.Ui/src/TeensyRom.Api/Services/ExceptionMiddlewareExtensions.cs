using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeensyRom.Core.Logging;

namespace TeensyRom.Api.Services;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddProblemDetailsWithLogging(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                if (ctx.Exception != null)
                {
                    var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILoggingService>();
                    logger.InternalError($"Unhandled exception: {ctx.Exception}");
                }
            };
        });
        return services;
    }
}
