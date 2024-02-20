using Microsoft.Extensions.DependencyInjection;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Servicios;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Services;
using gc.infraestructura.Core.Helpers;
using gc.api.infra.Datos.Implementacion;

namespace gc.api.infra.Extensions
{
    public static class ServicioExtensions
    {
        public static IServiceCollection AddServicios(this IServiceCollection services)
        {
            //services.AddTransient<IConfigServicio, ConfigServicio>();
            //services.AddScoped<IClienteServicio, ClienteServicio>();
            


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IServicio<>), typeof(Servicio<>));
            services.AddScoped(typeof(IExceptionManager), typeof(ExceptionManager));
            services.AddScoped(typeof(ILoggerHelper), typeof(LoggerHelper));


            return services;
        }
    }
}
