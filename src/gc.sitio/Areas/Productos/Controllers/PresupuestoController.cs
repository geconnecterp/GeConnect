using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
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
using Newtonsoft.Json;
using System.Reflection;
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
        private readonly IRubroServicio _rubroServicio;

        public PresupuestoController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
           ILogger<OfertasController> logger, IOptions<DocsManager> docsManager,
           IDocManagerServicio docManagerServicio, IAdministracionServicio admSv,
           IUserServicio userServicio, IRubroServicio rubro, IPresupuestoServicio presupuestoServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;

            // inicializo las variables para manejar el modulo de impresión
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
            _admSv = admSv;
            _userSv = userServicio;
            _presuSv = presupuestoServicio;
            _rubroServicio = rubro;
        }


        public IActionResult Index()
        {
            string msg = "Error de negocio al cargar la vista de PRESUPUESTOS";
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                string titulo = "Presupuesto/Cotización";
                ViewData["Titulo"] = titulo;
                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes[0].Id; //siempre el primer reporte

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                InicializaPresupuesto();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                TempData["error"] = "Hubo un problema al cargar la vista de PRESUPUESTOS. Si el problema persiste, contacte al administrador.";
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

                if (filters == null)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));
                }

                if ((filters.Rel01 == null || !filters.Rel01.Any()) &&
                    (filters.Rel04 == null || !filters.Rel04.Any()) &&
                    (filters.Rel02 == null || !filters.Rel02.Any()) &&
                    (filters.Rel03 == null || !filters.Rel03.Any()) &&
                    (!filters.FechaD.HasValue || !filters.FechaH.HasValue))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("Debe seleccionar algún filtro para buscar los Presupuestos"));
                }

                filters.Registros = _configuracion.NroRegistrosPagina;

                filters.Rel01 = filters.Rel01?.Where(x => !string.IsNullOrEmpty(x)).ToList();
                filters.Rel02 = filters.Rel02?.Where(x => !string.IsNullOrEmpty(x)).ToList();
                filters.Rel03 = filters.Rel03?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
                filters.Rel04 = filters.Rel04?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
                //debo realizar la busqueda de los presupuestos
                var presup = await _presuSv.BuscarPresupuestos(filters, TokenCookie);

                if (!presup.Ok)
                {
                    throw new NegocioException(presup.Mensaje ?? "Hubo algun problema en la busqueda de Presupuestos.");
                }

                // Para operar con la lista de Productos Seleccionados
                var lista = presup.ListaEntidad ?? new List<PresupuestoListDto>();
                MetadataGeneral = presup.Meta;

                // Generar grid con productos mapeados
                GridCoreSmart<PresupuestoListDto> grid = GenerarGridPresupuestos(lista, filters.Pagina ?? 1, filters);

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
            #region Carga de Combo Estado y Tipo
            ViewBag.Pret_Id = ComboPresupuestoTipo();
            ViewBag.Pree_Id = ComboPresupuestoEstado("P");
            #endregion
            var hoy = DateTime.Now;

            PresupuestoDto presup = new() { pree_id='P', pre_vigencia_desde= hoy,pre_vigencia_hasta=hoy.AddDays(30),
            adm_id=AdministracionId,adm_nombre=AdministracionName};

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

                if (pres.ListaEntidad == null || pres.ListaEntidad.Count() == 0)
                {
                    throw new NegocioException("No se encontraron los datos del Presupuesto");
                }

                #region Carga de Combo Estado y Tipo
                ViewBag.Pret_Id = ComboPresupuestoTipo(pres.ListaEntidad[0].pret_id.ToString());
                ViewBag.Pree_Id = ComboPresupuestoEstado(pres.ListaEntidad[0].pree_id.ToString());
                #endregion

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
                ProductosActualesPresupuesto = productos;              

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

        [HttpPost]
        public async Task<IActionResult> ObtenerPresupuestoProductoActualizado(string pre_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (string.IsNullOrWhiteSpace(pre_id))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del presupuesto no fue recepcionado."));
                }

                var pres = await _presuSv.ObtenerDetallePresupuestoActualizado(pre_id, TokenCookie);
                if (!pres.Ok)
                {
                    throw new NegocioException(pres.Mensaje ?? "No se ha podido obtener el detalle del presupuesto Actualizado.");
                }

                // Generar grid con productos del presupuesto
                var productos = pres.ListaEntidad ?? new List<PresupuestoProductoDto>();
                ProductosActualesPresupuesto = productos;
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

        [HttpPost]
        public async Task<JsonResult> ConfirmarPresupuesto([FromBody] PresupuestoConfirmaReqDto request)
        {
            try
            {
                // Verificar autenticación - consistente con otros métodos
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, mensaje = "No autorizado" });

                if (request == null)
                {
                    return Json(new { ok = false, mensaje = "Los datos de confirmación no fueron recepcionados. Verifique." });
                }

                // Validaciones de entrada
                if (request.Datos == null || request.Datos.pret_id == default ||
                    request.Datos.pre_vigencia_desde == DateTime.MinValue ||
                    request.Datos.pre_vigencia_hasta == DateTime.MinValue ||
                    request.Datos.pre_vigencia_hasta < request.Datos.pre_vigencia_desde)
                {
                    return Json(new { ok = false, mensaje = "Los datos del Presupuesto son requeridos" });
                }

                var cantidadInvalida = request.Productos?.FirstOrDefault(p =>
                    p.pre_cantidad <= 0 ||
                    (p.up_id == "07" && p.pre_cantidad != decimal.Truncate(p.pre_cantidad)) ||
                    (p.up_id != "07" && p.pre_cantidad != decimal.Round(p.pre_cantidad, 3)));
                if (cantidadInvalida != null)
                {
                    var regla = cantidadInvalida.up_id == "07" ? "un numero entero" : "hasta tres decimales";
                    return Json(new { ok = false, mensaje = $"La cantidad del producto {cantidadInvalida.p_id} debe ser {regla}." });
                }

                request.Datos.adm_id = AdministracionId;
                request.Datos.usu_id = UserName;

                if (request.Productos == null || !request.Productos.Any())
                {
                    return Json(new { ok = false, mensaje = "Al menos un producto es necesario informar en el presupuesto." });
                }               

                // Preparar datos para envío
            
                
                var req = new AbmPlusGenDto
                {
                    Abm = request.Abm,
                    Json = JsonConvert.SerializeObject(request.Productos),
                    Json2 = JsonConvert.SerializeObject(request.Datos),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };

                // Llamada al servicio
                var respuesta = await _presuSv.ConfirmarPresupuesto(req, TokenCookie);

                // Procesamiento de respuesta
                if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
                {
                    // Log y limpieza de datos temporales
                    var msg = $"PRESUPUESTO de {request.Datos.cta_id}-{request.Datos.cta_denominacion} fue guardado exitosamente.";
                    _logger?.LogInformation(msg);
                   
                    // Respuesta de éxito
                    return Json(new
                    {
                        ok = true,
                        error = false,
                        msg,
                        pre_id = !string.IsNullOrWhiteSpace(respuesta.Entidad?.resultado_id)
                            ? respuesta.Entidad.resultado_id
                            : request.Datos.pre_id
                    });
                }
                else
                {
                    // Log y respuesta de error/advertencia
                    _logger?.LogWarning("Error en la confirmación del presupuesto: {Mensaje}", respuesta.Mensaje);
                    return Json(new
                    {
                        ok = false,
                        error = respuesta.EsError,
                        warn = respuesta.EsWarn,
                        mensaje = respuesta.Mensaje ?? "Error al procesar el combo/promoción"
                    });
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones no esperadas
                _logger?.LogError(ex, "Error inesperado al confirmar combo/promoción");
                return Json(new
                {
                    ok = false,
                    error = true,
                    msg = "Error interno al procesar la solicitud"
                });
            }
        }

        private GridCoreSmart<PresupuestoListDto> GenerarGridPresupuestos(List<PresupuestoListDto> lista, int page, QueryFilters filtro)
        {
            var presup = lista
                .OrderBy(c => c.pre_id)
                .ToList();

            var registrosPorPagina = filtro.Registros.GetValueOrDefault(_configuracion.NroRegistrosPagina);
            var totalRegistros = MetadataGeneral?.TotalCount ?? presup.Count;
            var totalPaginas = MetadataGeneral?.TotalPages
                ?? (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);
            var primerRegistro = presup.Count == 0 ? 0 : ((page - 1) * registrosPorPagina) + 1;
            var pagedList = new StaticPagedList<PresupuestoListDto>(
                presup,
                page,
                registrosPorPagina,
                totalRegistros
            );

            var grid = new GridCoreSmart<PresupuestoListDto>
            {
                ListaDatos = pagedList, //lista de combos
                CantidadReg = totalRegistros,
                PrimerRegistro = primerRegistro,
                UltimoRegistro = presup.Count == 0 ? 0 : Math.Min(primerRegistro + presup.Count - 1, totalRegistros),
                RegistroFinal = totalRegistros,
                CantidadPaginas = totalPaginas,
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
            ProductosActualesPresupuesto = new List<PresupuestoProductoDto>();
            //ADMINISTRACIONES que se cargarán en el filtro solamente las activas.
            ObtenerAdministracionesLista(_admSv, "S");
            ObtenerEstadoPresupuesto(_presuSv);
            ObtenerTipoPresupuesto(_presuSv);

            if (RubroLista.Count == 0 )
            {
                ObtenerRubros(_rubroServicio);
            }

            #region Carga de Rubros
            var rubs = RubroLista
                .Select(r => new ComboGenDto
                {
                    Id = r.Rub_Id,
                    Descripcion = r.Rub_Id + " - " + r.Rub_Desc
                })
                .ToList();
            ViewBag.Rel02B2 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);
            #endregion


            //CLIENTE
            var listR011 = new List<ComboGenDto>();
            ViewBag.Rel011List = HelperMvc<ComboGenDto>.ListaGenerica(listR011);
            //USUARIO
            var listR022 = new List<ComboGenDto>();
            ViewBag.Rel022List = HelperMvc<ComboGenDto>.ListaGenerica(listR022);
            //ESTADO
            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel03 = ComboPresupuestoEstado();
            ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
            ViewBag.Rel03B2 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
            //ADMINISTRACION
            var adm = AdministracionesLista;
            var admins = adm.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre }).ToList();
            ViewBag.Rel04 = HelperMvc<ComboGenDto>.ListaGenerica(admins,AdministracionId);

        }

    }
}
