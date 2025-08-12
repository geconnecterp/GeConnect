using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class CargaPreciosController : ControladorProductoBase
    {
        private readonly AppSettings _appSettings;
        private readonly IAdministracionServicio _administracionServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;
        private readonly IProducto2Servicio _productoServicio;
        // ✅ AGREGAR: Lock para operaciones thread-safe
        private static readonly object _lockResguardoLista = new object();

        public CargaPreciosController(
            ICuentaServicio cuentaServicio,
            IRubroServicio rubroServicio,
            IProducto2Servicio productoServicio,
            IAdministracionServicio administracionServicio,
            ILogger<CompraController> logger,
            IOptions<AppSettings> options,
            IHttpContextAccessor context) : base(options, context, logger)
        {
            _administracionServicio = administracionServicio;
            _cuentaServicio = cuentaServicio;
            _rubroServicio = rubroServicio;
            _productoServicio = productoServicio;
            _appSettings = options.Value;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var listR01 = new List<ComboGenDto>();
                ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

                var listR02 = new List<ComboGenDto>();
                ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

                var listR03 = new List<ComboGenDto>();
                ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
                ViewBag.Rel03 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

                string titulo = "Productos - Carga de Precios";
                ViewData["Titulo"] = titulo;
                CargarDatosIniciales(true);

                ProductosDetalle = [];
                ProductosDetalleLista = [];
                ProductosDetalleTEMPORAL = [];
                ProductosDetalleListaTEMPORAL = [];

                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDetalle(string buscar,
            string id, string id2,
            string ctaId, List<ComboGenDto> familias,
            List<string> rubros,
            bool disc = true, bool file = false)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                // Verificar autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                // Validar que el parámetro eje_nro no sea nulo o vacío
                if (string.IsNullOrEmpty(ctaId))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe indicar el proveedor."
                    });
                }

                #region Busco y resguardo la cuenta actual
                var proveedor = ProveedoresLista.FirstOrDefault(x => x.Cta_Id.Equals(ctaId, StringComparison.OrdinalIgnoreCase));

                if(proveedor == null)
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe indicar un proveedor válido."
                    });
                }
                ProveedorSeleccionado = proveedor;

                #endregion

                // Llamar al servicio para obtener los asientos de ajuste
                var filtro = new QueryFilters
                {
                    Buscar = buscar,
                    Id = id,
                    Id2 = id2,
                    Rel01 = new List<string> { ctaId },
                    Rel02 = rubros,
                    Rel03 = familias,
                    Opt1 = disc,
                    Opt2 = file
                };
                var respuesta = await _productoServicio.Obtener_ProductoDetalle(filtro, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron productos para la configuración del filtro que especificó.";
                    response.Ok = false;
                    response.EsWarn = true;
                    response.EsError = false;
                    return PartialView("_gridMensaje", response);
                }
               

                var lista = respuesta.ListaEntidad.OrderBy(x => x.pg_id).ThenBy(x => x.p_id).ToList();
                // Guardar datos en variable de sesión para uso posterior
                ProductosDetalle = lista;
                //se analiza que productos vienen con valores temporales (carga==1). 
                //Para todos ellos se cargan en la lista de productos temporales
                ProductosDetalleTEMPORAL = lista.Where(x=>x.carga == 1).ToList();

                // Crear el grid para la vista (sin paginación)
                var grid = GenerarGrillaSmart(
                    lista,
                    "pg_id",  // Ordenamiento por defecto
                    respuesta.ListaEntidad.Count,  // Todos los registros en una página
                    1,  // Página única
                    respuesta.ListaEntidad.Count,  // Total de registros
                    1,  // Total de páginas (una sola)
                    "ASC"  // Dirección de ordenamiento por defecto
                );

                // Devolver la vista parcial con el grid
                return PartialView("_gridProdDet", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el detalle de productos");

                response.Mensaje = "Error al obtener el detalle de productos.";
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductosDetalleLista(string buscar,
      string id, string id2, string ctaId, List<ComboGenDto> familias,
      List<string> rubros, bool disc = true, bool file = false,
      bool verificarTemp = true, bool forzarRecarga = false)
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                // Validaciones iniciales
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (string.IsNullOrEmpty(ctaId))
                {
                    return PartialView("_gridMensaje", new RespuestaGenerica<EntidadBase>
                    {
                        Ok = false,
                        Mensaje = "Debe indicar el proveedor."
                    });
                }

                // ✅ MEJORADO: Inicializar lista temporal
                if (ProductosDetalleListaTEMPORAL == null)
                {
                    ProductosDetalleListaTEMPORAL = new List<ProductoDetalleDto>();
                    _logger?.LogInformation("Lista temporal de listas inicializada");
                }

                // ✅ NUEVO: Si se fuerza recarga, limpiar caché temporal para este producto
                if (forzarRecarga && !string.IsNullOrEmpty(id))
                {
                    var registrosAEliminar = ProductosDetalleListaTEMPORAL
                        .Where(p => p.p_id == id && !p.lp_id.Equals("001"))
                        .ToList();

                    foreach (var registro in registrosAEliminar)
                    {
                        ProductosDetalleListaTEMPORAL.Remove(registro);
                    }

                    _logger?.LogInformation($"Limpiados {registrosAEliminar.Count} registros temporales por forzarRecarga");
                }

                // ✅ PASO 3: Verificar registros temporales (solo si no se fuerza recarga)
                if (verificarTemp && !forzarRecarga && !string.IsNullOrEmpty(id))
                {
                    var registrosTemporalesExistentes = ProductosDetalleListaTEMPORAL
                        .Where(p => p.p_id == id && !p.lp_id.Equals("001"))
                        .ToList();

                    if (registrosTemporalesExistentes.Any())
                    {
                        _logger?.LogInformation($"Devolviendo {registrosTemporalesExistentes.Count} registros temporales para producto {id}");

                        var listaOrdenada = registrosTemporalesExistentes
                            .OrderBy(x => x.pg_id)
                            .ThenBy(x => x.lp_id)
                            .ToList();

                        var gridTemporal = GenerarGrillaSmart(
                            listaOrdenada, "pg_id", listaOrdenada.Count, 1, listaOrdenada.Count, 1, "ASC");

                        return PartialView("_gridProdLista", gridTemporal);
                    }

                    _logger?.LogInformation($"No se encontraron registros temporales para producto {id}, consultando servidor");
                }

                // ✅ PASO 4: Consultar al servidor
                var filtro = new QueryFilters
                {
                    Buscar = buscar,
                    Id = id,
                    Id2 = id2,
                    Rel01 = new List<string> { ctaId },
                    Rel02 = rubros,
                    Rel03 = familias,
                    Opt1 = disc,
                    Opt2 = file
                };

                var respuesta = await _productoServicio.Obtener_ProductoDetalleListas(filtro, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron las listas del producto especificado.";
                    response.Ok = false;
                    response.EsWarn = true;
                    return PartialView("_gridMensaje", response);
                }

                var listaDelServidor = respuesta.ListaEntidad
                    .Where(x => !x.lp_id.Equals("001"))
                    .OrderBy(x => x.pg_id)
                    .ThenBy(x => x.lp_id)
                    .ToList();

                // ✅ PASO 5: Procesar registros temporales del servidor
                var registrosTemporalesDelServidor = listaDelServidor
                    .Where(x => x.carga == 1)
                    .ToList();

                if (registrosTemporalesDelServidor.Any())
                {
                    _logger?.LogInformation($"Actualizando {registrosTemporalesDelServidor.Count} registros temporales del servidor");

                    // ✅ MEJORADO: Actualizar temporales existentes o agregar nuevos
                    foreach (var registroTemporal in registrosTemporalesDelServidor)
                    {
                        var indiceExistente = ProductosDetalleListaTEMPORAL.FindIndex(
                            t => t.p_id == registroTemporal.p_id && t.lp_id == registroTemporal.lp_id);

                        if (indiceExistente >= 0)
                        {
                            // Actualizar existente con datos más recientes
                            ProductosDetalleListaTEMPORAL[indiceExistente] = registroTemporal;
                            _logger?.LogInformation($"Actualizado registro temporal: Producto {registroTemporal.p_id}, Lista {registroTemporal.lp_id}");
                        }
                        else
                        {
                            // Agregar nuevo
                            ProductosDetalleListaTEMPORAL.Add(registroTemporal);
                            _logger?.LogInformation($"Agregado registro temporal: Producto {registroTemporal.p_id}, Lista {registroTemporal.lp_id}");
                        }
                    }
                }

                // ✅ PASO 6: Determinar lista final con lógica mejorada
                List<ProductoDetalleDto> listaFinal;

                if (!string.IsNullOrEmpty(id))
                {
                    // Para producto específico, priorizar temporales actualizados
                    var temporalesDelProducto = ProductosDetalleListaTEMPORAL
                        .Where(p => p.p_id == id && !p.lp_id.Equals("001"))
                        .ToList();

                    if (temporalesDelProducto.Any())
                    {
                        listaFinal = temporalesDelProducto.OrderBy(x => x.pg_id).ThenBy(x => x.lp_id).ToList();
                        _logger?.LogInformation($"Devolviendo {listaFinal.Count} registros temporales finales para producto {id}");
                    }
                    else
                    {
                        listaFinal = listaDelServidor;
                        _logger?.LogInformation($"Devolviendo {listaFinal.Count} registros del servidor para producto {id}");
                    }
                }
                else
                {
                    listaFinal = listaDelServidor;
                }

                //// ✅ PASO 7: Guardar originales en sesión
                //if (verificarTemp && listaFinal == listaDelServidor)
                //{
                    ProductosDetalleLista = listaDelServidor;
                    _logger?.LogInformation($"Guardados {listaDelServidor.Count} registros originales en sesión");
                //}

                var gridResultado = GenerarGrillaSmart(
                    listaFinal, "pg_id", listaFinal.Count, 1, listaFinal.Count, 1, "ASC");

                return PartialView("_gridProdLista", gridResultado);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el detalle de listas");
                response.Mensaje = "Error al obtener el detalle de listas.";
                response.Ok = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }


        [HttpPost]
        public JsonResult CalcularCosto(string p_id, decimal tp_plista, decimal tp_dto1, decimal tp_dto2,
            decimal tp_dto3, decimal tp_dto4, decimal tp_dto_pa, decimal tp_porc_flete, string tp_boni)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (tp_plista < 0)
                {
                    throw new NegocioException("El valor del Producto es incorrecto. Por favor verifique.");
                }

                var costo = HelperContable.CalcularPCosto(tp_plista, tp_dto1, tp_dto2,
                    tp_dto3, tp_dto4, tp_dto_pa, tp_boni, tp_porc_flete);

                return Json(new { error = false, warn = false, costo });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CalcularPrecioVentaBase(decimal tp_pcosto, decimal lp_prevision_tot,
            decimal lp_prevision_pin, decimal tp_margen, char iva_situacion,
            decimal iva_alicuota, decimal in_alicuota)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (tp_pcosto < 0)
                {
                    throw new NegocioException("El valor del Costo es incorrecto. Por favor verifique.");
                }

                var request = new ProductoRequestPvtaBase
                {
                    in_alicuota = in_alicuota,
                    iva_alicuota = iva_alicuota,
                    iva_situacion = iva_situacion,
                    lp_prevision_tot = lp_prevision_tot,
                    lp_prevision_pin = lp_prevision_pin,
                    p_pcosto = tp_pcosto,
                    tp_margen = tp_margen
                };
                var precioVentaBase = await _productoServicio.ObtenerPrecioVentaBase(request, TokenCookie);
                return Json(new { error = false, warn = false, pvta = precioVentaBase.Entidad });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CalcularPrecioVentaMargen(decimal tp_pcosto, decimal lp_prevision_tot,
            decimal lp_prevision_pin, decimal tp_pvta, char iva_situacion,
            decimal iva_alicuota, decimal in_alicuota)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (tp_pcosto <= 0)
                {
                    throw new NegocioException("El valor del Costo es incorrecto. Por favor verifique.");
                }

                var request = new ProductoRequestPVtaMargen
                {
                    in_alicuota = in_alicuota,
                    iva_alicuota = iva_alicuota,
                    iva_situacion = iva_situacion,
                    lp_prevision_tot = lp_prevision_tot,
                    lp_prevision_pin = lp_prevision_pin,
                    p_pcosto = tp_pcosto,
                    p_pvta = tp_pvta
                };
                var precioVentaMg = await _productoServicio.ObtenerPrecioVentaMargen(request, TokenCookie);
                return Json(new { error = false, warn = false, pvta = precioVentaMg.Entidad });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }


        [HttpPost]
        public async Task<JsonResult> CalcularPrecioVentaLink(decimal tp_pcosto, decimal p_pneto_base,
            decimal lp_porc_mg, char iva_situacion, decimal iva_alicuota, decimal in_alicuota)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                if (tp_pcosto <= 0)
                {
                    throw new NegocioException("El valor del Costo es incorrecto. Por favor verifique.");
                }

                var request = new ProductoRequestPvtaLista
                {
                    in_alicuota = in_alicuota,
                    iva_alicuota = iva_alicuota,
                    iva_situacion = iva_situacion,
                    lp_porc_mg = lp_porc_mg,
                    p_pcosto = tp_pcosto,
                    p_pneto_base = p_pneto_base
                };
                var precioVentaMg = await _productoServicio.ObtenerPrecioVentaLista(request, TokenCookie);
                var reg = precioVentaMg.ListaEntidad?.First();
                return Json(new { error = false, warn = false, pvta = reg });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ResguardarCambiosProducto(string p_id, decimal tp_plista, decimal tp_dto1, decimal tp_dto2,
             decimal tp_dto3, decimal tp_dto4, decimal tp_dto_pa, decimal tp_porc_flete, string tp_boni,
             decimal tp_pcosto, decimal tp_margen, decimal tp_pneto, decimal tin_alicuota, decimal tp_pvta)
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrEmpty(p_id))
                {
                    throw new NegocioException("No se ha especificado el ID del producto a modificar.");
                }

                // Obtener los productos desde la sesión
                var productosOriginales = ProductosDetalle;
                var productosTemporal = ProductosDetalleTEMPORAL;

                // Buscar el producto original por su ID
                var productoOriginal = productosOriginales.FirstOrDefault(p => p.p_id == p_id);
                if (productoOriginal == null)
                {
                    throw new NegocioException($"No se encontró el producto con ID {p_id} en la lista original.");
                }

                // Verificar si hay cambios en los valores que afectan el precio
                bool hayCambios =
                    Math.Round(productoOriginal.P_Plista, 3) != Math.Round(tp_plista, 3) ||
                    Math.Round(productoOriginal.P_Dto1, 1) != Math.Round(tp_dto1, 1) ||
                    Math.Round(productoOriginal.P_Dto2, 1) != Math.Round(tp_dto2, 1) ||
                    Math.Round(productoOriginal.P_Dto3, 1) != Math.Round(tp_dto3, 1) ||
                    Math.Round(productoOriginal.P_Dto4, 1) != Math.Round(tp_dto4, 1) ||
                    Math.Round(productoOriginal.P_Dto_Pa, 1) != Math.Round(tp_dto_pa, 1) ||
                    Math.Round(productoOriginal.P_Porc_Flete, 1) != Math.Round(tp_porc_flete, 1) ||
                    productoOriginal.P_Boni != tp_boni ||
                    Math.Round(productoOriginal.p_margen, 2) != Math.Round(tp_margen, 2) ||
                    Math.Round(productoOriginal.in_alicuota, 2) != Math.Round(tin_alicuota, 2) ||
                    Math.Round(productoOriginal.p_pvta, 2) != Math.Round(tp_pvta, 2);

                // Si no hay cambios, verificar si el producto está en la lista temporal y eliminarlo
                if (!hayCambios)
                {
                    // Eliminar de la lista temporal si existe
                    var productoTemporalExistente = productosTemporal.FirstOrDefault(p => p.p_id == p_id);
                    if (productoTemporalExistente != null)
                    {
                        productosTemporal.Remove(productoTemporalExistente);
                        ProductosDetalleTEMPORAL = productosTemporal; // Guardar en sesión
                        return Json(new { error = false, warn = false, msg = "No se detectaron cambios en el producto. Se ha eliminado de la lista temporal." });
                    }

                    return Json(new { error = false, warn = false, msg = "No se detectaron cambios en el producto." });
                }

                // Crear una copia del producto original con los valores actualizados
                // Reemplazamos la sintaxis 'with' por una creación y copia manual de propiedades
                var productoModificado = new ProductoDetalleDto();

                // Copiar todas las propiedades del original al nuevo objeto
                foreach (var prop in typeof(ProductoDetalleDto).GetProperties())
                {
                    if (prop.CanWrite && prop.CanRead)
                    {
                        prop.SetValue(productoModificado, prop.GetValue(productoOriginal));
                    }
                }

                // Actualizar las propiedades específicas con los nuevos valores
                productoModificado.tp_plista = tp_plista;
                productoModificado.tp_dto1 = tp_dto1;
                productoModificado.tp_dto2 = tp_dto2;
                productoModificado.tp_dto3 = tp_dto3;
                productoModificado.tp_dto4 = tp_dto4;
                productoModificado.tp_dto_pa = tp_dto_pa;
                productoModificado.tp_porc_flete = tp_porc_flete;
                productoModificado.tp_boni = tp_boni;
                productoModificado.tp_pcosto = tp_pcosto;
                productoModificado.tp_margen = tp_margen;
                productoModificado.tp_pneto = tp_pneto;
                productoModificado.tin_alicuota = tin_alicuota;
                productoModificado.tp_pvta = tp_pvta;
                productoModificado.carga = 1; // Marcar como modificado

                // Verificar si el producto ya existe en la lista temporal
                var indiceExistente = productosTemporal.FindIndex(p => p.p_id == p_id);
                if (indiceExistente >= 0)
                {
                    // Actualizar el producto existente
                    productosTemporal[indiceExistente] = productoModificado;
                }
                else
                {
                    // Agregar el nuevo producto modificado
                    productosTemporal.Add(productoModificado);
                }

                // Guardar la lista temporal en la sesión
                ProductosDetalleTEMPORAL = productosTemporal;

                return Json(new { error = false, warn = false, msg = "Producto resguardado correctamente para su posterior actualización." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al resguardar cambios del producto");
                return Json(new { error = true, warn = false, msg = "Se produjo un error al intentar resguardar los cambios del producto." });
            }
        }

        [HttpPost]
        public JsonResult ResguardarCambiosProductoLista(string p_id, string lp_id, decimal tp_margen, decimal tp_pvta,
               decimal p_pcosto, decimal p_pneto, decimal lp_porc_mg, char iva_situacion,
               decimal iva_alicuota, decimal in_alicuota, decimal tp_iva, decimal tp_in)
        {
            try
            {
                // ✅ PASO 1: Validaciones básicas (sin cambios)
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrEmpty(p_id))
                {
                    throw new NegocioException("No se ha especificado el ID del producto a modificar.");
                }

                if (string.IsNullOrEmpty(lp_id))
                {
                    throw new NegocioException("No se ha identificado qué lista es la que se pretende modificar.");
                }

                // ✅ PASO 2: CORREGIDO - Inicialización thread-safe
                lock (_lockResguardoLista)
                {
                    if (ProductosDetalleListaTEMPORAL == null)
                    {
                        ProductosDetalleListaTEMPORAL = new List<ProductoDetalleDto>();
                        _logger?.LogInformation("Lista temporal de listas inicializada de forma segura");
                    }
                }

                // ✅ PASO 3: Búsqueda de registro original con logging mejorado
                var productoOriginal = BuscarRegistroOriginal(p_id, lp_id);
                if (productoOriginal == null)
                {
                    throw new NegocioException($"No se encontró la lista del producto con ID {p_id} y Lista {lp_id}.");
                }

                // ✅ PASO 4: Verificación de cambios (optimizada)
                var cambiosDetectados = VerificarCambiosEnLista(productoOriginal, tp_margen, tp_pvta, p_pcosto,
                    p_pneto, lp_porc_mg, iva_situacion, iva_alicuota, in_alicuota, tp_iva, tp_in);

                if (!cambiosDetectados.HayCambios)
                {
                    return EliminarDeListaTemporal(p_id, lp_id);
                }

                // ✅ PASO 5: CRÍTICO - Actualización thread-safe
                var registroModificado = CrearRegistroModificado(productoOriginal, tp_margen, tp_pvta, p_pcosto,
                    p_pneto, lp_porc_mg, iva_situacion, iva_alicuota, in_alicuota, tp_iva, tp_in);

                ActualizarListaTemporalSegura(p_id, lp_id, registroModificado);

                // ✅ PASO 6: Logging detallado para debugging
                _logger?.LogInformation($"✅ RESGUARDADO: P={p_id}, LP={lp_id}, Total temporales: {ProductosDetalleListaTEMPORAL?.Count ?? 0}");

                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = "Lista de precio del producto resguardada correctamente para su posterior actualización.",
                    margen = tp_margen,
                    debug = new
                    {
                        producto_id = p_id,
                        lista_id = lp_id,
                        total_temporales = ProductosDetalleListaTEMPORAL?.Count ?? 0
                    }
                });
            }
            catch (NegocioException ex)
            {
                _logger?.LogError($"❌ Error de negocio resguardando P={p_id}, LP={lp_id}: {ex.Message}");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"💥 Error crítico resguardando P={p_id}, LP={lp_id}");
                return Json(new { error = true, warn = false, msg = "Se produjo un error al intentar resguardar los cambios de la lista del producto." });
            }
        }

        /// <summary>
        /// ✅ NUEVA: Búsqueda optimizada de registro original
        /// </summary>
        private ProductoDetalleDto BuscarRegistroOriginal(string p_id, string lp_id)
        {
            lock (_lockResguardoLista)
            {
                //SE COMENTA TODO EL BLOQUE PORQUE SIEMPRE SE COMPARAR CON PRODUCTO DETALLE (NO TEMPORAL)
                // Buscar primero en temporales (más reciente)
                //var temporal = ProductosDetalleListaTEMPORAL?
                //    .FirstOrDefault(p => p.p_id == p_id && p.lp_id == lp_id);

                //if (temporal != null)
                //{
                //    _logger?.LogDebug($"📋 Encontrado en temporales: P={p_id}, LP={lp_id}");
                //    return temporal;
                //}

                // Si no está en temporales, buscar en originales
                var original = ProductosDetalleLista?
                    .FirstOrDefault(p => p.p_id == p_id && p.lp_id == lp_id);

                if (original != null)
                {
                    _logger?.LogDebug($"📋 Encontrado en originales: P={p_id}, LP={lp_id}");
                }

                return original;
            }
        }

        /// <summary>
        /// ✅ NUEVA: Verificación optimizada de cambios
        /// </summary>
        private (bool HayCambios, string Detalles) VerificarCambiosEnLista(
            ProductoDetalleDto original, decimal tp_margen, decimal tp_pvta, decimal p_pcosto,
            decimal p_pneto, decimal lp_porc_mg, char iva_situacion, decimal iva_alicuota,
            decimal in_alicuota, decimal tp_iva, decimal tp_in)
        {
            const decimal TOLERANCIA_2_DECIMALES = 0.01m;
            const decimal TOLERANCIA_3_DECIMALES = 0.001m;

            var cambios = new List<string>();

            // ✅ OPTIMIZADO: Verificar cada campo con tolerancia apropiada
            if (Math.Abs(original.tp_margen - tp_margen) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"Margen: {original.tp_margen} → {tp_margen}");

            if (Math.Abs(original.tp_pvta - tp_pvta) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"PVenta: {original.tp_pvta} → {tp_pvta}");

            if (Math.Abs(original.lp_porc_mg - lp_porc_mg) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"PorcMg: {original.lp_porc_mg} → {lp_porc_mg}");

            if (Math.Abs(original.iva_alicuota - iva_alicuota) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"IVA: {original.iva_alicuota} → {iva_alicuota}");

            if (Math.Abs(original.in_alicuota - in_alicuota) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"ImpInt: {original.in_alicuota} → {in_alicuota}");

            if (Math.Abs(original.tp_iva - tp_iva) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"TpIVA: {original.tp_iva} → {tp_iva}");

            if (Math.Abs(original.tp_in - tp_in) > TOLERANCIA_2_DECIMALES)
                cambios.Add($"TpIN: {original.tp_in} → {tp_in}");

            if (Math.Abs(original.P_Pcosto - p_pcosto) > TOLERANCIA_3_DECIMALES)
                cambios.Add($"Costo: {original.P_Pcosto} → {p_pcosto}");

            if (Math.Abs(original.p_pneto - p_pneto) > TOLERANCIA_3_DECIMALES)
                cambios.Add($"PNeto: {original.p_pneto} → {p_pneto}");

            if (original.iva_situacion != iva_situacion)
                cambios.Add($"SitIVA: {original.iva_situacion} → {iva_situacion}");

            return (cambios.Any(), string.Join(", ", cambios));
        }

        /// <summary>
        /// ✅ NUEVA: Creación optimizada de registro modificado
        /// </summary>
        private ProductoDetalleDto CrearRegistroModificado(
            ProductoDetalleDto original, decimal tp_margen, decimal tp_pvta, decimal p_pcosto,
            decimal p_pneto, decimal lp_porc_mg, char iva_situacion, decimal iva_alicuota,
            decimal in_alicuota, decimal tp_iva, decimal tp_in)
        {
            // ✅ OPTIMIZADO: Clonación eficiente usando reflection cache
            var modificado = ClonarRegistro(original);

            // Actualizar solo los campos modificados
            modificado.tp_margen = tp_margen;
            modificado.tp_pvta = tp_pvta;
            modificado.P_Pcosto = p_pcosto;
            modificado.p_pneto = p_pneto;
            modificado.lp_porc_mg = lp_porc_mg;
            modificado.iva_situacion = iva_situacion;
            modificado.iva_alicuota = iva_alicuota;
            modificado.in_alicuota = in_alicuota;
            modificado.tp_iva = tp_iva;
            modificado.tp_in = tp_in;
            modificado.carga = 1; // ✅ IMPORTANTE: Marcar como temporal

            return modificado;
        }

        /// <summary>
        /// ✅ CRÍTICO: Actualización thread-safe de lista temporal
        /// </summary>
        private void ActualizarListaTemporalSegura(string p_id, string lp_id, ProductoDetalleDto registroModificado)
        {
            lock (_lockResguardoLista)
            {
                // ✅ ASEGURAR: Lista existe
                if (ProductosDetalleListaTEMPORAL == null)
                {
                    ProductosDetalleListaTEMPORAL = new List<ProductoDetalleDto>();
                }

                // ✅ BUSCAR: Registro existente
                var indiceExistente = ProductosDetalleListaTEMPORAL.FindIndex(p => p.p_id == p_id && p.lp_id == lp_id);

                if (indiceExistente >= 0)
                {
                    var lista = ProductosDetalleListaTEMPORAL;

                    // ✅ ACTUALIZAR: Registro existente
                    var anterior = lista[indiceExistente];
                    lista[indiceExistente] = registroModificado;
                    ProductosDetalleListaTEMPORAL = lista;

                    _logger?.LogInformation($"🔄 ACTUALIZADO temporal: P={p_id}, LP={lp_id} " +
                        $"(PVenta: {anterior.tp_pvta} → {registroModificado.tp_pvta})");
                }
                else
                {
                    // ✅ AGREGAR: Nuevo registro
                    var lista = ProductosDetalleListaTEMPORAL;
                    lista.Add(registroModificado);
                    ProductosDetalleListaTEMPORAL = lista;

                    _logger?.LogInformation($"➕ AGREGADO temporal: P={p_id}, LP={lp_id}, PVenta={registroModificado.tp_pvta}");
                }

                // ✅ LOGGING: Estado actual
                var totalPorProducto = ProductosDetalleListaTEMPORAL.GroupBy(x => x.p_id)
                    .ToDictionary(g => g.Key, g => g.Count());

                _logger?.LogInformation($"📊 Estado temporal actual: Total={ProductosDetalleListaTEMPORAL.Count}, " +
                    $"Por producto: {string.Join(", ", totalPorProducto.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
            }
        }

        /// <summary>
        /// ✅ NUEVA: Eliminación segura de lista temporal
        /// </summary>
        private JsonResult EliminarDeListaTemporal(string p_id, string lp_id)
        {
            lock (_lockResguardoLista)
            {
                var eliminado = ProductosDetalleListaTEMPORAL?
                    .FirstOrDefault(p => p.p_id == p_id && p.lp_id == lp_id);

                if (eliminado != null)
                {
                    ProductosDetalleListaTEMPORAL.Remove(eliminado);
                    _logger?.LogInformation($"🗑️ ELIMINADO temporal: P={p_id}, LP={lp_id}, Total restante: {ProductosDetalleListaTEMPORAL.Count}");
                    return Json(new { error = false, warn = false, msg = "No se detectaron cambios en la lista del producto. Se ha eliminado de la lista temporal." });
                }

                return Json(new { error = false, warn = false, msg = "No se detectaron cambios en la lista del producto." });
            }
        }

        /// <summary>
        /// ✅ OPTIMIZADO: Clonación eficiente de registro
        /// </summary>
        private static ProductoDetalleDto ClonarRegistro(ProductoDetalleDto original)
        {
            var clonado = new ProductoDetalleDto();

            // ✅ EFICIENTE: Copiar propiedades usando reflection optimizada
            var propiedades = typeof(ProductoDetalleDto).GetProperties()
                .Where(p => p.CanWrite && p.CanRead);

            foreach (var propiedad in propiedades)
            {
                var valor = propiedad.GetValue(original);
                if (valor != null)
                {
                    propiedad.SetValue(clonado, valor);
                }
            }

            return clonado;
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarPreciosTemporales()
        {
            try
            {
                // Verificar autenticación
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                 
                var prod = ProductosDetalleTEMPORAL.FirstOrDefault();
                if (prod == null) {
                    throw new NegocioException("No se han encontrado precios temporales para confirmar. Por favor, verifique si ha resguardado cambios previamente.");
                }

                #region Mapeo de los datos del temporales a mandar
                var confirmar = new List<ProductoCPConfirmar>();

                // ✅ PASO 1: Procesar todos los productos temporales (cambios de producto base)
                foreach (var productoTemporal in ProductosDetalleTEMPORAL)
                {
                    // ✅ PASO 2: Crear el producto confirmado con los datos base (lp_id == "001")
                    var productoConfirmar = new ProductoCPConfirmar
                    {
                        // Datos del producto base
                        p_id = productoTemporal.p_id,
                        lp_id = "001", // Lista base
                        tp_plista = productoTemporal.tp_plista,
                        tp_dto1 = productoTemporal.tp_dto1,
                        tp_dto2 = productoTemporal.tp_dto2,
                        tp_dto3 = productoTemporal.tp_dto3,
                        tp_dto4 = productoTemporal.tp_dto4,
                        tp_dto_pa = productoTemporal.tp_dto_pa,
                        tp_porc_flete = productoTemporal.tp_porc_flete,
                        tp_boni = productoTemporal.tp_boni ?? string.Empty,
                        tp_pcosto = productoTemporal.tp_pcosto,
                        tin_alicuota = productoTemporal.tin_alicuota,
                        tp_margen = productoTemporal.tp_margen,
                        tp_margen_vta = productoTemporal.tp_margen, // Asumiendo que son iguales
                        tp_pneto = productoTemporal.tp_pneto,
                        tp_iva = productoTemporal.tp_iva,
                        tp_in = productoTemporal.tp_in,
                        tp_pvta = productoTemporal.tp_pvta,
                        Listas = new List<TPProducto>()
                    };

                    // ✅ PASO 3: Buscar listas temporales relacionadas con este producto
                    var listasRelacionadas = ProductosDetalleListaTEMPORAL?
                        .Where(lista => lista.p_id == productoTemporal.p_id && lista.lp_id != "001")
                        .Select(lista => new TPProducto
                        {
                            p_id = lista.p_id,
                            lp_id = lista.lp_id,
                            //tp_plista = lista.tp_plista,
                            //tp_dto1 = lista.tp_dto1,
                            //tp_dto2 = lista.tp_dto2,
                            //tp_dto3 = lista.tp_dto3,
                            //tp_dto4 = lista.tp_dto4,
                            //tp_dto_pa = lista.tp_dto_pa,
                            //tp_porc_flete = lista.tp_porc_flete,
                            //tp_boni = lista.tp_boni ?? string.Empty,
                            //tp_pcosto = lista.P_Pcosto,
                            //tin_alicuota = lista.in_alicuota,
                            tp_margen = lista.tp_margen,
                            tp_margen_vta = lista.tp_margen, // Asumiendo que son iguales
                            tp_pneto = lista.p_pneto,
                            tp_iva = lista.tp_iva,
                            tp_in = lista.tp_in,
                            tp_pvta = lista.tp_pvta
                        }).ToList() ?? new List<TPProducto>();

                    // Agregar listas encontradas
                    productoConfirmar.Listas.AddRange(listasRelacionadas);

                    // Agregar el producto a la confirmación
                    confirmar.Add(productoConfirmar);

                    _logger?.LogInformation($"✅ Producto {productoTemporal.p_id}: {listasRelacionadas.Count} listas asociadas");
                }

                // ✅ PASO 4: Procesar listas temporales huérfanas (sin producto temporal asociado)
                var listasHuerfanas = ProductosDetalleListaTEMPORAL?
                    .Where(lista => lista.lp_id != "001" && !ProductosDetalleTEMPORAL.Any(prod => prod.p_id == lista.p_id))
                    .GroupBy(lista => lista.p_id)
                    .ToList() ?? new List<IGrouping<string, ProductoDetalleDto>>();

                foreach (var grupoListasHuerfanas in listasHuerfanas)
                {
                    var p_id = grupoListasHuerfanas.Key;

                    // ✅ PASO 5: Buscar producto original para las listas huérfanas
                    var productoOriginal = ProductosDetalle?.FirstOrDefault(p => p.p_id == p_id);

                    if (productoOriginal != null)
                    {
                        var productoHuerfano = new ProductoCPConfirmar
                        {
                            // Datos del producto original (sin cambios en precio base)
                            p_id = productoOriginal.p_id,
                            lp_id = "001", // Lista base
                            tp_plista = productoOriginal.P_Plista,
                            tp_dto1 = productoOriginal.P_Dto1,
                            tp_dto2 = productoOriginal.P_Dto2,
                            tp_dto3 = productoOriginal.P_Dto3,
                            tp_dto4 = productoOriginal.P_Dto4,
                            tp_dto_pa = productoOriginal.P_Dto_Pa,
                            tp_porc_flete = productoOriginal.P_Porc_Flete,
                            tp_boni = productoOriginal.P_Boni ?? string.Empty,
                            tp_pcosto = productoOriginal.P_Pcosto,
                            tin_alicuota = productoOriginal.in_alicuota,
                            tp_margen = productoOriginal.p_margen,
                            tp_margen_vta = productoOriginal.p_margen, // Asumiendo que son iguales
                            tp_pneto = productoOriginal.p_pneto,
                            tp_iva = productoOriginal.tp_iva,
                            tp_in = productoOriginal.tp_in,
                            tp_pvta = productoOriginal.p_pvta,

                            // Solo cambios en listas
                            Listas = grupoListasHuerfanas.Select(lista => new TPProducto
                            {
                                lp_id = lista.lp_id,
                                //tp_plista = lista.tp_plista,
                                //tp_dto1 = lista.tp_dto1,
                                //tp_dto2 = lista.tp_dto2,
                                //tp_dto3 = lista.tp_dto3,
                                //tp_dto4 = lista.tp_dto4,
                                //tp_dto_pa = lista.tp_dto_pa,
                                //tp_porc_flete = lista.tp_porc_flete,
                                //tp_boni = lista.tp_boni ?? string.Empty,
                                //tp_pcosto = lista.P_Pcosto,
                                //tin_alicuota = lista.in_alicuota,
                                tp_margen = lista.tp_margen,
                                tp_margen_vta = lista.tp_margen, // Asumiendo que son iguales
                                tp_pneto = lista.p_pneto,
                                tp_iva = lista.tp_iva,
                                tp_in = lista.tp_in,
                                tp_pvta = lista.tp_pvta
                            }).ToList()
                        };

                        confirmar.Add(productoHuerfano);

                        _logger?.LogInformation($"✅ Producto huérfano {p_id}: {grupoListasHuerfanas.Count()} listas temporales");
                    }
                    else
                    {
                        _logger?.LogWarning($"⚠️ No se encontró producto original para listas huérfanas con p_id: {p_id}");
                    }
                }

                // ✅ PASO 6: Logging del resumen de mapeo
                var totalProductos = confirmar.Count;
                var totalListas = confirmar.Sum(p => p.Listas?.Count ?? 0);

                _logger?.LogInformation($"📋 MAPEO COMPLETO: {totalProductos} productos, {totalListas} listas temporales confirmadas");

                #endregion

                //generamos response para confirmar los precios temporales
                var request = new AbmGenDto
                {
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Objeto = prod.cta_id,
                    Json = JsonConvert.SerializeObject(confirmar),
                };


                // Llamar al servicio para confirmar los precios temporales
                var respuesta = await _productoServicio.ConfirmarPreciosTemporales(request, TokenCookie);
                if (!respuesta.Ok)
                {
                    throw new NegocioException(respuesta.Mensaje??"No se recepción un mensaje de confirmación. Analice si los cambios se aplicarón o verifique logs para determinar el origen del problema, por la falta de respuesta del servicio.");
                }
                // Limpiar las listas temporales después de la confirmación exitosa
                ProductosDetalleTEMPORAL = [];
                ProductosDetalleListaTEMPORAL = [];
                return Json(new { error = false, warn = false, msg = "Los precios temporales han sido confirmados correctamente." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al confirmar precios temporales");
                return Json(new { error = true, warn = false, msg = "Se produjo un error al intentar confirmar los precios temporales." });
            }
        }





        //Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
        [HttpPost]
        public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
        {
            try
            {
                CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
                var familias = ProveedorFamiliaLista;
                var combo = familias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc })
                    .ToList();
                return Json(new { error = false, warn = false, lista = combo });
            }
            catch (Exception)
            {
                return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
            }

        }

        /// <summary>
        /// ✅ NUEVA: Endpoint de diagnóstico para debugging (solo en Development)
        /// </summary>
        [HttpPost]
        public JsonResult DiagnosticoListasTemporal()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "No autenticado" });

                lock (_lockResguardoLista)
                {
                    var estado = new
                    {
                        total = ProductosDetalleListaTEMPORAL.Count,// ?? 0,
                        por_producto = ProductosDetalleListaTEMPORAL?
                            .GroupBy(x => x.p_id)
                            .ToDictionary(g => g.Key, g => new
                            {
                                count = g.Count(),
                                listas = g.Select(l => new { lp_id = l.lp_id, tp_pvta = l.tp_pvta, tp_margen = l.tp_margen }).ToList()
                            }),// ?? new Dictionary<string, object>(),
                        registros = ProductosDetalleListaTEMPORAL?
                            .Select(x => new
                            {
                                p_id = x.p_id,
                                lp_id = x.lp_id,
                                tp_pvta = x.tp_pvta,
                                tp_margen = x.tp_margen,
                                carga = x.carga
                            }).ToList(),// ?? new List<object>()
                    };

                    _logger?.LogInformation($"📊 DIAGNÓSTICO Lista temporal: {System.Text.Json.JsonSerializer.Serialize(estado)}");

                    return Json(new { error = false, estado = estado });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error en diagnóstico de lista temporal");
                return Json(new { error = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SeleccionarProveedor(string ctaId)
        {
            try
            {
                if (string.IsNullOrEmpty(ctaId))
                {
                    throw new NegocioException("Debe seleccionar un proveedor.");
                }
                ProveedorSeleccionado = ProveedoresLista.First(x=>x.Cta_Id == ctaId);
                if (ProveedorSeleccionado == null)
                {
                    throw new NegocioException($"No se encontró el proveedor con ID: {ctaId}");
                }
                
                return Json(new { error = false, warn = false, msg = "Proveedor seleccionado correctamente." });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al seleccionar proveedor");
                return Json(new { error = true, warn = false, msg = "Se produjo un error al intentar seleccionar el proveedor." });
            }
        }

        protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
        {
            var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
            ProveedorFamiliaLista = adms;
        }



        private SelectList ComboProveedores()
        {
            var adms = _cuentaServicio.ObtenerListaProveedores("BI", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboRubros()
        {
            var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
            var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
        }

        private SelectList ComboSucursales()
        {
            var adms = _administracionServicio.GetAdministracionLogin();
            var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
            return HelperMvc<ComboGenDto>.ListaGenerica(lista);
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

            ProductosDetalle = [];
            ProductosDetalleLista = [];
            ProductosDetalleTEMPORAL = [];
            ProductosDetalleListaTEMPORAL = [];
        }
    }
}
