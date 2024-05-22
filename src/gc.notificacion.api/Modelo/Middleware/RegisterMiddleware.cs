using Microsoft.AspNetCore.Http.Extensions;

namespace gc.notificacion.api.Modelo.Middleware
{
    public class RegisterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RegisterMiddleware> _logger;
        public RegisterMiddleware(RequestDelegate request, ILogger<RegisterMiddleware> logger)
        {
            _next = request;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Url: {context.Request.GetDisplayUrl()}");
            await _next(context);
        }
    }
}
