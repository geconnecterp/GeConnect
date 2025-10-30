using DocumentFormat.OpenXml.Spreadsheet;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class PresupuestoController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.PRESUP.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;

        private readonly IAdministracionServicio _admSv;
        private readonly IUserServicio _userSv;
        private readonly IPresupuestoServicio _presuSv;

        public PresupuestoController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
           ILogger<OfertasController> logger, IOptions<DocsManager> docsManager,
           IDocManagerServicio docManagerServicio, IAdministracionServicio admSv,
           IUserServicio userServicio, IPresupuestoServicio presupuestoServicio) :base(options,contexo,logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _admSv = admSv;
            _userSv = userServicio;
            _presuSv = presupuestoServicio;
        }


        public IActionResult Index()
        {
            string msg = "Error de negocio al cargar la vista de PRESUPUESTOS";
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;


                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                //DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);

                //_logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                ////en este mismo acto se cargan los posibles documentos
                ////que se pueden imprimir, exportar, enviar por email o whatsapp
                //ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion
                InicializaPresupuesto();

                ViewData["Titulo"] = "PRESUPUESTOS";
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = "Hubo un problema al cargar la vista de PROMOS y COMBOS. Si el problema persiste, contacte al administrador.";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPresupuestos(QueryFilters filters)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (filters == null )
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));
                }

                if((filters.Rel01 == null || !filters.Rel01.Any())&& 
                    (filters.Rel04 == null || !filters.Rel04.Any()) &&
                    (filters.Rel02==null || !filters.Rel02.Any()) &&
                    (filters.Rel03== null || !filters.Rel03.Any()))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("Debe seleccionar algún filtro para buscar los Presupuestos"));
                }

                filters.Registros = _configuracion.NroRegistrosPagina;

                filters.Rel01 = filters.Rel01?.Where(x => !string.IsNullOrEmpty(x)).ToList();
                filters.Rel02=filters.Rel02?.Where(x => !string.IsNullOrEmpty(x)).ToList();
                filters.Rel03 = filters.Rel03?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
                filters.Rel04 = filters.Rel04?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
                //debo realizar la busqueda de los presupuestos
                var presup = await _presuSv.BuscarPresupuestos(filters, TokenCookie);

                if (!presup.Ok)
                {
                    throw new NegocioException(presup.Mensaje??"Hubo algun problema en la busqueda de Presupuestos.");
                }

                // Para operar con la lista de Productos Seleccionados
                var lista = presup.ListaEntidad??new List<PresupuestoListDto>();
                MetadataGeneral = presup.Meta;
               
                // Generar grid con productos mapeados
                GridCoreSmart<PresupuestoListDto> grid = GenerarGridPresupuestos(lista, filters.Pagina??1,filters);

                return PartialView("_gridPresupuesto", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al presentar productos múltiples para oferta");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar productos múltiples para oferta");
                return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
            }
        }

        [HttpPost]
        public IActionResult NuevoPresupuesto()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            PresupuestoDto presup = new();

            return PartialView("_presupuestoDatos", presup);
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerPresupuestoDato(string pre_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (pre_id == null)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del presupuesto no fue recepcionado."));
                }

                var pres = await _presuSv.ObtenerPresupuesto(pre_id, TokenCookie);
                if (!pres.Ok)
                {
                    throw new NegocioException(pres.Mensaje ?? "No se ha podido identificar el presupuesto.");
                }
                PresupuestoDto presup = new();
                
                if(pres.ListaEntidad==null || pres.ListaEntidad.Count()==0)
                {
                    throw new NegocioException("No se encontraron los datos del Presupuesto");
                }
                return PartialView("_presupuestoDatos", pres.ListaEntidad[0]);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al presentar productos múltiples para oferta");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar productos múltiples para oferta");
                return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerPresupuestoProducto(string pre_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (string.IsNullOrWhiteSpace(pre_id))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del presupuesto no fue recepcionado."));
                }

                var pres = await _presuSv.ObtenerDetallePresupuesto(pre_id, TokenCookie);
                if (!pres.Ok)
                {
                    throw new NegocioException(pres.Mensaje ?? "No se ha podido obtener el detalle del presupuesto.");
                }

                // Generar grid con productos del presupuesto
                var productos = pres.ListaEntidad ?? new List<PresupuestoProductoDto>();
                var grid = GenerarGridPresupuestoProductos(productos);

                return PartialView("_presupuestoProds", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error al obtener productos del presupuesto");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener productos del presupuesto");
                return PartialView("_gridMensaje", CrearRespuestaError("Error al cargar productos del presupuesto"));
            }
        }

        private GridCoreSmart<PresupuestoListDto> GenerarGridPresupuestos(List<PresupuestoListDto> lista, int page,QueryFilters filtro)
        {
            var presup = lista
                .OrderBy(c => c.pre_id)
                .ToList();

            const int registrosPorPagina = 10;
            var pagedList = new StaticPagedList<PresupuestoListDto>(
                presup,
                page,
                registrosPorPagina,
                presup.Count
            );

            var grid = new GridCoreSmart<PresupuestoListDto>
            {
                ListaDatos = pagedList, //lista de combos
                CantidadReg = presup.Count, //cantidad actual de registros
                PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
                UltimoRegistro = Math.Min(page * registrosPorPagina, presup.Count), //define cual es el ultimo registro
                RegistroFinal = presup.Count, //indica cual es el ultimo registro
                CantidadPaginas = (int)Math.Ceiling((double)presup.Count / registrosPorPagina),//calcula la cantidad de paginas
                PaginaActual = page,//especifica que pagina es la actual
                Sort = filtro.Sort ?? "pre_id",
                SortDir = filtro.SortDir ?? "ASC",
                DatoAux01 = $"Presupuestos cargados: {DateTime.Now:HH:mm:ss}"
            };

            return grid;
        }

        private GridCoreSmart<PresupuestoProductoDto> GenerarGridPresupuestoProductos(List<PresupuestoProductoDto> productos)
        {
            const int registrosPorPagina = 50; // Mayor cantidad para productos
            var ordenados = productos.OrderBy(p => p.p_id).ToList();

            var pagedList = new StaticPagedList<PresupuestoProductoDto>(
                ordenados,
                1,
                registrosPorPagina,
                ordenados.Count
            );

            return new GridCoreSmart<PresupuestoProductoDto>
            {
                ListaDatos = pagedList,
                CantidadReg = ordenados.Count,
                PrimerRegistro = 1,
                UltimoRegistro = ordenados.Count,
                RegistroFinal = ordenados.Count,
                CantidadPaginas = 1,
                PaginaActual = 1,
                Sort = "p_id",
                SortDir = "ASC",
                DatoAux01 = $"Productos: {ordenados.Count} | Total: {ordenados.Sum(x => x.pre_total):N2}"
            };
        }

        private void InicializaPresupuesto()
        {
            //ADMINISTRACIONES que se cargarán en el filtro solamente las activas.
            ObtenerAdministracionesLista(_admSv, "S");
            ObtenerEstadoPresupuesto(_presuSv);

            //CLIENTE
            var listR01 = new List<ComboGenDto>();
            ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
            //USUARIO
            var listR02 = new List<ComboGenDto>();
            ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);
            //ESTADO
            var listR03 = new List<ComboGenDto>();
            var est = EstadosPresupuesto;
            var estCbo = est.Select(x => new ComboGenDto { Id = x.pree_id.ToString(), Descripcion = x.pree_desc }).ToList();
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(estCbo);
            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
            //ADMINISTRACION
            var adm = AdministracionesLista;
            var admins = adm.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre }).ToList();
            ViewBag.Rel04 = HelperMvc<ComboGenDto>.ListaGenerica(admins);

        }
    }
}
