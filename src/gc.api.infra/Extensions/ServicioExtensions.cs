using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.ABM;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Contratos.Servicios.Contable;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.api.core.Servicios;
using gc.api.core.Servicios.ABM;
using gc.api.core.Servicios.Asientos;
using gc.api.core.Servicios.Contable;
using gc.api.core.Servicios.Libros;
using gc.api.core.Servicios.Reportes;
using gc.api.core.Servicios.Tipos;
using gc.api.infra.Datos.Contratos;
using gc.api.infra.Datos.Contratos.Security;
using gc.api.infra.Datos.Implementacion;
using gc.api.infra.Datos.Implementacion.Security;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.infraestructura.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IApiUsuarioServicio, ApiUsuarioServicio>();
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
            services.AddScoped<ITipoRetGanServicio, TipoRetGanServicio>();
            services.AddScoped<ITipoRetIbServicio, TipoRetIbServicio>();
            services.AddScoped<IABMSectorServicio, ABMSectorServicio>();
            services.AddScoped<ISectorServicio, SectorServicio>();
            services.AddScoped<IABMMedioDePagoServicio, ABMMedioDePagoServicio>();
            services.AddScoped<ITipoCuentaFinServicio, TipoCuentaFinServicio>();
            services.AddScoped<IMedioDePagoServicio, MedioDePagoServicio>();
            services.AddScoped<ITipoMonedaServicio, TipoMonedaServicio>();
            services.AddScoped<IConsultaServicio, ConsultaServicio>();
            services.AddScoped<IABMBancoServicio, ABMBancoServicio>();
            services.AddScoped<IBancoServicio, BancoServicio>();
            services.AddScoped<ITipoCuentaGastoServicio, TipoCuentaGastoServicio>();
            services.AddScoped<IABMCuentaDirectaServicio, ABMCuentaDirectaServicio>();
            services.AddScoped<ICuentaGastoServicio, CuentaGastoServicio>();
            services.AddScoped<IOrdenDeCompraEstadoServicio, OrdenDeCompraEstadoServicio>();
            services.AddScoped<IABMVendedorServicio, ABMVendedorServicio>();
            services.AddScoped<ITipoTributoServicio, TipoTributoServicio>();
            services.AddScoped<IABMRepartidorServicio, ABMRepartidorServicio>();
            services.AddScoped<IABMPlanCuentaServicio, ABMPlanCuentaServicio>();
            services.AddScoped<ITipoDtoValorizaRprServicio, TipoDtoValorizaRprServicio>();
            /// Servicios de Asientos
            services.AddScoped<IAsientoServicio, AsientoServicio>();
            services.AddScoped<IAsientoTemporalServicio, AsientoTemporalServicio>();
            services.AddScoped<IAsientoDefinitivoServicio, AsientoDefinitivoServicio>();

            /// Servicios de Libros
            services.AddScoped<IApiLMayorServicio, ApiLMayorServicio>();
            services.AddScoped<IApiLDiarioServicio, ApiLDiarioServicio>();
            services.AddScoped<IApiSumaSaldoServicio, ApiSumaSaldoServicio>();
            //De Reportes

            services.AddScoped<IReportService, ReportService>();
			services.AddScoped<IOrdenDePagoServicio, OrdenDePagoServicio>();
			//De Reportes

			services.AddScoped<IReportService, ReportService>();
            //services.AddScoped<IGeneradorReporte, R001_InformeCuentaCorriente>();
            services.AddLogging();
            

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IServicio<>), typeof(Servicio<>));
            services.AddScoped(typeof(IExceptionManager), typeof(ExceptionManager));
            services.AddScoped(typeof(ILoggerHelper), typeof(LoggerHelper));


            return services;
        }
    }
}
