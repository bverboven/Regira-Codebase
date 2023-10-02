using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Regira.Web.ExceptionHandling.Abstractions;
using Regira.Web.Middleware;

namespace Regira.Web.ExceptionHandling;

public class GlobalExceptionHandlingMiddleware : IRequestHandler
{
    private readonly RequestDelegate _next;
    private readonly IExceptionHandler _exceptionHandler;
    public GlobalExceptionHandlingMiddleware(RequestDelegate next, IExceptionHandler exceptionHandler)
    {
        _next = next;
        _exceptionHandler = exceptionHandler;
    }


    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next.Invoke(httpContext);
        }
        catch (Exception ex)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await _exceptionHandler.HandleException(httpContext, ex);
        }
    }
}
public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}