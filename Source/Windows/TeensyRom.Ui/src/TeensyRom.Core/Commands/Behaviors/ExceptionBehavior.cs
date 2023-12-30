using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;

public class ExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : CommandResult, new()
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response;
        try
        {
            response = await next();
        }
        catch (Exception ex)
        {
            return new TResponse
            {
                IsSuccess = false,
                Error = $"An exception was thrown: \r\n{ex}" 
            };
        }
        return response;
    }
}