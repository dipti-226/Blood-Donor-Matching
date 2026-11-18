namespace BloodDonor.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly bool _applyContentSecurityPolicy;

        public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
        {
            _next = next;

            // Scalar needs scripts in Development, so the strict policy applies elsewhere only.
            _applyContentSecurityPolicy = !environment.IsDevelopment();
        }

        public Task InvokeAsync(HttpContext context)
        {
            // OnStarting sets the headers when the response begins, so they are present on every
            // response, including errors produced by the exception handler or the rate limiter.
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;

                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "no-referrer";

                // Responses can carry donor contact details, so they must never be cached.
                headers["Cache-Control"] = "no-store";

                if (_applyContentSecurityPolicy)
                {
                    headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                }

                return Task.CompletedTask;
            });

            return _next(context);
        }
    }
}