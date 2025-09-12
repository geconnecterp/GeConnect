using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;
        public OfertasController(IOptions<AppSettings> options, IHttpContextAccessor contexo, 
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio)
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
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

                // MAPEAR la lista ProductoBusquedaDto a ProductoOfertaDto
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
                .OrderBy(c => c.adm_nombre)
                .ThenBy(c => c.lp_desc)
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
    }
}
