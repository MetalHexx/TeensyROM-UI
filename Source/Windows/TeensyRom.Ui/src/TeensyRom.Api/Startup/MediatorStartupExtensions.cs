using MediatR;
using TeensyRom.Core.Serial;
using TeensyRom.Core.Serial.Commands.Behaviors;

namespace TeensyRom.Api.Startup
{
    public static class MediatorStartupExtensions
    {
        /// <summary>
        /// Adds MediatR and pipeline behaviors for TeensyROM.
        /// </summary>
        public static IServiceCollection AddTeensyRomMediatR(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CoreSerialAssemblyMarker>());
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ExceptionBehavior<,>));
            services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(SerialBehavior<,>));
            return services;
        }
    }
}