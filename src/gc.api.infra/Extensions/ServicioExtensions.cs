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
using gc.api.core.Servicios.ABM;
using gc.api.core.Contratos.Servicios.ABM;

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
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IUsuarioServicio, UsuarioServicio>();
            services.AddScoped<IDataConnectionContext, DataConnectionContext>();
            services.AddScoped<IAdministracionServicio, AdministracionServicio>();
            services.AddScoped<IBilleteraOrdenServicio, BilleteraOrdenServicio>();
            services.AddScoped<IBilleteraServicio, BilleteraServicio>();
            services.AddScoped<IBOrdenEstadoServicio, BOrdenEstadoServicio>();
            services.AddScoped<ICajaServicio, CajaServicio>();
            services.AddScoped<IProveedorServicio, ProveedorServicio>();
            services.AddScoped<IApiProductoServicio, ApiProductoServicio>();
            services.AddScoped<ICuentaServicio, CuentaServicio>();
            services.AddScoped<IRubroServicio, RubroServicio>();
            services.AddScoped<IDepositoServicio, DepositoServicio>();
			services.AddScoped<ITiposComprobanteServicio, TipoComprobanteServicio>();
            services.AddScoped<ITipoNegocioServicio, TipoNegocioServicio>();
			services.AddScoped<ITipoProveedorServicio, TipoProveedorServicio>();
			services.AddScoped<ITipoGastoServicio, TipoGastoServicio>();
			services.AddScoped<IZonaServicio, ZonaServicio>();
            services.AddScoped<IApiAlmacenServicio, ApiAlmacenServicio>();
            services.AddScoped<ITipoMotivoServicio, TipoMotivoServicio>();
			services.AddScoped<ITipoOpeIvaServicio, TipoOpeIvaServicio>();
			services.AddScoped<IProductoDepositoServicio, ProductoDepositoServicio>();
			services.AddScoped<IRemitoServicio, RemitoServicio>();
			services.AddScoped<IABMProductoServicio, ABMProductoServicio>();
			services.AddScoped<IABMClienteServicio, ABMClienteServicio>();
			services.AddScoped<IABMProveedorServicio, ABMProveedorServicio>();
			services.AddScoped<ICondicionAfipServicio, CondicionAfipServicio>();
            services.AddScoped<ICondicionIBServicio, CondicionIBServicio>();
            services.AddScoped<IDepartamentoServicio, DepartamentoServicio>();
            services.AddScoped<INaturalezaJuridicaServicio, NaturalezaJuridicaServicio>();
            services.AddScoped<IProvinciaServicio, ProvinciaServicio>();
            services.AddScoped<ITipoCanalServicio, TipoCanalServicio>();
            services.AddScoped<ITipoCuentaBcoServicio, TipoCuentaBcoServicio>();
            services.AddScoped<IVendedorServicio, VendedorServicio>();
            services.AddScoped<IListaPrecioServicio, ListaPrecioServicio>();
            services.AddScoped<IRepartidorServicio, RepartidorServicio>();
            services.AddScoped<IFinancieroServicio, FinancieroServicio>();
            services.AddScoped<IFormaDePagoServicio, FormaDePagoServicio>();
            services.AddScoped<ITiposDocumentoServicio, TipoDocumentoServicio>();
			services.AddScoped<ITipoContactoServicio, TipoContactoServicio>();
            services.AddScoped<ITipoObsServicio, TipoObsServicio>();
            services.AddScoped<IAbmServicio, AbmServicio>();


            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IServicio<>), typeof(Servicio<>));
            services.AddScoped(typeof(IExceptionManager), typeof(ExceptionManager));
            services.AddScoped(typeof(ILoggerHelper), typeof(LoggerHelper));


            return services;
        }
    }
}
