using gc.sitio.core.Extensions;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Models.Middleware;
using gc.sitio.Models.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddLog4Net("log4net.config", watch: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
//extraigo del appsettings.js las configuraciones de cada modulo para el Gestor Documental
builder.Services.Configure<DocsManager>(builder.Configuration.GetSection("DocsManager"));
builder.Services.Configure<EmpresaGeco>(builder.Configuration.GetSection("EmpresaGeco"));
builder.Services.Configure<ImportacionExtracto>(builder.Configuration.GetSection("ImportacionExtracto"));
builder.Services.Configure<ImportacionLiquidacionDeEmpleado>(builder.Configuration.GetSection("ImportacionLiquidacionDeEmpleado"));

var cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Obtener PathBase desde configuración
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>();
var pathBase = appSettings?.PathBase ?? string.Empty;
var sessionDurationMinutes = builder.Configuration.GetValue<int?>("SessionSettings:DurationMinutes") ?? 240;

builder.Services.Configure<CookieAuthenticationOptions>(opt =>
{
    opt.LoginPath = new PathString($"{pathBase}/seguridad/token/login");
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
{
	options.MaxModelBindingCollectionSize = int.MaxValue;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
 {
     opt.Cookie.Name = "GCSitioCookie";
     opt.LoginPath = new PathString($"{pathBase}/seguridad/token/login");
     opt.LogoutPath = new PathString($"{pathBase}/seguridad/token/logout");
     opt.AccessDeniedPath = new PathString($"{pathBase}/seguridad/token/login");  //aca debere generar la ruta para indicar el acceso denegado y volver al login    
     opt.ExpireTimeSpan = TimeSpan.FromMinutes(sessionDurationMinutes);
     opt.SlidingExpiration = true;
 });


builder.Services.AddServicios();

// ✅ AGREGADO: Configurar política CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowEmailClients",
        policy =>
        {
            policy.AllowAnyOrigin()  // Permitir cualquier origen (para clientes de email)
                  .AllowAnyMethod()  // Permitir GET, POST, etc.
                  .AllowAnyHeader(); // Permitir cualquier header
        });
});

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
    opt.Cookie.Name = ".gcsite.session";
    opt.IdleTimeout = TimeSpan.FromMinutes(sessionDurationMinutes);
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
    _ = endpoints.MapControllerRoute(
        name: "docmanager",
        pattern: "docmanager/{parametros?}",
        defaults: new { controller = "DocMg", action = "Index" });

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
