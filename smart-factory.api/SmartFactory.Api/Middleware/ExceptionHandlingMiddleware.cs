using Microsoft.AspNetCore.Http;
using SmartFactory.Application.DTOs;
using SmartFactory.Application.Exceptions;
using Serilog;
using System.Net;
using System.Text.Json;

namespace SmartFactory.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case BaseException baseEx:
                // Handle custom application exceptions
                response.StatusCode = baseEx.StatusCode;
                errorResponse.ErrorCode = baseEx.ErrorCode;
                errorResponse.Message = baseEx.Message;

                if (baseEx is ValidationException validationEx)
                {
                    errorResponse.ValidationErrors = validationEx.Errors;
                }

                Log.Warning(exception, "Application exception: {ErrorCode} - {Message}", 
                    baseEx.ErrorCode, baseEx.Message);
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.ErrorCode = "UNAUTHORIZED";
                errorResponse.Message = "You are not authorized to access this resource.";
                Log.Warning(exception, "Unauthorized access attempt");
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.ErrorCode = "NOT_FOUND";
                errorResponse.Message = exception.Message;
                Log.Warning(exception, "Resource not found");
                break;

            case ArgumentNullException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = "BAD_REQUEST";
                errorResponse.Message = exception.Message;
                Log.Warning(exception, "Bad request - null argument");
                break;

            case ArgumentException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.ErrorCode = "BAD_REQUEST";
                errorResponse.Message = exception.Message;
                Log.Warning(exception, "Bad request");
                break;

            case InvalidOperationException:
                response.StatusCode = (int)HttpStatusCode.Conflict;
                errorResponse.ErrorCode = "CONFLICT";
                errorResponse.Message = exception.Message;
                Log.Warning(exception, "Invalid operation");
                break;

            case TimeoutException:
                response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                errorResponse.ErrorCode = "TIMEOUT";
                errorResponse.Message = "The request timed out. Please try again.";
                Log.Warning(exception, "Request timeout");
                break;

            default:
                // Handle unexpected errors
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.ErrorCode = "INTERNAL_SERVER_ERROR";
                errorResponse.Message = "An unexpected error occurred. Please contact support.";
                
                // Log full exception details for server errors
                Log.Error(exception, "Unhandled exception occurred");

                // Include detailed error information in development environment
                if (IsDevelopmentEnvironment())
                {
                    errorResponse.Details = exception.ToString();
                }
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteAsync(jsonResponse);
    }

    private static bool IsDevelopmentEnvironment()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return environment?.Equals("Development", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}

/// <summary>
/// Extension method to register the exception handling middleware
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
