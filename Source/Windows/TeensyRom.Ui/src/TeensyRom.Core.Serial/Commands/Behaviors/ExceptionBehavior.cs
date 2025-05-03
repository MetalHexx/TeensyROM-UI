﻿using MediatR;
using System.Diagnostics;
using TeensyRom.Core.Commands;
using TeensyRom.Core.Common;
using TeensyRom.Core.Logging;

namespace TeensyRom.Core.Serial.Commands.Behaviors
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
            catch (TeensyDjException)
            {
                throw;
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
                Debug.WriteLine(ex);

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
                _ when ex.Message.Contains("Cannot perform serial operations in", StringComparison.OrdinalIgnoreCase) => "Error communicating with TeensyROM.",
                _ => $"Unexpected Error: {ex.Message}"
            };
    }
}