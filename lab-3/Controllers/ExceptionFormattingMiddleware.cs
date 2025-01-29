using Microsoft.AspNetCore.Http;

namespace IntegratorIntoAspNet.Controllers;

public class ExceptionFormattingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            await HandleException(context, e);
        }
    }

    private Task HandleException(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";

        int statusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError,
        };

        httpContext.Response.StatusCode = statusCode;

        var errorData = new ErrorData()
        {
            Message = exception.Message,
            StatusCode = statusCode,
        };
        return httpContext.Response.WriteAsJsonAsync(errorData);
    }
}