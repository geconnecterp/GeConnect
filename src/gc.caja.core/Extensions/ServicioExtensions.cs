using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.core.Servicios.Implementacion.Cajas;
using gc.caja.core.Servicios.Implementacion.Seguridad;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
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
           
            services.AddScoped<ICajaServicio, CajaServicio>();
            services.AddScoped<ICajaInitServicio, CajaInitServicio>();
            services.AddScoped<IProductoFactServicio, ProductoFactServicio>();
            services.AddSingleton<IReportesConfigService, ReportesConfigService>();
            services.AddScoped<IReportesService, ReportesService>();
            services.AddScoped<IBackupProductosServicio, BackupProductosServicio>();
            services.AddScoped<ICheckoutServicio, CheckoutServicio>();
            services.AddScoped<IFactDiferidaServicio, FactDiferidaServicio>();
            services.AddScoped<ICtaCteServicio, CtaCteServicio>();
            services.AddScoped<INotaCreditoServicio, NotaCreditoServicio>();

            return services;
        }
    }
}
