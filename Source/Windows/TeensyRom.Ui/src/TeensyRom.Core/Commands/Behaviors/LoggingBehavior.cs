using MediatR;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : TeensyCommandResult
{
    private readonly ILoggingService _logService;

    public LoggingBehavior(ILoggingService logService)
    {
        _logService = logService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;

        _logService.Internal($"{requestType} Started {LoggingBehavior<TRequest, TResponse>.FormatRequest(request)}");

        var response = await next();

        if (IsSuccess(response))
        {
            _logService.InternalSuccess($"{requestType} Completed {FormatResponse(response)}");
            return response;
        }
            
        _logService.InternalError($"{requestType} Completed {FormatResponse(response)}");

        return response;
    }

    private static string FormatRequest(TRequest request)
    {
        var properties = request.GetType().GetProperties();
        var sb = new StringBuilder();

        if(properties.Length == 0) return string.Empty;
        
        foreach (var property in properties)
        {
            sb.AppendWithLimit($"=> {property.Name}: {property.GetValue(request)?.ToString() ?? "<null>"}\r\n");
        }
        return $"{sb.ToString().DropLastComma()}";
    }

    private string FormatResponse(TResponse response) => string.IsNullOrWhiteSpace(response.Error)
            ? "(Success)"
            : $"(Failure) => {response.Error}";

    private bool IsSuccess(TResponse response) => string.IsNullOrWhiteSpace(response.Error);
}
