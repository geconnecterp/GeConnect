using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace gc.caja.Models.Filters
{
    public class AuthenticationCheckAttribute : ActionFilterAttribute
    {
        private readonly string _pathBase;

        public AuthenticationCheckAttribute(IOptions<AppSettings> options)
        {
            _pathBase = options.Value.PathBase ?? string.Empty;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                var loginPath = $"{_pathBase}/seguridad/Token/Login";
                context.Result = new RedirectResult(loginPath);
                return;
            }

            var admClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"));
            if (admClaim == null || string.IsNullOrEmpty(admClaim.Value))
            {
                var loginPath = $"{_pathBase}/seguridad/Token/Login";
                context.Result = new RedirectResult(loginPath);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
