using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;

        public OfertasController(IOptions<AppSettings> options, IHttpContextAccessor contexo, ILogger<OfertasController> logger)
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
        }
        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Inicializar lista al ingresar el modulo
                ProductosSeleccionados = [];

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
        public IActionResult PresentarProductoOferta(ProductoBusquedaDto producto, int pag = 1)
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
                var lista = ProductosSeleccionados ?? new List<ProductoBusquedaDto>();

                // Verificar si el producto ya existe en la lista
                if (!lista.Any(p => p.P_id == producto.P_id))
                {
                    lista.Add(producto);
                }

                // Actualizar la lista resguardada
                ProductosSeleccionados = lista;

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

        #region Métodos Privados

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
        private List<ProductoOfertaDto> MapearProductosAOfertas(List<ProductoBusquedaDto> productos)
        {
            return productos.Select(p => new ProductoOfertaDto
            {
                p_id = p.P_id,
                p_desc = p.P_desc,
                p_pcosto = decimal.TryParse(p.P_pcosto, out var costo) ? costo : 0m,
                p_mayorista = decimal.TryParse(p.P_pvta, out var mayorista) ? mayorista : 0m,
                p_minorista = decimal.TryParse(p.P_pvta_oferta, out var minorista) ? minorista : 0m,
                p_estado = 'A' // Estado por defecto
            }).ToList();
        }
        #endregion
    }
}
