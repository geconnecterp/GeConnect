using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class MenuesController : ControladorUsuariosBase
    {
        private readonly AppSettings _settings;
        private readonly ILogger<MenuesController> _logger;
        private readonly IMenuesServicio _mnSrv;

        public MenuesController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<MenuesController> logger, IMenuesServicio menuesServicio) : base(options, accessor, logger)

        {
            _logger = logger;
            _settings = options.Value;
            _mnSrv = menuesServicio;
        }

        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPerfiles(QueryFilters query, bool buscaNew, string sort = "p_desc", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<PerfilDto> lista;
            MetadataGrid metadata;
            GridCore<PerfilDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaActual == pag && !buscaNew && PerfilesBuscados.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = PerfilesBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    PerfilesBuscados = lista;
                }
                else
                {
                    PaginaActual = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _mnSrv.GetPerfiles (query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataPerfil = res.Item2 ?? null;
                    PerfilesBuscados = lista;
                }
                metadata = MetadataPerfil;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(PerfilesBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataPerfil.TotalCount, MetadataPerfil.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridPerfiles", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda de Perfiles";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Perfiles");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }
    }
}

