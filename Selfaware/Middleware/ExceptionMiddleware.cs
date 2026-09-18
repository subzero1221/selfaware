using Microsoft.EntityFrameworkCore;
using Npgsql;
using Selfaware.Shared.Models;
using System.Net;
using System.Text.Json;

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
                    case DbUpdateException dbEx when dbEx.InnerException is PostgresException pgEx:
                        switch (pgEx.SqlState)
                        {
                            case PostgresErrorCodes.UniqueViolation: 
                                statusCode = HttpStatusCode.Conflict; 
                                displayMessage = "You have already submitted an answer for this question.";
                                break;

                            case PostgresErrorCodes.ForeignKeyViolation: 
                                statusCode = HttpStatusCode.BadRequest; 
                                displayMessage = "The provided survey session or option does not exist.";
                                break;

                            default:
                                statusCode = HttpStatusCode.BadRequest;
                                displayMessage = "A database constraint error occurred.";
                                break;
                        }
                        break;

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
