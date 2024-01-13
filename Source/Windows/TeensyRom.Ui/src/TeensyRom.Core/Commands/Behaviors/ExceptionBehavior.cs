using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Logging;

public class ExceptionBehavior<TRequest, TResponse>(IAlertService alert) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : TeensyCommandResult, new()
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
            alert.Publish(ex.Message);
            return new TResponse
            {
                IsSuccess = false,
                Error = $"An exception was thrown: \r\n{ex}" 
            };
        }
        return response;
    }
}