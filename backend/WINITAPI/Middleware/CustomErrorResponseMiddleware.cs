using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Winit.Shared.Models.Common;
using System.Data.SqlClient;

namespace WINITAPI.Middleware
{
    /// <summary>
    /// Custom middleware for handling exceptions and providing consistent error responses
    /// </summary>
    /// <summary>
    /// Custom middleware for handling exceptions and providing consistent error responses
    /// </summary>
    public class CustomErrorResponseMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomErrorResponseMiddleware> _logger;

        public CustomErrorResponseMiddleware(RequestDelegate next, ILogger<CustomErrorResponseMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred in {Method} {Path}", 
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = new ApiResponse<object>(
                data: null,
                statusCode: GetStatusCode(exception),
                errorMessage: GetErrorMessage(exception)
            );

            context.Response.StatusCode = errorResponse.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.BadRequest,
                TimeoutException => (int)HttpStatusCode.RequestTimeout,
                Microsoft.Data.SqlClient.SqlException => (int)HttpStatusCode.ServiceUnavailable,
                Npgsql.NpgsqlException => (int)HttpStatusCode.ServiceUnavailable,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private static string GetErrorMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentException => "Invalid request parameters. Please check your input and try again.",
                UnauthorizedAccessException => "Access denied. Please ensure you have proper authorization.",
                InvalidOperationException => "Invalid operation. Please check your request and try again.",
                TimeoutException => "Request timed out. Please try again later.",
                Microsoft.Data.SqlClient.SqlException => "Database connection error. Please try again later.",
                Npgsql.NpgsqlException => "Database connection error. Please try again later.",
                _ => "An unexpected error occurred. Please try again later."
            };
        }
    }

    /// <summary>
    /// Extension method to register the custom error handling middleware
    /// </summary>
    public static class CustomErrorResponseMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomErrorResponseMiddleware>();
        }
    }
}
