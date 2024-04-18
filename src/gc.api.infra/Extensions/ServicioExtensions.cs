using Microsoft.Extensions.DependencyInjection;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Services;
using gc.infraestructura.Core.Helpers;
using gc.api.infra.Datos.Implementacion;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.api.infra.Datos.Implementacion.Security;
using gc.api.Core.Servicios;
using gc.api.Core.Interfaces.Servicios;

namespace gc.api.infra.Extensions
{
    public static class ServicioExtensions
    {
        public static IServiceCollection AddServicios(this IServiceCollection services)
        {
            //services.AddTransient<IConfigServicio, ConfigServicio>();
            services.AddScoped<ISecurityServicio, SecurityServicio>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUsuarioServicio, UsuarioServicio>();


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IServicio<>), typeof(Servicio<>));
            services.AddScoped(typeof(IExceptionManager), typeof(ExceptionManager));
            services.AddScoped(typeof(ILoggerHelper), typeof(LoggerHelper));


            return services;
        }
    }
}
