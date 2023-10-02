using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Regira.Web.ExceptionHandling.Abstractions;

namespace Regira.Web.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILoggerFactory _loggerFactory;
    public GlobalExceptionHandler(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    public Task HandleException(HttpContext context, Exception ex)
    {
        var logger = _loggerFactory.CreateLogger(GetType());
        logger.LogError(ex, ex.Message);

        return Task.CompletedTask;
    }
}