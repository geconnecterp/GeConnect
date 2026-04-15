using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class ClienteController : ControladorBaseCaja
    {
        private readonly ICajaInitServicio _cajaInitSv;
        private readonly ICajaServicio _cajaServicio; // ✅ INYECTAR SERVICIO
        //private readonly ILogger<InicioController> _logger;

        public ClienteController(
            IOptions<AppSettings> options,
            ICajaInitServicio cajaInitSv,
            ICajaServicio cajaServicio, // ✅ AGREGAR
            IHttpContextAccessor httpContext,
            ILogger<InicioController> logger) : base(options, httpContext, logger)
        {
            _cajaInitSv = cajaInitSv;
            _cajaServicio = cajaServicio; // ✅ ASIGNAR
        }

        /// <summary>
        /// Busca un cliente por CUIT, DNI, ID o nombre.
        /// Si encuentra 1 solo resultado, carga datos fiscales completos automáticamente.
        /// Si encuentra múltiples, retorna indicador para mostrar grilla.
        /// </summary>
        /// <param name="criterio">Criterio de búsqueda (CUIT, DNI, ID o nombre)</param>
        /// <returns>JSON con datos del cliente encontrado o indicador de múltiples resultados</returns>
        [HttpPost]
        public async Task<JsonResult> BuscarCliente(string criterio)
        {
            try
            {
                // ❶ VALIDAR CRITERIO
                if (string.IsNullOrWhiteSpace(criterio))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Debe ingresar un criterio de búsqueda válido"
                    });
                }

                // ❷ VALIDAR CAJA
                var cajaActual = CajaActual;
                if (string.IsNullOrEmpty(cajaActual?.CajaId))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se ha configurado una caja para esta estación."
                    });
                }

                // ❸ LLAMAR AL SERVICIO DE BÚSQUEDA INICIAL
                var result = await _cajaServicio.BusquedaClientes(
                    busqueda: criterio.Trim(),
                    adm_id: AdministracionId,
                    usu_id: UserName,
                    token: TokenCookie
                );

                // ❹ VALIDAR RESPUESTA
                if (!result.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = result.Mensaje ?? "Error al buscar el cliente"
                    });
                }

                // ❺ ANALIZAR CANTIDAD DE RESULTADOS
                int cantidadResultados = 0;
                List<CuentaBusquedaResultadoDto> listaClientes = new();

                if (result.ListaEntidad != null && result.ListaEntidad.Count > 0)
                {
                    listaClientes = result.ListaEntidad;
                    cantidadResultados = listaClientes.Count;
                }
                else if (result.Entidad != null && !string.IsNullOrEmpty(result.Entidad.Cta_Id))
                {
                    listaClientes = new List<CuentaBusquedaResultadoDto> { result.Entidad };
                    cantidadResultados = 1;
                }

                // ❻ CASOS SEGÚN CANTIDAD DE RESULTADOS
                
                // ❻.1 - NO SE ENCONTRARON REGISTROS
                if (cantidadResultados == 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = $"No se encontró ningún cliente con el criterio '{criterio}'"
                    });
                }

                // ❻.2 - SE ENCONTRÓ UN ÚNICO CLIENTE → ✅ CARGAR DATOS COMPLETOS
                if (cantidadResultados == 1)
                {
                    var clienteParcial = listaClientes[0];

                    if(clienteParcial.Origen.Equals("N", StringComparison.OrdinalIgnoreCase))
                    {
                        return Json(new
                        {
                            ok = false,
                            mensaje = "Cliente Registrado NO HABILITADO",
                            cantidadResultados = 1,
                            cliente = MapearClienteParcial(clienteParcial)
                        });
                    }

                    
                    // ✅ INVOCAR SERVICIO DE DATOS COMPLETOS
                    var dCompletos = await ObtenerDatosCompletosCliente(
                        clienteParcial,
                        clienteParcial.Cta_Id, 
                        clienteParcial.Cta_Documento
                    );
                    
                    if (!dCompletos.ok)
                    {
                        // Si falla, retornar datos parciales con advertencia
                        _logger?.LogWarning(
                            "No se pudieron cargar datos fiscales del cliente {ClienteId}. Error: {Error}", 
                            clienteParcial.Cta_Id, 
                            dCompletos.mensaje
                        );
                        
                        return Json(new
                        {
                            ok = true,
                            mensaje = "Cliente encontrado (datos parciales)",
                            cantidadResultados = 1,
                            advertencia = "No se pudieron cargar los datos fiscales completos",
                            cliente = MapearClienteParcial(clienteParcial)
                        });
                    }

                    // ✅ NUEVO: GUARDAR CLIENTE COMPLETO EN SESIÓN
                    if (dCompletos.datosCompletos != null)
                    {
                        dCompletos.datosCompletos.Origen = clienteParcial.Origen; // Asegurar que el origen esté presente
                        ClienteActual = dCompletos.datosCompletos;
                                                
                        _logger?.LogInformation(
                            "Cliente guardado en sesión: {ClienteId} - {ClienteNombre} - Origen: {Origen}",
                            dCompletos.datosCompletos.cta_id,
                            dCompletos.datosCompletos.cta_denominacion,
                            clienteParcial.Origen
                        );
                    }
                    
                    // ✅ RETORNAR DATOS COMPLETOS
                    return Json(new
                    {
                        ok = true,
                        mensaje = "Cliente encontrado",
                        cantidadResultados = 1,
                        cliente = dCompletos.cliente
                    });
                }

                // ❻.3 - MÚLTIPLES CLIENTES → GUARDAR EN SESIÓN
                ClientesBuscados = listaClientes;
                
                return Json(new
                {
                    ok = true,
                    mensaje = $"Se encontraron {cantidadResultados} clientes",
                    cantidadResultados
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar cliente. Criterio: {Criterio}", criterio);

                return Json(new
                {
                    ok = false,
                    mensaje = "Error interno al buscar el cliente. Por favor, contacte al administrador.",
                    detalle = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtiene la grilla de clientes múltiples previamente buscados (desde sesión)
        /// Similar al patrón de EtiquetaController.ObtenerDetalleEtiquetas
        /// </summary>
        /// <returns>Vista parcial con GridCoreSmart de clientes</returns>
        [HttpPost]
        public IActionResult TraerGrillaClientes()
        {
            try
            {
                // ❶ Obtener lista desde sesión
                var listaClientes = ClientesBuscados;

                if (listaClientes == null || listaClientes.Count == 0)
                {
                    _logger?.LogWarning("TraerGrillaClientes llamado sin datos en sesión");
                    
                    // Retornar grilla vacía con mensaje
                    var gridVacio = GenerarGrillaSmart(new List<CuentaBusquedaResultadoDto>(), nameof(CuentaBusquedaResultadoDto.Cta_Denominacion));
                    return PartialView("_GrillaClientesMultiples", gridVacio);
                }

                // ❷ Ordenar por denominación para UX consistente
                var ordenada = listaClientes
                    .OrderBy(x => x.Cta_Denominacion, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // ❸ Generar GridCoreSmart centralizado desde la base
                var grid = GenerarGrillaSmart(ordenada, nameof(CuentaBusquedaResultadoDto.Cta_Denominacion));

                // ❹ Retornar vista parcial
                return PartialView("_GrillaClientesMultiples", grid);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al generar grilla declientes múltiples");
                
                // En caso de error, retornar grilla vacía
                var gridError = GenerarGrillaSmart(new List<CuentaBusquedaResultadoDto>(), nameof(CuentaBusquedaResultadoDto.Cta_Denominacion));
                return PartialView("_GrillaClientesMultiples", gridError);
            }
        }

        /// <summary>
        /// ✅ NUEVO (OPCIONAL): Limpia la sesión de ClientesBuscados
        /// 
        /// Esta action permite limpiar la lista de clientes almacenados
        /// en la sesión cuando el usuario hace clic en CANCELAR.
        /// </summary>
        [HttpPost]
        public IActionResult LimpiarSesionClientes()
        {
            try
            {
                // Limpiar la lista de sesión
                ClientesBuscados = new List<CuentaBusquedaResultadoDto>();
                
                _logger?.LogInformation("Sesión de ClientesBuscados limpiada por el usuario");
                
                return Json(new { ok = true, mensaje = "Sesión limpiada" });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al limpiar sesión de clientes");
                return Json(new { ok = false, mensaje = "Error al limpiar sesión" });
            }
        }

        ///// <summary>
        ///// ✅ NUEVA ACTION: Busca cliente por ID y retorna datos fiscales completos.
        ///// Se invoca desde JavaScript cuando el usuario selecciona un cliente de la grilla.
        ///// </summary>
        //[HttpPost]
        //public async Task<JsonResult> BuscarClientePorId(string clienteId, string origen, string documento)
        //{
        //    try
        //    {
        //        // ❶ VALIDAR PARÁMETROS
        //        if (string.IsNullOrWhiteSpace(clienteId))
        //        {
        //            return Json(new { ok = false, mensaje = "ID de cliente inválido" });
        //        }

        //        if (string.IsNullOrWhiteSpace(origen))
        //        {
        //            return Json(new { ok = false, mensaje = "Origen de cliente inválido" });
        //        }

        //        // ❷ OBTENER DATOS COMPLETOS
        //        var resultado = await ObtenerDatosCompletosCliente(origen, clienteId, documento);
                
        //        if (!resultado.ok)
        //        {
        //            return Json(new { ok = false, mensaje = resultado.mensaje });
        //        }

        //        // ❸ RETORNAR DATOS COMPLETOS
        //        return Json(new
        //        {
        //            ok = true,
        //            mensaje = "Cliente encontrado",
        //            cantidadResultados = 1,
        //            cliente = resultado.cliente
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger?.LogError(ex, "Error al buscar cliente por ID: {ClienteId}", clienteId);
                
        //        return Json(new
        //        {
        //            ok = false,
        //            mensaje = "Error al cargar datos del cliente"
        //        });
        //    }
        //}

        /// <summary>
        /// ✅ MÉTODO PRIVADO: Obtiene datos fiscales y comerciales completos del cliente.
        /// Reutilizable desde BuscarCliente (1 resultado) y BuscarClientePorId (desde grilla).
        /// </summary>
        private async Task<(bool ok, string mensaje, object? cliente, CuentaDatosResultadoDto? datosCompletos)> ObtenerDatosCompletosCliente(
            CuentaBusquedaResultadoDto cuenta,            
            string clienteId, 
            string? numeroDocumento)
        {
            try
            {
                // ❶ Determinar valor de búsqueda según origen
                // Si es Cuenta Registrada → usar ID
                // Si es Consumidor Final → usar Documento
                string valorBusqueda = cuenta.Origen.Equals("C", StringComparison.OrdinalIgnoreCase) 
                    ? clienteId 
                    : numeroDocumento ?? clienteId;

                // ❷ Invocar servicio de datos completos
                var resultadoDatos = await _cajaServicio.BusquedaDatosCliente(
                    origen: cuenta.Origen,
                    valor: valorBusqueda,
                    adm_id: AdministracionId,
                    usu_id: UserName,
                    token: TokenCookie
                );

                // ❸ Validar respuesta
                if (resultadoDatos == null || !resultadoDatos.Ok || resultadoDatos.Entidad == null)
                {
                    return (false, resultadoDatos?.Mensaje ?? "No se pudieron obtener los datos del cliente", null, null);
                }

                var datos = resultadoDatos.Entidad;

                // ❹ Mapear a objeto de respuesta con TODOS los datos (para frontend)
                var clienteCompleto = new
                {
                    // Datos básicos
                    id = datos.cta_id,
                    nombre = datos.cta_denominacion ?? string.Empty,
                    domicilio = datos.cta_domicilio ?? string.Empty,
                    
                    // ✅ Tipo de documento separado
                    tdocId = cuenta.Tdoc_Id ?? string.Empty,
                    tdocDesc = datos.tdoc_desc ?? string.Empty,
                    documento = datos.cta_documento ?? string.Empty,
                    
                    // ✅ Retrocompatibilidad
                    tipoNumero = $"{datos.tdoc_desc ?? ""} {datos.cta_documento ?? ""}".Trim(),
                    
                    email = datos.cta_email ?? string.Empty,
                    movil = datos.cta_celu ?? string.Empty,
                    origen = cuenta.Origen,
                    origenDesc = cuenta.Origen_Desc,

                    // ✅ Datos fiscales
                    condicionAfip = datos.afip_desc ?? string.Empty,
                    condicionAfipId = datos.afip_id ?? string.Empty,
                    emite = $"Factura {datos.tco_letra}",
                    emiteId = datos.tco_letra,
                };

                // ✅ RETORNAR: objeto para frontend + DTO completo para sesión
                return (true, "OK", clienteCompleto, datos);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener datos completos del cliente {ClienteId}", clienteId);
                return (false, "Error al procesar datos del cliente", null, null);
            }
        }

        /// <summary>
        /// ✅ MÉTODO AUXILIAR: Mapea datos parciales cuando no se pueden obtener datos completos
        /// </summary>
        private object MapearClienteParcial(CuentaBusquedaResultadoDto cliente)
        {
            return new
            {
                id = cliente.Cta_Id,
                nombre = cliente.Cta_Denominacion ?? string.Empty,
                domicilio = cliente.Cta_Domicilio ?? string.Empty,
                tipoNumero = $"{cliente.Tdoc_Desc ?? ""} {cliente.Cta_Documento ?? ""}".Trim(),
                email = cliente.Cta_Email ?? string.Empty,
                movil = cliente.Cta_Celu ?? string.Empty,
                origen = cliente.Origen,
                origenDesc = cliente.Origen_Desc,
                
                // Datos fiscales vacíos (no disponibles)
                condicionAfip = string.Empty,
                condicionAfipId = string.Empty,
                emite = string.Empty,
                emiteId = string.Empty,
                listaPrecio = string.Empty,
                listaPrecioId = string.Empty,
                condicionVenta = string.Empty,
                condicionVentaId = string.Empty
            };
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Obtiene el cliente actualmente seleccionado desde sesión
        /// Se invoca desde JavaScript para cargar datos en modal de edición
        /// </summary>
        [HttpPost]
        public IActionResult ObtenerClienteActual()
        {
            try
            {
                var clienteActual = ClienteActual;

                if (clienteActual == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No hay cliente seleccionado en sesión"
                    });
                }

                // Mapear datos para el frontend
                var clienteParaEdicion = new
                {
                    id = clienteActual.cta_id,
                    tipoDocumento = clienteActual.tdoc_id ?? string.Empty,
                    numeroDocumento = clienteActual.cta_documento ?? string.Empty,
                    nombre = clienteActual.cta_denominacion ?? string.Empty,
                    domicilio = clienteActual.cta_domicilio ?? string.Empty,
                    email = clienteActual.cta_email ?? string.Empty,
                    movil = clienteActual.cta_celu ?? string.Empty,
                    origen = clienteActual.Origen ?? string.Empty,
                    
                    // Datos adicionales para mostrar
                    tdocDesc = clienteActual.tdoc_desc ?? string.Empty,
                    condicionAfip = clienteActual.afip_desc ?? string.Empty,
                    emite = $"Factura {clienteActual.tco_letra ?? ""}"
                };

                _logger?.LogInformation(
                    "Cliente actual cargado desde sesión: {ClienteId} - {ClienteNombre}",
                    clienteActual.cta_id,
                    clienteActual.cta_denominacion
                );

                return Json(new
                {
                    ok = true,
                    cliente = clienteParaEdicion
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al obtener cliente actual desde sesión");
                return Json(new
                {
                    ok = false,
                    mensaje = "Error al recuperar datos del cliente"
                });
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Actualiza datos del Consumidor Final
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ActualizarConsumidorFinal(
            string id,
            string tipoDocumento,
            string numeroDocumento,
            string nombre,
            string domicilio,
            string email,
            string movil)
        {
            try
            {
                // ❶ VALIDAR PARÁMETROS OBLIGATORIOS
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new { ok = false, mensaje = "ID de cliente requerido" });
                }

                if (string.IsNullOrWhiteSpace(tipoDocumento))
                {
                    return Json(new { ok = false, mensaje = "Tipo de documento requerido" });
                }

                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return Json(new { ok = false, mensaje = "Número de documento requerido" });
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return Json(new { ok = false, mensaje = "Nombre requerido" });
                }

                // ❷ VERIFICAR QUE EL CLIENTE EN SESIÓN SEA CONSUMIDOR FINAL
                var clienteActual = ClienteActual;

                if (clienteActual == null || clienteActual.Origen?.ToUpper() != "F")
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Solo se pueden actualizar Consumidores Finales"
                    });
                }

                // ❸ TODO: LLAMAR AL SERVICIO DE ACTUALIZACIÓN
                // Aquí iría la llamada al servicio que actualiza el cliente
                // var resultado = await _cajaServicio.ActualizarConsumidorFinal(...);

                _logger?.LogInformation(
                    "Consumidor Final actualizado: {ClienteId} - {ClienteNombre}",
                    id, nombre
                );

                // ❹ ACTUALIZAR CLIENTE EN SESIÓN
                clienteActual.cta_denominacion = nombre.ToUpper();
                clienteActual.cta_domicilio = domicilio?.ToUpper();
                clienteActual.cta_email = email?.ToLower();
                clienteActual.cta_celu = movil;
                
                ClienteActual = clienteActual;

                return Json(new
                {
                    ok = true,
                    mensaje = "Consumidor Final actualizado correctamente"
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al actualizar Consumidor Final: {ClienteId}", id);
                return Json(new
                {
                    ok = false,
                    mensaje = "Error al actualizar el cliente"
                });
            }
        }
    }
}
