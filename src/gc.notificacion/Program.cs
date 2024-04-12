using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();

// Add services to the container.
builder.Services.AddControllersWithViews();


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

app.UseAuthorization();

app.MapControllerRoute(
    name:"notification",
    pattern:"orden/{ordenId}/mp",
    defaults: new {controller="Notificacion",action= "Notify" }
    );
app.MapControllerRoute(
    name: "notification",
    pattern: "notifymp/{ordenId}/mp",
    defaults: new { controller = "Notificacion", action = "NotifyFromMP" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
