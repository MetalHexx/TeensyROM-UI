using MediatR;
using System.Diagnostics;
using System.Text;
using TeensyRom.Cli.Core.Commands;
using TeensyRom.Core.Logging;

namespace TeensyRom.Cli.Core.Commands.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : TeensyCommandResult
    {
        private readonly ILoggingService _logs;

        public LoggingBehavior(ILoggingService logs)
        {
            _logs = logs;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestType = typeof(TRequest).Name;

            _logs.Internal($"{requestType} Started {LoggingBehavior<TRequest, TResponse>.FormatRequest(request)}");

            var sw = Stopwatch.StartNew();

            var response = await next();

            sw.Stop();

            if (IsSuccess(response))
            {
                _logs.InternalSuccess($"{requestType} Completed in {sw.ElapsedMilliseconds}ms {FormatResponse(response)}\n");
                return response;
            }

            _logs.InternalError($"{requestType} Completed {FormatResponse(response)}\n");

            return response;
        }

        private static string FormatRequest(TRequest request)
        {
            if (request == null) return string.Empty;

            var properties = request.GetType().GetProperties();
            StringBuilder sb = new();

            if (properties.Length == 0) return string.Empty;

            foreach (var property in properties)
            {
                if (IsNativeType(property.PropertyType))
                {
                    sb.AppendLine($"\r => {property.Name}: {property.GetValue(request)?.ToString() ?? "<null>"}");
                }
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsNativeType(Type type)
        {
            return type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type.IsEnum;
        }

        private string FormatResponse(TResponse response) => string.IsNullOrWhiteSpace(response.Error)
                ? "(Success)"
                : $"(Failure) => {response.Error}";

        private bool IsSuccess(TResponse response) => string.IsNullOrWhiteSpace(response.Error);
    }

}