using gc.infraestructura.Core.EntidadesComunes.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
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

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

//estas dos llamadas permite establecer la prioridad de HttpContext.User  y ejecutar la autorización para las solicitudes
//quien sos??
app.UseAuthentication();
//se te permite algo??? estas autorizado?
app.UseAuthorization();




app.UseEndpoints(endpoints => {
    _ = endpoints.MapControllerRoute(name: "areas", pattern: "{area:exists}/{controller=Home}/{action=index}/{id?}");
    _ = endpoints.MapControllerRoute(name:"default",pattern: "{controller=Home}/{action=Index}/{id?}");
} );

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
