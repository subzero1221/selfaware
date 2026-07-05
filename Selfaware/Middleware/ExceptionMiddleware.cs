using System.Net;
using System.Text.Json;
using Selfaware.Shared.Models;

namespace Selfaware.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";

                var statusCode = HttpStatusCode.InternalServerError;
                var displayMessage = "A server error occurred.";

                switch (ex)
                {
                    case ArgumentException:
                    case InvalidOperationException:
                    case FormatException:
                        statusCode = HttpStatusCode.BadRequest;
                        displayMessage = $"Invalid request data. Details: {ex.Message}";
                        break;
                    case UnauthorizedAccessException:
                        statusCode = HttpStatusCode.Unauthorized;
                        displayMessage = "Access denied.";
                        break;
                    case KeyNotFoundException:
                        statusCode = HttpStatusCode.NotFound;
                        displayMessage = "The requested resource was not found.";
                        break;
                    case TimeoutException:
                        statusCode = HttpStatusCode.GatewayTimeout;
                        displayMessage = "The operation timed out.";
                        break;
                }

                context.Response.StatusCode = (int)statusCode;

                var response = CustomResponse<object>.ErrorResponse(
                    displayMessage,
                    new List<string> { ex.Message }
                );

                var json = JsonSerializer.Serialize(
                    response,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );

                await context.Response.WriteAsync(json);
            }
        }
    }
}
