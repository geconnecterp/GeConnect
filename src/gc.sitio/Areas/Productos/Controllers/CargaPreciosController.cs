using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.Extensions.Options;

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
                var respuesta = await _productoServicio.Obtener_ProductoDetalleListas(filtro, TokenCookie);

                if (!respuesta.Ok || respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    response.Mensaje = "No se encontraron las listas del producto especificado.";
                    response.Ok = false;
                    response.EsWarn = true;
                    response.EsError = false;
                    return PartialView("_gridMensaje", response);
                }
                var lista = respuesta.ListaEntidad.Where(x => !x.lp_id.Equals("001")).OrderBy(x => x.pg_id).ThenBy(x => x.p_id).ToList();
                // Guardar datos en variable de sesión para uso posterior
                ProductosDetalleLista = lista;

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
                return PartialView("_gridProdLista", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener el detalle de listas");

                response.Mensaje = "Error al obtener el detalle de listas.";
                response.Ok = false;
                response.EsWarn = false;
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

                if (tp_plista <= 0)
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
                if (tp_pcosto <= 0)
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
                #region Validaciones
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

                if (string.IsNullOrEmpty(lp_id))
                {
                    throw new NegocioException("No se ha identificado qué lista es la que se pretende modificar.");
                }
                #endregion

                // Obtener los productos desde la sesión
                var listaProductosOriginales = ProductosDetalleLista;
                var listaProductosTemporal = ProductosDetalleListaTEMPORAL;

                // Buscar el producto original por su ID y lista ID
                var productoOriginal = listaProductosOriginales.FirstOrDefault(p => p.p_id == p_id && p.lp_id == lp_id);
                if (productoOriginal == null)
                {
                    throw new NegocioException($"No se encontró la lista del producto con ID {p_id} y Lista {lp_id}.");
                }

                // Verificar si hay cambios en los valores que afectan el precio
                bool hayCambios =
                    Math.Round(productoOriginal.tp_margen, 2) != Math.Round(tp_margen, 2) ||
                    Math.Round(productoOriginal.tp_pvta, 2) != Math.Round(tp_pvta, 2) ||
                    Math.Round(productoOriginal.P_Pcosto, 3) != Math.Round(p_pcosto, 3) ||
                    Math.Round(productoOriginal.p_pneto, 3) != Math.Round(p_pneto, 3) ||
                    Math.Round(productoOriginal.lp_porc_mg, 2) != Math.Round(lp_porc_mg, 2) ||
                    productoOriginal.iva_situacion != iva_situacion ||
                    Math.Round(productoOriginal.iva_alicuota, 2) != Math.Round(iva_alicuota, 2) ||
                    Math.Round(productoOriginal.in_alicuota, 2) != Math.Round(in_alicuota, 2) ||
                    Math.Round(productoOriginal.tp_iva, 2) != Math.Round(tp_iva, 2) ||
                    Math.Round(productoOriginal.tp_in, 2) != Math.Round(tp_in, 2);

                // Si no hay cambios, verificar si el producto está en la lista temporal y eliminarlo
                if (!hayCambios)
                {
                    // Eliminar de la lista temporal si existe
                    var productoTemporalExistente = listaProductosTemporal.FirstOrDefault(p => p.p_id == p_id && p.lp_id == lp_id);
                    if (productoTemporalExistente != null)
                    {
                        listaProductosTemporal.Remove(productoTemporalExistente);
                        ProductosDetalleListaTEMPORAL = listaProductosTemporal; // Guardar en sesión
                        return Json(new { error = false, warn = false, msg = "No se detectaron cambios en la lista del producto. Se ha eliminado de la lista temporal." });
                    }

                    return Json(new { error = false, warn = false, msg = "No se detectaron cambios en la lista del producto." });
                }

                // Crear una copia del producto original con los valores actualizados
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
                productoModificado.tp_margen = tp_margen;
                productoModificado.tp_pvta = tp_pvta;
                productoModificado.P_Pcosto = p_pcosto;
                productoModificado.p_pneto = p_pneto;
                productoModificado.lp_porc_mg = lp_porc_mg;
                productoModificado.iva_situacion = iva_situacion;
                productoModificado.iva_alicuota = iva_alicuota;
                productoModificado.in_alicuota = in_alicuota;
                productoModificado.tp_iva = tp_iva;
                productoModificado.tp_in = tp_in;
                productoModificado.carga = 1; // Marcar como modificado

                // Verificar si el producto ya existe en la lista temporal
                var indiceExistente = listaProductosTemporal.FindIndex(p => p.p_id == p_id && p.lp_id == lp_id);
                if (indiceExistente >= 0)
                {
                    // Actualizar el producto existente
                    listaProductosTemporal[indiceExistente] = productoModificado;
                }
                else
                {
                    // Agregar el nuevo producto modificado
                    listaProductosTemporal.Add(productoModificado);
                }

                // Guardar la lista temporal en la sesión
                ProductosDetalleListaTEMPORAL = listaProductosTemporal;

                return Json(new { error = false, warn = false, msg = "Lista de precio del producto resguardada correctamente para su posterior actualización." });
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
                _logger?.LogError(ex, "Error al resguardar cambios de la lista del producto");
                return Json(new { error = true, warn = false, msg = "Se produjo un error al intentar resguardar los cambios de la lista del producto." });
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
        }
    }
}
