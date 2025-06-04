using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Host.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate requestDelegate, IHostEnvironment hostEnvironment, ILogger<ExceptionMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _hostEnvironment = hostEnvironment;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _requestDelegate(context);
            }
            catch (ApiException ex)
            {
                var problemDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = ex.StatusCode,
                    Title = ex.ErrorCode,
                    Instance = context.Request.Path
                };

                switch (ex.StatusCode)
                {
                    case 400: // Bad Request
                    case 401: // Unauthorized
                    case 403: // Forbidden
                    case 404: // Not Found
                    case 409: // Conflict
                    case 422: // Unprocessable Entity
                        await Results.Json(problemDetails, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }).ExecuteAsync(context);
                        break;

                    default:
                        problemDetails.Title = "Unhandled ApiException"; // Optional fallback
                        await Results.Json(problemDetails, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }).ExecuteAsync(context);
                        break;
                }
            }
        }
    }
}
