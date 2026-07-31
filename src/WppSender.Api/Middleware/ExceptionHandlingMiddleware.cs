using System.Net;
using System.Text.Json;
using WppSender.Api.Auth;

namespace WppSender.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var corpo = JsonSerializer.Serialize(new ErroResponse("Ocorreu um erro inesperado. Tente novamente."));
            await context.Response.WriteAsync(corpo);
        }
    }
}
