using MediatR;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult
{
    private readonly ILoggingService _logService;

    public LoggingBehavior(ILoggingService logService)
    {
        _logService = logService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;

        _logService.Log($"{requestType} Started {FormatRequest(request)}\r\n");

        var response = await next();

        _logService.Log($"{requestType} Completed {FormatResponse(response)}\r\n");

        return response;
    }

    private string FormatRequest(TRequest request)
    {
        var properties = request.GetType().GetProperties();
        var sb = new StringBuilder();

        if(properties.Length == 0) return string.Empty;
        
        foreach (var property in properties)
        {
            sb.Append($"{property.Name}: {property.GetValue(request)?.ToString() ?? "<null>"}\r\n");
        }
        return $"with properties: \r\n {sb.ToString().TrimEnd(',', ' ')}";
    }

    private string FormatResponse(TResponse response) => string.IsNullOrWhiteSpace(response.Error)
            ? "(Success)"
            : $"(Failure) => {response.Error}";
}
