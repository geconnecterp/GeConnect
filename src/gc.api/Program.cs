using gc.api.infra.Datos;
using gc.api.infra.Filtros;
using gc.api.infra.Extensions;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.EntityFrameworkCore;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using FluentValidation.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Agregamos el AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Activamos el Filtro de Exception General
builder.Services.AddControllers(opt => { opt.Filters.Add<GlobalExceptionFilter>(); })
    .AddNewtonsoftJson(opt=> {
        opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });

builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
builder.Services.Configure<PasswordOptions>(builder.Configuration.GetSection("PasswordOptions"));
builder.Services.Configure<ConfigNegocioOption>(builder.Configuration.GetSection("ConfigNegocio"));

string? conn = builder.Configuration.GetConnectionString("GeConnectKey");
if (!string.IsNullOrEmpty(conn))
{
    conn = conn.Replace(@"\\", @"\");
    builder.Services.AddDbContext<GeConnectContext>(opt => opt.UseSqlServer(conn));
}

builder.Services.AddServicios();

builder.Services.AddSingleton<IUriService>(provider =>
{
    var accesor = provider.GetRequiredService<IHttpContextAccessor>();
    HttpRequest? request = null;
    string? absoluteUri = null;
    if (accesor.HttpContext != null)
    {
        request = accesor.HttpContext.Request;
        absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent());
        return new UriService(absoluteUri);
    }
    else
    {
        return new UriService("");
    }


});

//Configuración del JWT
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt => {
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        //se va a validar
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,  //definimos q el token tenga un tiempo de vida
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Authentication:Issuer"],   //se valida el emisor
        ValidAudience = builder.Configuration["Authentication:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authentication:SecretKey"])),
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<ValidationFilter>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
