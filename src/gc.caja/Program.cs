using gc.caja.Models.Middleware;
using gc.sitio.core.Extensions;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;


var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLog4Net("log4net.config", watch: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//builder.Services.Configure<CajaSettings>(builder.Configuration.GetSection("CajaSettings"));


var cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Obtener PathBase desde configuración
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
var pathBase = appSettings?.PathBase ?? string.Empty;

builder.Services.Configure<CookieAuthenticationOptions>(opt =>
{
    opt.LoginPath = new PathString($"{pathBase}/seguridad/token/login");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
{
    opt.Cookie.Name = "GCCajaCookie";
    opt.LoginPath = new PathString($"{pathBase}/seguridad/token/login");
    opt.LogoutPath = new PathString($"{pathBase}/seguridad/token/logout");
    opt.AccessDeniedPath = new PathString($"{pathBase}/seguridad/token/login");  //aca debere generar la ruta para indicar el acceso denegado y volver al login    
});

// ✅ AGREGADO: Política de autorización por defecto
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddServicios();

//// ✅ AGREGADO: Configurar política CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowEmailClients",
//        policy =>
//        {
//            policy.AllowAnyOrigin()  // Permitir cualquier origen (para clientes de email)
//                  .AllowAnyMethod()  // Permitir GET, POST, etc.
//                  .AllowAnyHeader(); // Permitir cualquier header
//        });
//});

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHsts(opt =>
{
    opt.Preload = true;
    opt.IncludeSubDomains = true;
    opt.MaxAge = TimeSpan.FromDays(1);
});

builder.Services.AddSession(opt =>
{
    opt.Cookie.Name = ".gccaja.session";
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});
// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    //options.Filters.Add<AuthenticationCheckAttribute>();
});

builder.Services.AddMvc();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseSession();

//estas dos llamadas permite establecer la prioridad de HttpContext.User  y ejecutar la autorización para las solicitudes
//quien sos??
app.UseAuthentication();
//se te permite algo??? estas autorizado?
app.UseAuthorization();

// En Program.cs, después de app.UseAuthentication() y app.UseAuthorization()
app.UseSessionExpirationCheck(); // Agregar antes de app.UseMiddleware<AuthenticationCheckMiddleware>();
app.UseMiddleware<AuthenticationCheckMiddleware>();


app.UseEndpoints(endpoints =>
{
    //_ = endpoints.MapControllerRoute(
    //    name: "docmanager",
    //    pattern: "docmanager/{parametros?}",
    //    defaults: new { controller = "DocMg", action = "Index" });

    _ = endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=index}/{id?}");

    _ = endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
