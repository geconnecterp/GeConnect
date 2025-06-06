using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.EntidadesComunes.Options;
using gc.notificacion.api.Modelo.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ClaveSettings>(builder.Configuration.GetSection("ClaveSettings"));
builder.Logging.AddLog4Net("log4net.config", watch: true);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<RegisterMiddleware>();

app.MapControllers();

app.Run();
