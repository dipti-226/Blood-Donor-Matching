using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BloodDonor.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // Client errors raised by the server itself (for example a request body over the size limit)
            // keep their real status code. They are not server faults.
            if (exception is BadHttpRequestException badRequest)
            {
                var clientProblem = new ProblemDetails
                {
                    Status = badRequest.StatusCode,
                    Title = "The request could not be processed.",
                    Instance = httpContext.Request.Path
                };

                httpContext.Response.StatusCode = clientProblem.Status.Value;

                await httpContext.Response.WriteAsJsonAsync(clientProblem, cancellationToken);

                return true;
            }

            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = "Please contact support if the problem persists.",
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}