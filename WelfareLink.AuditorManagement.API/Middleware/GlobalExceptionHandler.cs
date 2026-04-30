using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WelfareLink.AuditorManagement.API.Exceptions;

namespace WelfareLink.AuditorManagement.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = httpContext.TraceIdentifier;
            _logger.LogError(
                exception,
                "An error has occurred while processing the request. TraceId {TraceId}",
                traceId
            );

            int statusCode = 0;
            switch (exception)
            {
                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    break;
                case BadRequestException:
                    statusCode = StatusCodes.Status400BadRequest;
                    break; 
                case BusinessValidationException:
                    statusCode = StatusCodes.Status422UnprocessableEntity;
                    break;
                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Extensions = { ["traceId"] = httpContext.TraceIdentifier }
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                problemDetails.Detail = "An unexpected error occurred. Please contact support.";
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }

        private static string GetTitle(int statusCode)
        {
            string title = string.Empty;
            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    title = "Bad Request";
                    break;
                case StatusCodes.Status404NotFound:
                    title = "Resource not found";
                    break;
                case StatusCodes.Status422UnprocessableEntity: // ADDED MISSING TITLE HERE
                    title = "Business Validation Failed";
                    break;
                case StatusCodes.Status500InternalServerError:
                    title = "Internal Server Error";
                    break;
                default:
                    title = "An error occurred";
                    break;
            }
            return title;
        }
    }
}