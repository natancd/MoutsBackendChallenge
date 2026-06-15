using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.WebApi.Common;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

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
        catch (KeyNotFoundException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound", ex.Message, ex.Message);
        }
        catch (DomainException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, "DomainError", ex.Message, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status400BadRequest, "BusinessRuleViolation", ex.Message, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorResponseAsync(context, StatusCodes.Status401Unauthorized, "AuthenticationError", ex.Message, ex.Message);
        }
    }

    private static Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string type,
        string error,
        string detail)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            type,
            error,
            detail
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
