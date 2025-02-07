using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddLog4Net("log4net.config", watch: true);
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<MenuSettings>(builder.Configuration.GetSection("MenuSettings"));
builder.Services.Configure<BusquedaProducto>(builder.Configuration.GetSection("BusquedaProducto"));
builder.Services.Configure<GecoNetConfig>(builder.Configuration.GetSection("GecoNetConfig"));


var cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";
cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.Configure<CookieAuthenticationOptions>(opt => {
    opt.LoginPath = new PathString("/seguridad/token/login");
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
    {
        opt.Cookie.Name = "GCPocketCookie";
        opt.LoginPath = new PathString("/seguridad/token/login");
        opt.LogoutPath = new PathString("/seguridad/token/logout");
        opt.AccessDeniedPath = new PathString("/seguridad/token/login");  //aca debere generar la ruta para indicar el acceso denegado y volver al login
    });
builder.Services.AddServicios();



builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHsts(opt =>
{
    opt.Preload = true;
    opt.IncludeSubDomains = true;
    opt.MaxAge=TimeSpan.FromDays(1);
});

builder.Services.AddSession(opt =>
{
    opt.Cookie.Name = ".gcpocket.session";
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

//estas dos llamadas permite establecer la prioridad de HttpContext.User  y ejecutar la autorización para las solicitudes
//quien sos??
app.UseAuthentication();
//se te permite algo??? estas autorizado?
app.UseAuthorization();

app.UseEndpoints(endpoints => {
    _ = endpoints.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=index}/{id?}");
    _ = endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
