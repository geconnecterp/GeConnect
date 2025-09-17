using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public OfertasController(IOptions<AppSettings> options, IHttpContextAccessor contexo, 
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio, 
            ICuentaServicio cuenta, IRubroServicio rubro)
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
        }
        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Inicializar lista al ingresar el modulo
                ProductosSeleccionadosV02 = [];

                ViewData["Titulo"] = "Alta de Oferta (sin activar)";
                CargarDatosIniciales(true);
                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de BSS");
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }


        /// <summary>
        /// Confirma el alta de ofertas con los productos seleccionados en sesión
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ConfirmacionAltaOferta([FromBody] ConfirmacionOfertaRequestDto request)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });

                // ✅ VALIDACIÓN: Datos de entrada
                var validacionRequest = ValidarRequestConfirmacion(request);
                if (!validacionRequest.EsValido)
                {
                    _logger?.LogWarning("Validación fallida en ConfirmacionAltaOferta: {Error}", validacionRequest.MensajeError);
                    return Json(new { error = true, msg = validacionRequest.MensajeError });
                }

                // ✅ VALIDACIÓN: Productos en sesión
                if (ProductosSeleccionadosV02 == null || !ProductosSeleccionadosV02.Any())
                {
                    _logger?.LogWarning("ConfirmacionAltaOferta llamada sin productos en sesión");
                    return Json(new { error = true, msg = "No hay productos seleccionados para la oferta" });
                }

                // ✅ LOG: Información del proceso
                var totalProductos = ProductosSeleccionadosV02.Count;
                var totalCanales = ObtenerCanalesParaProcesamiento(request).Count;
                _logger?.LogInformation("Iniciando confirmación de oferta: {TotalProductos} productos, {TotalCanales} canales",
                    totalProductos, totalCanales);

                // ✅ CONSTRUCCIÓN: AbmPlusGenDto optimizado
                var abmRequest = ConstruirAbmPlusGenDto(request);

                // ✅ LLAMADA: Al servicio de ofertas
                var respuesta = await _ofertaServicio.ConfirmacionAltaOferta(abmRequest, TokenCookie);

                // ✅ PROCESAMIENTO: Respuesta del servicio
                if (respuesta.Ok && (!respuesta.EsError || !respuesta.EsWarn) )
                {
                    _logger?.LogInformation("Oferta confirmada exitosamente para {CantidadProductos} productos", totalProductos);

                    // ✅ LIMPIAR: Sesión después del éxito
                    ProductosSeleccionadosV02.Clear();

                    return Json(new
                    {
                        error = false,
                        msg = respuesta.Mensaje ?? "Ofertas guardadas correctamente",
                        totalOfertas = CalcularTotalOfertas(request),
                        totalProductos = totalProductos,
                        totalCanales = totalCanales
                    });
                }
                else
                {
                    _logger?.LogWarning("Error en servicio de ofertas: {Mensaje}", respuesta.Mensaje);
                    return Json(new
                    {
                        error = respuesta.EsError,
                        warn = respuesta.EsWarn,
                        msg = respuesta.Mensaje ?? "Error al procesar la oferta"
                    });
                }
            }
            catch (JsonException jsonEx)
            {
                _logger?.LogError(jsonEx, "Error de serialización JSON en ConfirmacionAltaOferta");
                return Json(new { error = true, msg = "Error al procesar los datos de la oferta" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error inesperado al confirmar alta de oferta");
                return Json(new { error = true, msg = "Error interno del servidor" });
            }
        }


        /// <summary>
        /// Recibe un producto y lo agrega a la lista de productos seleccionados para ofertas
        /// </summary>
        [HttpPost]
        public IActionResult PresentarProductoOferta(ProductoListaDto producto, int pag = 1)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (producto == null || string.IsNullOrWhiteSpace(producto.P_id))
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("Producto requerido"));
                }

                // Para operar con la lista de Productos Seleccionados
                var lista = ProductosSeleccionadosV02 ;

                // Verificar si el producto ya existe en la lista
                if (!lista.Any(p => p.P_id == producto.P_id))
                {
                    lista.Add(producto);
                }

                // Actualizar la lista resguardada
                ProductosSeleccionadosV02 = lista;

                // MAPEAR la lista ProductoBusquedaDto a Producto Oferta Dto
                var listaOfertas = MapearProductosAOfertas(lista);

                // Generar grid con productos mapeados
                var grid = GenerarGridProductosOferta(listaOfertas, pag);

                return PartialView("_gridProductosOferta", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar producto para oferta: {ProductoId}", producto?.P_id);
                return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar producto a ofertas"));
            }
        }

        /// <summary>
        /// Recibe múltiples productos y los agrega a la lista de productos seleccionados para ofertas
        /// </summary>
        [HttpPost]
        public IActionResult PresentarProductosOfertaMultiple([FromBody] List<ProductoListaDto> productos, int pag = 1)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (productos == null || !productos.Any())
                {
                    return PartialView("_gridMensaje", CrearRespuestaError("Lista de productos requerida"));
                }

                // Para operar con la lista de Productos Seleccionados
                var lista = ProductosSeleccionadosV02;

                // Agregar productos que no existan en la lista
                int productosAgregados = 0;
                foreach (var producto in productos)
                {
                    if (!string.IsNullOrWhiteSpace(producto.P_id) && 
                        !lista.Any(p => p.P_id == producto.P_id))
                    {
                        lista.Add(producto);
                        productosAgregados++;
                    }
                }

                // Actualizar la lista resguardada
                ProductosSeleccionadosV02 = lista;

                // MAPEAR la lista a ProductoOfertaDto
                var listaOfertas = MapearProductosAOfertas(lista);

                // Generar grid con productos mapeados
                var grid = GenerarGridProductosOferta(listaOfertas, pag);

                // Retornar resultado con información de productos agregados
                ViewBag.ProductosAgregados = productosAgregados;
                ViewBag.ProductosExistentes = productos.Count - productosAgregados;

                return PartialView("_gridProductosOferta", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al presentar productos múltiples para oferta");
                return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
            }
        }

        /// <summary>
        /// Busca y retorna la lista de canales disponibles
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BuscarCanales()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var respuesta = await _ofertaServicio.BuscarCanales(TokenCookie);

                if (!respuesta.Ok || respuesta.EsError)
                {
                    return PartialView("_gridMensaje", CrearRespuestaError(respuesta.Mensaje ?? "Error al obtener canales"));
                }

                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    return PartialView("_gridMensaje", CrearRespuestaWarning("No se encontraron canales disponibles"));
                }

                // Generar grid con canales
                var grid = GenerarGridCanales(respuesta.ListaEntidad);

                return PartialView("_gridCanales", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar canales");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al obtener canales"));
            }
        }


        #region Métodos Privados

        /// <summary>
        /// Genera el grid optimizado para canales usando GridCoreSmart
        /// </summary>
        private GridCoreSmart<CanalDto> GenerarGridCanales(List<CanalDto> canales, int pag = 1)
        {
            var canalesOrdenados = canales
                .OrderBy(c => c.adm_id)
                .ThenBy(c => c.lp_id)
                .ToList();

            const int registrosPorPagina = 10;
            var pagedList = new StaticPagedList<CanalDto>(
                canalesOrdenados,
                pag,
                registrosPorPagina,
                canales.Count
            );

            return new GridCoreSmart<CanalDto>
            {
                ListaDatos = pagedList,
                CantidadReg = canales.Count,
                PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                UltimoRegistro = Math.Min(pag * registrosPorPagina, canales.Count),
                RegistroFinal = canales.Count,
                CantidadPaginas = (int)Math.Ceiling((double)canales.Count / registrosPorPagina),
                PaginaActual = pag,
                Sort = "adm_nombre",
                SortDir = "ASC",
                DatoAux01 = $"Canales cargados: {DateTime.Now:HH:mm:ss}"
            };
        }

        /// <summary>
        /// Crea una respuesta de Warning estandarizada
        /// </summary>
        private RespuestaGenerica<EntidadBase> CrearRespuestaWarning(string mensaje)
        {
            return new RespuestaGenerica<EntidadBase>
            {
                Mensaje = mensaje,
                Ok = false,
                EsWarn = true,
                EsError = false
            };
        }

        /// <summary>
        /// Genera el grid optimizado para productos en ofertas usando GridCoreSmart
        /// </summary>
        private GridCoreSmart<ProductoOfertaDto> GenerarGridProductosOferta(List<ProductoOfertaDto> productos, int pag = 1)
        {
            var productosOrdenados = productos
                .OrderBy(p => p.p_desc)
                .ToList();

            const int registrosPorPagina = 10;
            var pagedList = new StaticPagedList<ProductoOfertaDto>(
                productosOrdenados,
                pag,
                registrosPorPagina,
                productos.Count
            );

            return new GridCoreSmart<ProductoOfertaDto>
            {
                ListaDatos = pagedList,
                CantidadReg = productos.Count,
                PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                UltimoRegistro = Math.Min(pag * registrosPorPagina, productos.Count),
                RegistroFinal = productos.Count,
                CantidadPaginas = (int)Math.Ceiling((double)productos.Count / registrosPorPagina),
                PaginaActual = pag,
                Sort = "p_desc",
                SortDir = "ASC",
                DatoAux01 = $"Productos en ofertas: {DateTime.Now:HH:mm:ss}"
            };
        }

        /// <summary>
        /// Crea una respuesta de error estandarizada
        /// </summary>
        private RespuestaGenerica<EntidadBase> CrearRespuestaError(string mensaje)
        {
            return new RespuestaGenerica<EntidadBase>
            {
                Mensaje = mensaje,
                Ok = false,
                EsWarn = false,
                EsError = true
            };
        }


        /// <summary>
        /// Mapea una lista de ProductoBusquedaDto a ProductoOfertaDto
        /// </summary>
        private List<ProductoOfertaDto> MapearProductosAOfertas(List<ProductoListaDto> productos)
        {
            return productos.Select(p => new ProductoOfertaDto
            {
                p_id = p.P_id,
                p_desc = p.P_desc,
                p_pcosto = p.P_pcosto,// decimal.TryParse(p.P_pcosto, out var costo) ? costo : 0m,
                p_mayorista = p.p_pvta_001,// Precio mayorista = p_pvta_001
                p_minorista = p.p_pvta_002,// Precio minorista = p_pvta_002
                //p_estado = _ofertaServicio.ConocerEstadoOferta(p.P_id,AdministracionId,p.p)
            }).ToList();
        }
        #endregion

        /// <summary>
        /// Valida el request de confirmación de oferta con parsing mejorado
        /// </summary>
        private (bool EsValido, string MensajeError) ValidarRequestConfirmacion(ConfirmacionOfertaRequestDto request)
        {
            // ✅ VALIDACIÓN: Precio
            if (request.Precio <= 0)
                return (false, "El precio debe ser mayor a cero");

            // ✅ VALIDACIÓN: Tope de venta
            if (request.TopeVenta < 0)
                return (false, "El tope de venta debe ser mayor o igual a cero");

            // ✅ VALIDACIÓN: Fechas (ya son DateTime, no necesitamos TryParse)
            if (request.FechaDesde == default || request.FechaHasta == default)
                return (false, "Las fechas de inicio y fin son requeridas");

            if (request.FechaDesde > request.FechaHasta)
                return (false, "La fecha de inicio debe ser menor o igual a la fecha de fin");

            if (request.FechaDesde.Date < DateTime.Today)
                return (false, "La fecha de inicio no puede ser anterior a la fecha actual");

            // ✅ CORRECCIÓN: Validación de período máximo
            if (request.FechaHasta > request.FechaDesde.AddDays(30))
                return (false, "El período de la oferta no puede exceder 30 días");

            // ✅ VALIDACIÓN: Canales
            var canalesValidos = ObtenerCanalesParaProcesamiento(request);
            if (!canalesValidos.Any())
                return (false, "Debe seleccionar al menos un canal");

            return (true, string.Empty);
        }

        /// <summary>
        /// Construye el AbmPlusGenDto con logging detallado para debugging
        /// </summary>
        private AbmPlusGenDto ConstruirAbmPlusGenDto(ConfirmacionOfertaRequestDto request)
        {
            try
            {
                // ✅ JSON: Solo p_id de productos (desde sesión)
                var productosIds = ProductosSeleccionadosV02.Select(p => new { p_id = p.P_id }).ToList();
                var jsonProductos = JsonConvert.SerializeObject(productosIds);
                
                _logger?.LogDebug("JSON Productos: {JsonProductos}", jsonProductos);

                // ✅ JSON2: Canales con solo adm_id y lp_id
                var canalesParaSerializar = ObtenerCanalesParaProcesamiento(request)
                    .Select(c => new { adm_id = c.AdmId, lp_id = c.LpId })
                    .ToList();
                var jsonCanales = JsonConvert.SerializeObject(canalesParaSerializar);
                
                _logger?.LogDebug("JSON Canales: {JsonCanales}", jsonCanales);

                // ✅ JSON3: Datos de la oferta usando ParamOferta
                var parametrosOferta = new ParamOferta
                {
                    Precio = request.Precio,
                    Desde = request.FechaDesde,
                    Hasta = request.FechaHasta,
                    TopeVta = request.TopeVenta
                };
                var jsonOferta = JsonConvert.SerializeObject(parametrosOferta);
                
                _logger?.LogDebug("JSON Oferta: {JsonOferta}", jsonOferta);

                // ✅ CONSTRUCCIÓN: AbmPlusGenDto optimizado
                var abmDto = new AbmPlusGenDto
                {
                    // ✅ DATOS BASE: Heredados de AbmGenDto
                    Abm = 'A', // Alta de oferta
                    Objeto = "OFERTA",
                    Json = jsonProductos,
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    
                    // ✅ DATOS PLUS: Específicos de AbmPlusGenDto
                    Json2 = jsonCanales,
                    Json3 = jsonOferta,
                    Json4 = string.Empty,
                    Json5 = string.Empty,
                    
                    // ✅ CONFIGURACIÓN: Campos adicionales
                    IdFile = Guid.NewGuid(),
                    SoloPLista = 'N',
                    Nuevos = true,
                    DatosLogisticos = false,
                    Inactivos = false,
                    vaciarTemporal = false
                };

                _logger?.LogInformation("AbmPlusGenDto construido exitosamente para {CantidadProductos} productos y {CantidadCanales} canales", 
                    productosIds.Count, canalesParaSerializar.Count);

                return abmDto;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al construir AbmPlusGenDto");
                throw;
            }
        }

        /// <summary>
        /// Obtiene la lista de canales para procesamiento según el modo de selección
        /// </summary>
        private List<CanalSeleccionadoDto> ObtenerCanalesParaProcesamiento(ConfirmacionOfertaRequestDto request)
        {
            return request.ModoSeleccion?.ToLower() switch
            {
                "individual" when request.CanalIndividual != null => 
                    new List<CanalSeleccionadoDto> { request.CanalIndividual },
                
                "multiple" when request.Canales?.Any() == true => 
                    request.Canales,
                
                _ => new List<CanalSeleccionadoDto>()
            };
        }

        /// <summary>
        /// Calcula el total de ofertas que se crearán
        /// </summary>
        private int CalcularTotalOfertas(ConfirmacionOfertaRequestDto request)
        {
            var totalProductos = ProductosSeleccionadosV02?.Count ?? 0;
            var totalCanales = ObtenerCanalesParaProcesamiento(request).Count;
            return totalProductos * totalCanales;
        }

        private void CargarDatosIniciales(bool actualizar)
        {
            if (ProveedoresLista.Count == 0 || actualizar)
            {
                ObtenerProveedores(_cuentaServicio, "BI");
            }

            if (RubroLista.Count == 0 || actualizar)
            {
                ObtenerRubros(_rubroServicio);
            }

            var listR03 = new List<ComboGenDto>();
            ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
        }

        /// <summary>
        /// Obtiene el estado de las ofertas para un producto específico en todos los canales
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ObtenerEstadoOfertaProducto(string p_id)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });

                // ✅ VALIDACIÓN: ID de producto
                if (string.IsNullOrEmpty(p_id))
                {
                    _logger?.LogWarning("ObtenerEstadoOfertaProducto llamado sin ID de producto");
                    return Json(new { error = true, msg = "ID de producto requerido" });
                }

                _logger?.LogInformation("Obteniendo estado de oferta para producto {ProductoId}", p_id);

                // ✅ LLAMADA: Al servicio de ofertas
                var respuesta = await _ofertaServicio.ObtenerEstadoOfertaProducto(p_id, TokenCookie);

                // ✅ PROCESAMIENTO: Respuesta del servicio
                if (!respuesta.Ok || respuesta.EsError)
                {
                    _logger?.LogWarning("Error en servicio de ofertas: {Mensaje}", respuesta.Mensaje);
                    return Json(new { 
                        error = true, 
                        msg = respuesta.Mensaje ?? "Error al obtener estado de oferta para el producto" 
                    });
                }

                // ✅ VALIDACIÓN: Datos obtenidos
                var estados = respuesta.ListaEntidad ?? new List<OfertaEstadoDto>();
                
                if (!estados.Any())
                {
                    _logger?.LogInformation("No hay información de ofertas para producto {ProductoId}", p_id);
                    return Json(new { 
                        error = false, 
                        warn = true,
                        msg = "No hay información de ofertas disponible para este producto",
                        estados = estados,
                        totalEstados = 0
                    });
                }

                _logger?.LogInformation("Estados de oferta obtenidos para producto {ProductoId}: {CantidadEstados}", 
                    p_id, estados.Count);

                // ✅ RESPUESTA: Con datos completos
                return Json(new {
                    error = false,
                    warn = false,
                    msg = "Estados de oferta obtenidos correctamente",
                    estados,
                    totalEstados = estados.Count
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener estado de oferta para producto: {ProductoId}", p_id);
                return Json(new { 
                    error = true, 
                    msg = "Error interno al obtener estado de oferta" 
                });
            }
        }
    }
}
