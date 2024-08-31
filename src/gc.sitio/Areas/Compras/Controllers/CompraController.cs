
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

//using System.Data;
using System.Reflection;
using X.PagedList;

namespace gc.sitio.Areas.Compras.Controllers
{
    [Area("Compras")]
    public class CompraController : ControladorBase
    {
        private readonly ICuentaServicio _cuentaServicio;
        private readonly ITipoComprobanteServicio _tiposComprobantesServicio;
        private readonly AppSettings _appSettings;
        private readonly ILogger<CompraController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly IDepositoServicio _depositoServicio;

        public CompraController(ILogger<CompraController> logger, IOptions<AppSettings> options, IProductoServicio productoServicio, ICuentaServicio cuentaServicio,
            ITipoComprobanteServicio tipoComprobanteServicio, IDepositoServicio depositoServicio, IHttpContextAccessor context) : base(options, context)
        {
            _logger = logger;
            _appSettings = options.Value;
            _productoServicio = productoServicio;
            _cuentaServicio = cuentaServicio;
            _tiposComprobantesServicio = tipoComprobanteServicio;
            _depositoServicio = depositoServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RPRAutorizacionesLista()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            List<RPRAutoComptesPendientesDto> pendientes;
            GridCore<RPRAutoComptesPendientesDto> grid;
            try
            {
                pendientes = await _productoServicio.RPRObtenerComptesPendiente(AdministracionId, TokenCookie);
                //resguardo lista de autorizaciones pendientes 
                //Veo que en  gc.pocket.site.Areas.PocketPpal.Controllers.RPRController.Index() almacena en una variable de sesión los datos.
                //Consultar si tengo que hacer lo mismo
                grid = ObtenerAutorizacionPendienteGrid(pendientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                grid = new();
            }
            return View(grid);
        }

        public async Task<IActionResult> NuevaAut(string rp)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            //var model = new RPRNuevaAutorizaciónDto() { ComboDeposito = ComboDepositos() };
            var model = new BuscarCuentaDto() { ComboDeposito = ComboDepositos() };
            //return PartialView("RPRNuevaAutorizacion", model);

            return PartialView("RPRNuevaAutorizacion", model);
        }


        public async Task<JsonResult> BuscarCuentaComercial(string cuenta, char tipo, string vista)
        {
            List<CuentaDto> Lista = new();
            try
            {
                if (string.IsNullOrEmpty(cuenta) || string.IsNullOrWhiteSpace(cuenta))
                {
                    throw new NegocioException("Debe enviar codigo de cuenta");
                }
                if (CuentaComercialSeleccionada == null)
                {
                    Lista = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
                }
                else
                {
                    Lista.Add(CuentaComercialSeleccionada);
                }
                if (Lista.Count == 0)
                {
                    throw new NegocioException("No se obtuvierion resultados");
                }
                if (Lista.Count == 1)
                {
                    CuentaComercialSeleccionada = Lista[0];
                    return Json(new { error = false, warn = false, unico = true, cuenta = Lista[0] });
                }
                return Json(new { error = false, warn = false, unico = false, cuenta = Lista });
            }
            catch (NegocioException neg)
            {
                return Json(new { error = false, warn = true, msg = neg.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name} cuenta: {cuenta} tipo: {tipo}");
                return Json(new { error = true, msg = "Algo no fue bien al buscar la cuenta comercial, intente nuevamente mas tarde." });
            }
        }

        [HttpPost]
        public ActionResult ComboTiposComptes(string cuenta)
        {
            var adms = _tiposComprobantesServicio.BuscarTiposComptesPorCuenta(cuenta, TokenCookie).GetAwaiter().GetResult();
            var lista = adms.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
            var TiposComptes = HelperMvc<ComboGenDto>.ListaGenerica(lista);
            return PartialView("~/Areas/ControlComun/Views/CuentaComercial/_ctrComboTipoCompte.cshtml", TiposComptes);
        }

        [HttpPost]
        public ActionResult CargarComprobantesDeRP(string tipo, string tipoDescripcion, string nroComprobante, string fecha, string importe)
        {
            GridCore<RPRComptesDeRPDto> datosIP;
            var lista = new List<RPRComptesDeRPDto>();
            if (tipo.IsNullOrEmpty())
            {
                datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
            }
            else if (RPRComptesDeRPRegs.Exists(x => x.Tipo == tipo && x.NroComprobante == nroComprobante))
            {
                datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
            }
            else
            {
                lista = RPRComptesDeRPRegs;
                var nuevo = new RPRComptesDeRPDto() 
                { 
                    Tipo = tipo, 
                    TipoDescripcion = tipoDescripcion, 
                    NroComprobante = nroComprobante, 
                    Fecha = Convert.ToDateTime(fecha), 
                    Importe = importe 
                };
                lista.Add(nuevo);
                RPRComptesDeRPRegs = lista;
                datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
            }
            return PartialView("_rprComprobantesDeRP", datosIP);
        }

        #region Métodos privados
        private GridCore<RPRComptesDeRPDto> ObtenerComprobantesDeRPGrid(List<RPRComptesDeRPDto> listaComptesDeRP)
        {

            var lista = new StaticPagedList<RPRComptesDeRPDto>(listaComptesDeRP, 1, 999, listaComptesDeRP.Count);

            return new GridCore<RPRComptesDeRPDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
        }

        private GridCore<RPRAutoComptesPendientesDto> ObtenerAutorizacionPendienteGrid(List<RPRAutoComptesPendientesDto> pendientes)
        {

            var lista = new StaticPagedList<RPRAutoComptesPendientesDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<RPRAutoComptesPendientesDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }


        private SelectList ComboDepositos()
        {
            var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }
        #endregion
    }
}
