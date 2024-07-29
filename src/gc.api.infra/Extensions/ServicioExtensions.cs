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
using gc.api.infra.Datos.Contratos;
using Microsoft.AspNetCore.Http;

namespace gc.api.infra.Extensions
{
    public static class ServicioExtensions
    {
        public static IServiceCollection AddServicios(this IServiceCollection services)
        {
            //services.AddTransient<IConfigServicio, ConfigServicio>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<ISecurityServicio, SecurityServicio>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IUsuarioServicio, UsuarioServicio>();
            services.AddScoped<IDataConnectionContext, DataConnectionContext>();
            services.AddScoped<IAdministracionServicio, AdministracionesServicio>();
            services.AddScoped<IBilleteraOrdenServicio, BilleteraOrdenServicio>();
            services.AddScoped<IBilleteraServicio, BilleteraServicio>();
            services.AddScoped<IBOrdenEstadoServicio, BOrdenEstadoServicio>();
            services.AddScoped<ICajaServicio, CajaServicio>();
            services.AddScoped<IProveedorServicio, ProveedorServicio>();
            services.AddScoped<IProductoServicio, ProductoServicio>();
            services.AddScoped<ICuentaServicio, CuentaServicio>();


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IServicio<>), typeof(Servicio<>));
            services.AddScoped(typeof(IExceptionManager), typeof(ExceptionManager));
            services.AddScoped(typeof(ILoggerHelper), typeof(LoggerHelper));


            return services;
        }
    }
}
