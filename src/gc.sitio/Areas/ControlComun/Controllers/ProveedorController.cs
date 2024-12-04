using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("ControlComun")]
    public class ProveedorController : ControladorBase
    {
        private readonly ILogger<ProveedorController> _logger;
        private readonly AppSettings _appSettings;
        private readonly IProveedorServicio _provSv;
		private readonly ICuentaServicio _ctaSv;

		public ProveedorController(ICuentaServicio cuentaServicio, IProveedorServicio proveedoresServicio, ILogger<ProveedorController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
        {
            _appSettings = options.Value;
			_ctaSv = cuentaServicio;
			_logger = logger;
            _provSv = proveedoresServicio;
        }

        // GET: proveedoresController
        public async Task<IActionResult> Index(string buscar, string sortdir = "ASC", string sort = "cta_id", int page = 1)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<ProveedorDto> grillaDatos;
            try
            {
                string token = TokenCookie;
                grillaDatos = await ObtenerProveedorAsync(buscar, sortdir, sort, page, token);
                ViewData["Title"] = "Listado de proveedoress";
                return View(response);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "", controller = "Home", action = "Index" }));

        }

        private async Task<GridCore<ProveedorDto>> ObtenerProveedorAsync(string buscar, string sortdir, string sort, int page, string token)
        {
            var proveedoress = await _provSv.BuscarAsync(new QueryFilters { Buscar = buscar, Pagina = page, Sort = sort, SortDir = sortdir }, token);
            List<ProveedorDto> entidades = proveedoress.Item1;
            var lista = new StaticPagedList<ProveedorDto>(entidades, proveedoress.Item2.CurrentPage, proveedoress.Item2.PageSize, proveedoress.Item2.TotalCount);


            return new GridCore<ProveedorDto>() { ListaDatos = lista, CantidadReg = _appSettings.LimiteAvalancha, PaginaActual = page, CantidadPaginas = _appSettings.LimiteAvalancha / 20, Sort = sort, SortDir = sortdir.Equals("ASC") ? "DESC" : "ASC" };
        }

        // GET: proveedoresController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            try
            {
                var proveedores = await _provSv.BuscarAsync(id, TokenCookie);
                if (proveedores == null)
                {
                    TempData["warn"] = "El proveedores buscado no se encontró.";
                }
                return View(proveedores);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
        }

        // GET: proveedoresController/Create
        public async Task<ActionResult> Create()
        {
            return View(new ProveedorDto());
        }

        // POST: proveedoresController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorDto datos)
        {
            try
            {
                if (datos is null)
                {
                    throw new ArgumentNullException(nameof(datos));
                }
                if (!ModelState.IsValid)
                {
                    return View(datos);
                }
                else
                {
                    var res = await _provSv.AgregarAsync(datos, TokenCookie);
                    if (res)
                    {
                        TempData["info"] = "Se agrego satisfactoriamente el proveedores";
                        return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
                    }
                    else
                    {
                        TempData["warn"] = $"No se pudo agregar el proveedores. Si el problema persiste avise al administrador del sistema.";
                    }
                }
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }

            return View(datos);
        }

        // GET: proveedoresController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            try
            {
                var proveedores = await _provSv.BuscarAsync(id, TokenCookie);
                if (proveedores == null)
                {
                    TempData["warn"] = "El proveedores buscado no se encontró.";
                }
                return View(proveedores);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
        }

        // POST: proveedoresController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedorDto datos)
        {
            try
            {
                if (datos is null)
                {
                    throw new ArgumentNullException(nameof(datos));
                }
                if (!ModelState.IsValid)
                {
                    return View(datos);
                }
                else
                {
                    var res = await _provSv.ActualizarAsync(datos.Cta_Id, datos, TokenCookie);
                    if (res)
                    {
                        TempData["info"] = "Se Actualizo satisfactoriamente el proveedores";
                        return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
                    }
                    else
                    {
                        TempData["warn"] = $"No se pudo actualizar el proveedores. Si el problema persiste avise al administrador del sistema.";
                    }
                }
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }

            return View(datos);
        }

        // GET: proveedoresController/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                var proveedores = await _provSv.BuscarAsync(id, TokenCookie);
                if (proveedores == null)
                {
                    TempData["warn"] = "El proveedores buscado no se encontró.";
                }
                return View(proveedores);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
        }

        // POST: proveedoresController/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePpal(string id)
        {
            try
            {
                var res = await _provSv.EliminarAsync(id, TokenCookie);
                if (res)
                {
                    TempData["info"] = "Se Eliminó satisfactoriamente el proveedores";
                }
                else
                {
                    TempData["warn"] = $"No se pudo eliminar el proveedores. Si el problema persiste avise al administrador del sistema.";
                }
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogError(ex, "Error de Autenticación");
                return RedirectToAction("Login", new RouteValueDictionary(new { area = "Seguridad", controller = "Token", action = "Login" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error No Controlado");
                TempData["error"] = ex.Message;
            }
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "ESPECIFICAR", controller = "proveedores", action = "Index" }));
        }

        [HttpPost]
        [Route("BuscarProveedor")]
        public JsonResult Buscar(string prefix)
        {
            //var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
            //var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
            if (ProveedoresLista == null || ProveedoresLista.Count == 0)
            {
				ProveedoresLista = _ctaSv.ObtenerListaProveedores(TokenCookie);
			}
            var nombres = ProveedoresLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            return Json(nombres);
        }
    }
}
