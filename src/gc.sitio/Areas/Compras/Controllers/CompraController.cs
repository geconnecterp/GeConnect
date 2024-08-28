
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
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

        public CompraController(ILogger<CompraController> logger, IOptions<AppSettings> options, IProductoServicio productoServicio,ICuentaServicio cuentaServicio,
            IHttpContextAccessor context) : base(options, context)
        {
            _logger = logger;
            _appSettings = options.Value;
            _productoServicio = productoServicio;
            _cuentaServicio = cuentaServicio;
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
			//VerificaAutenticacion();
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			return View("RPRNuevaAutorizacion");
        }


        public async Task<JsonResult> BuscarCuentaComercial(string cuenta, char tipo, string vista)
        {
            try
            {
                if (string.IsNullOrEmpty(cuenta) || string.IsNullOrWhiteSpace(cuenta))
                {
                    throw new NegocioException("Debe enviar codigo de cuenta");
                }
                var lista = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
                if (lista.Count == 0)
                {
					throw new NegocioException("No se obtuvierion resultados");
				}
                if (lista.Count == 1)
                {
                    //Buscar tipos de comprobantes por cuenta
                    //Metodo ACAESTAELMETODO
                    var tiposCompte=await _tiposComprobantesServicio.BuscarTiposComptesPorCuenta(cuenta, TokenCookie);
                    return Json(new { error = false, warn = false, unico = true, cuenta = lista[0], tiposCompte });
                }
                return Json(new { error = false, warn = false, unico = false, cuenta = lista });
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

        #region Métodos privados
        private GridCore<RPRAutoComptesPendientesDto> ObtenerAutorizacionPendienteGrid(List<RPRAutoComptesPendientesDto> pendientes)
        {

            var lista = new StaticPagedList<RPRAutoComptesPendientesDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<RPRAutoComptesPendientesDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }
        #endregion
    }
}
