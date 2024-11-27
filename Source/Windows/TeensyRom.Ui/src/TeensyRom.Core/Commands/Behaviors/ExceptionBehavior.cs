using MediatR;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Commands.Behaviors
{
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
            catch (TeensyBusyException ex)
            {
                alert.Publish(ex.Message);
                return new TResponse
                {
                    IsSuccess = false,
                    IsBusy = true,
                    Error = ex.Message
                };
            }
            catch (Exception ex)
            {
                var message = GetExceptionMessage(ex);
                alert.Publish(message);

                return new TResponse
                {
                    IsSuccess = false,
                    Error = message
                };
            }
            return response;
        }

        public string GetExceptionMessage(Exception ex) =>
            ex switch
            {
                _ when ex.Message.Contains("port is closed", StringComparison.OrdinalIgnoreCase) => "Disconnected from TeensyROM",
                _ when ex.Message.Contains("Cannot perform serial operations in", StringComparison.OrdinalIgnoreCase) => "Error communicating with TeensyROM.\rGo to the terminal to check the logs and connection.",
                _ => $"Unexpected Error: {ex.Message}. See logs."
            };
    }
}