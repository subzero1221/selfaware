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
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";

                var statusCode = HttpStatusCode.InternalServerError; 
                var message = "Something went wrong on our end.";

                if (ex is ArgumentException || ex is InvalidOperationException)
                {
           
                    statusCode = HttpStatusCode.BadRequest; 
                    message = ex.Message;
                }
                else if (ex is UnauthorizedAccessException)
                {
                 statusCode = HttpStatusCode.Unauthorized; 
                 message = "You are not authorized to do this.";
                }
                else if(ex is KeyNotFoundException)
                {
                    statusCode = HttpStatusCode.NotFound;
                    message = ex.Message;
                }else if(ex is TimeoutException)
                {
                    statusCode = HttpStatusCode.GatewayTimeout;
                    message = ex.Message;  
                }

                context.Response.StatusCode = (int)statusCode;

                var response = new CustomResponse<object>
                {
                    Success = false,
                    Message = "A server error occurred.",
                    Errors = new List<string> { ex.Message }
                };

                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                await context.Response.WriteAsync(json);
            }
        }
    }
}