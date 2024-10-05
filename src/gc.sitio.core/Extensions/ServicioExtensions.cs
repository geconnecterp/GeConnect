using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace gc.sitio.core.Extensions
{
    public static class ServicioExtensions
    {
        public static IServiceCollection AddServicios(this IServiceCollection services)
        {
            services.AddSingleton<ILoggerHelper, LoggerHelper>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IAdministracionServicio,AdministracionServicio>();
            services.AddScoped<ITipoDocumentoServicio, TipoDocumentoServicio>();
			services.AddScoped<ITipoComprobanteServicio, TipoComprobanteServicio>();
			services.AddScoped<ICuentaServicio, CuentaServicio>();
            services.AddScoped<IProveedorServicio, ProveedorServicio>();
            services.AddScoped<IRubroServicio, RubroServicio>();
            services.AddScoped<IProductoServicio, ProductoServicio>();
            services.AddScoped<IDepositoServicio, DepositoServicio>();
			services.AddScoped<IRemitoServicio, RemitoServicio>();
			return services;
        }
    }
}
