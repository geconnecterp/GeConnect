using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace gc.sitio.Models.Filters
{
    public class AuthenticationCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectResult("/seguridad/Token/Login");
                return;
            }

            var admClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"));
            if (admClaim == null || string.IsNullOrEmpty(admClaim.Value))
            {
                context.Result = new RedirectResult("/seguridad/Token/Login");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
