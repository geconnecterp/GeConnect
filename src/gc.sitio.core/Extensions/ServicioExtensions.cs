using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Users;
using gc.sitio.core.Servicios.Implementacion;
using gc.sitio.core.Servicios.Implementacion.ABM;
using gc.sitio.core.Servicios.Implementacion.DocManager;
using gc.sitio.core.Servicios.Implementacion.Users;
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
            services.AddScoped<IAdministracionServicio, AdministracionServicio>();
            services.AddScoped<ITipoDocumentoServicio, TipoDocumentoServicio>();
            services.AddScoped<ITipoComprobanteServicio, TipoComprobanteServicio>();
            services.AddScoped<ITipoNegocioServicio, TipoNegocioServicio>();
            services.AddScoped<IZonaServicio, ZonaServicio>();
            services.AddScoped<ICuentaServicio, CuentaServicio>();
            services.AddScoped<IProveedorServicio, ProveedorServicio>();
            services.AddScoped<IRubroServicio, RubroServicio>();
            services.AddScoped<IProductoServicio, ProductoServicio>();
            services.AddScoped<IProducto2Servicio, Producto2Servicio>();
            services.AddScoped<IDepositoServicio, DepositoServicio>();
            services.AddScoped<IRemitoServicio, RemitoServicio>();
            services.AddScoped<IABMProductoServicio, ABMProductoServicio>();
			services.AddScoped<IABMClienteServicio, ABMClienteServicio>();
			services.AddScoped<IABMProveedorServicio, ABMProveedorServicio>();
			services.AddScoped<ICondicionAfipServicio, CondicionAfipServicio>();
			services.AddScoped<ICondicionIBServicio, CondicionIBServicio>();
			services.AddScoped<IDepartamentoServicio, DepartamentoServicio>();
			services.AddScoped<IFormaDePagoServicio, FormaDePagoServicio>();
			services.AddScoped<INaturalezaJuridicaServicio, NaturalezaJuridicaServicio>();
			services.AddScoped<IProvinciaServicio, ProvinciaServicio>();
			services.AddScoped<ITipoCanalServicio, TipoCanalServicio>();
			services.AddScoped<ITipoCuentaBcoServicio, TipoCuentaBcoServicio>();
			services.AddScoped<IListaDePrecioServicio, ListaDePrecioServicio>();
			services.AddScoped<IVendedorServicio, VendedorServicio>();
			services.AddScoped<IRepartidorServicio, RepartidorServicio>();
			services.AddScoped<IFinancieroServicio, FinancieroServicio>();
			services.AddScoped<ITipoContactoServicio, TipoContactoServicio>();
            services.AddScoped<ITipoObsServicio, TipoObsServicio>();
			services.AddScoped<ITipoOpeIvaServicio, TipoOpeIvaServicio>();
			services.AddScoped<ITipoProveedorServicio, TipoProveedorServicio>();
			services.AddScoped<ITipoGastoServicio, TipoGastoServicio>();
			services.AddScoped<ITipoRetGanServicio, TipoRetGanServicio>();
			services.AddScoped<ITipoRetIbServicio, TipoRetIbServicio>();
			services.AddScoped<IAbmServicio, AbmServicio>();
			services.AddScoped<IMenuesServicio, MenuesServicio>();
            services.AddScoped<IABMSectorServicio, ABMSectorServicio>();
			services.AddScoped<ISectorServicio, SectorServicio>();
			services.AddScoped<IABMMedioDePagoServicio, ABMMedioDePagoServicio>();
			services.AddScoped<ITipoCuentaFinServicio, TipoCuentaFinServicio>();
			services.AddScoped<IMedioDePagoServicio, MedioDePagoServicio>();
			services.AddScoped<ITipoMonedaServicio, TipoMonedaServicio>();
			services.AddScoped<IUserServicio, UserServicio>();
			services.AddScoped<IConsultasServicio, ConsultasServicio>();
			services.AddScoped<IABMBancoServicio, ABMBancoServicio>();
			services.AddScoped<IBancoServicio, BancoServicio>();
			services.AddScoped<ITipoCuentaGastoServicio, TipoCuentaGastoServicio>();
			services.AddScoped<IABMCuentaDirectaServicio, ABMCuentaDirectaServicio>();
			services.AddScoped<ICuentaGastoServicio, CuentaGastoServicio>();
			services.AddScoped<IDocManagerServicio, DocManagerServicio>();
			services.AddScoped<IOrdenDeCompraEstadoServicio, OrdenDeCompraEstadoServicio>();
			services.AddScoped<ITipoTributoServicio, TipoTributoServicio>();
			services.AddScoped<IABMVendedorServicio, ABMVendedorServicio>();
			services.AddScoped<IABMRepartidorServicio, ABMRepartidorServicio>();
			services.AddScoped<IABMZonaServicio, ABMZonaServicio>();
			services.AddScoped<IABMPlanCuentaServicio, ABMPlanCuentaServicio>();
			services.AddScoped<ITipoDtoValorizaRprServicio, TipoDtoValorizaRprServicio>();

			return services;
        }
    }
}
