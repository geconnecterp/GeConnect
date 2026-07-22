using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
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
        public async Task<JsonResult> BuscarCliente(string criterio, string app = "FV")
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

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
                    app: app,
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
                    ProductosSeleccionados = []; // Limpiar productos seleccionados al cargar nuevo cliente
                    var clienteParcial = listaClientes[0];

                    if (clienteParcial.Origen.Equals("N", StringComparison.OrdinalIgnoreCase) ||
                        clienteParcial.Origen.Equals("Q", StringComparison.OrdinalIgnoreCase))
                    {
                        var mensajeNoHabilitado = clienteParcial.Origen.Equals("Q", StringComparison.OrdinalIgnoreCase)
                            ? "Proveedor NO HABILITADO"
                            : "Cliente Registrado NO HABILITADO";

                        return Json(new
                        {
                            ok = false,
                            mensaje = mensajeNoHabilitado,
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
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });


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
        /// Busca datos completos para una cuenta seleccionada desde la grilla.
        /// Preserva el origen elegido por el operador para evitar que una nueva busqueda general
        /// resuelva otro registro con el mismo documento.
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> BuscarClientePorId(string clienteId, string origen, string documento)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });

                var origenNormalizado = origen?.Trim().ToUpperInvariant() ?? string.Empty;
                var clienteIdNormalizado = clienteId?.Trim() ?? string.Empty;
                var documentoNormalizado = documento?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(origenNormalizado))
                    return Json(new { ok = false, mensaje = "Origen de cliente inválido" });

                if (string.IsNullOrWhiteSpace(clienteIdNormalizado) && string.IsNullOrWhiteSpace(documentoNormalizado))
                    return Json(new { ok = false, mensaje = "ID de cliente inválido" });

                var cuentaSeleccionada = ClientesBuscados?.FirstOrDefault(x =>
                    string.Equals(x.Origen?.Trim(), origenNormalizado, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrWhiteSpace(clienteIdNormalizado) || string.Equals(x.Cta_Id?.Trim(), clienteIdNormalizado, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrWhiteSpace(documentoNormalizado) || string.Equals(x.Cta_Documento?.Trim(), documentoNormalizado, StringComparison.OrdinalIgnoreCase)));

                if (cuentaSeleccionada == null)
                {
                    _logger?.LogWarning(
                        "BuscarClientePorId no encontro la cuenta seleccionada en sesion. ClienteId={ClienteId}, Documento={Documento}, Origen={Origen}. Se usaran datos enviados desde la grilla.",
                        clienteIdNormalizado,
                        documentoNormalizado,
                        origenNormalizado);

                    cuentaSeleccionada = new CuentaBusquedaResultadoDto
                    {
                        Cta_Id = clienteIdNormalizado,
                        Cta_Documento = documentoNormalizado,
                        Origen = origenNormalizado
                    };
                }

                if (origenNormalizado is "N" or "Q")
                {
                    var mensajeNoHabilitado = origenNormalizado == "Q"
                        ? "Proveedor NO HABILITADO"
                        : "Cliente Registrado NO HABILITADO";

                    return Json(new
                    {
                        ok = false,
                        mensaje = mensajeNoHabilitado,
                        cantidadResultados = 1,
                        cliente = MapearClienteParcial(cuentaSeleccionada)
                    });
                }

                var resultado = await ObtenerDatosCompletosCliente(
                    cuentaSeleccionada,
                    cuentaSeleccionada.Cta_Id,
                    cuentaSeleccionada.Cta_Documento);

                if (!resultado.ok)
                {
                    _logger?.LogWarning(
                        "No se pudieron cargar datos completos desde seleccion de grilla. ClienteId={ClienteId}, Documento={Documento}, Origen={Origen}, Error={Error}",
                        clienteIdNormalizado,
                        documentoNormalizado,
                        origenNormalizado,
                        resultado.mensaje);

                    return Json(new { ok = false, mensaje = resultado.mensaje });
                }

                if (resultado.datosCompletos != null)
                {
                    resultado.datosCompletos.Origen = origenNormalizado;
                    ClienteActual = resultado.datosCompletos;
                }

                return Json(new
                {
                    ok = true,
                    mensaje = "Cliente encontrado",
                    cantidadResultados = 1,
                    cliente = resultado.cliente
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al buscar cliente por ID desde grilla: {ClienteId}", clienteId);
                return Json(new { ok = false, mensaje = "Error al cargar datos del cliente" });
            }
        }
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
                var origenParaBusqueda = cuenta.Origen?.Trim() ?? string.Empty;
                string valorBusqueda = string.Equals(origenParaBusqueda, "C", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(origenParaBusqueda, "P", StringComparison.OrdinalIgnoreCase)
                    ? clienteId
                    : numeroDocumento ?? clienteId;

                // ❷ Invocar servicio de datos completos
                var resultadoDatos = await _cajaServicio.BusquedaDatosCliente(
                    origen: origenParaBusqueda,
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

                // Resguardar la lista de precios del cliente. Si el SP no informa una,
                // se usa la configuracion inicial de caja segun el origen de la cuenta.
                var origenCuenta = !string.IsNullOrWhiteSpace(origenParaBusqueda)
                    ? origenParaBusqueda.Trim().ToUpperInvariant()
                    : datos.Origen?.Trim().ToUpperInvariant() ?? string.Empty;

                var cajaActual = CajaActual;
                // En la caja, lp_id_min corresponde a la lista mayorista y lp_id_may a la minorista.
                // La decision por defecto se toma por origen: CF -> minorista, CR/proveedor -> mayorista.
                var listaPrecioMayoristaDefault = cajaActual?.Caja?.lp_id_min?.Trim() ?? string.Empty;
                var listaPrecioMinoristaDefault = cajaActual?.Caja?.lp_id_may?.Trim() ?? string.Empty;
                var listaPrecioDesdeCliente = datos.lp_id?.Trim() ?? string.Empty;
                if (string.Equals(listaPrecioDesdeCliente, "NULL", StringComparison.OrdinalIgnoreCase))
                {
                    listaPrecioDesdeCliente = string.Empty;
                }

                var listaPrecioPredeterminada = listaPrecioDesdeCliente;
                var origenListaPrecio = "SP";

                if (string.IsNullOrWhiteSpace(listaPrecioPredeterminada))
                {
                    listaPrecioPredeterminada = origenCuenta switch
                    {
                        "F" => listaPrecioMinoristaDefault,
                        "C" or "P" => listaPrecioMayoristaDefault,
                        _ => string.Empty
                    };

                    origenListaPrecio = origenCuenta switch
                    {
                        "F" => "FALLBACK_CAJA_MINORISTA_CF",
                        "C" or "P" => "FALLBACK_CAJA_MAYORISTA_CR",
                        _ => "SIN_FALLBACK"
                    };
                }

                if (string.IsNullOrWhiteSpace(listaPrecioPredeterminada))
                {
                    _logger?.LogError(
                        "El cliente {ClienteId} no devolvio lp_id y no se pudo resolver fallback. Origen={Origen}, LP_SP='{LP_SP}', LP_MayoristaDefault={LPMayorista}, LP_MinoristaDefault={LPMinorista}.",
                        datos.cta_id,
                        origenCuenta,
                        datos.lp_id,
                        listaPrecioMayoristaDefault,
                        listaPrecioMinoristaDefault);

                    return (
                        false,
                        "No se pudo determinar la lista de precios predeterminada del cliente.",
                        null,
                        null);
                }

                _logger?.LogInformation(
                    "Lista de precios resuelta para cliente {ClienteId}. OrigenCuenta={Origen}, LP_SP='{LP_SP}', LP_Final={LPFinal}, Fuente={Fuente}, LP_MayoristaDefault={LPMayorista}, LP_MinoristaDefault={LPMinorista}.",
                    datos.cta_id,
                    origenCuenta,
                    datos.lp_id,
                    listaPrecioPredeterminada,
                    origenListaPrecio,
                    listaPrecioMayoristaDefault,
                    listaPrecioMinoristaDefault);

                datos.Origen = origenCuenta;
                datos.lp_id = listaPrecioPredeterminada;
                LP_Id = listaPrecioPredeterminada;

                string[] nombre = datos.cta_denominacion
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries);

                // ✅ NUEVO: Validación de CUIT para clientes registrados
                bool requiereCuit = origenCuenta == "C" && datos.tdoc_id != "80";

                // ❹ Mapear a objeto de respuesta con TODOS los datos (para frontend)
                var clienteCompleto = new
                {
                    // Datos básicos
                    id = datos.cta_id,
                    apellido = nombre[0] ?? string.Empty,
                    nombre = string.Join(" ", nombre.Skip(1)) ?? string.Empty,
                    denominacion = datos.cta_denominacion,
                    domicilio = datos.cta_domicilio ?? string.Empty,

                    // ✅ Tipo de documento separado
                    tdocId = cuenta.Tdoc_Id ?? string.Empty,
                    tdocDesc = datos.tdoc_desc ?? string.Empty,
                    documento = datos.cta_documento ?? string.Empty,

                    // ✅ Retrocompatibilidad
                    tipoNumero = $"{datos.tdoc_desc ?? ""} {datos.cta_documento ?? ""}".Trim(),

                    email = datos.cta_email ?? string.Empty,
                    movil = datos.cta_celu ?? string.Empty,
                    origen = origenCuenta,
                    origenDesc = cuenta.Origen_Desc,
                    lp_id = listaPrecioPredeterminada,
                    listaPrecio = listaPrecioPredeterminada,

                    // ✅ Datos fiscales
                    condicionAfip = datos.afip_desc ?? string.Empty,
                    condicionAfipId = datos.afip_id ?? string.Empty,
                    emite = $"Factura {datos.tco_letra}",
                    emiteId = datos.tco_letra,
                    // ✅ NUEVO: Indicador de requisito de CUIT
                    requiereCuit
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
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                var clienteActual = ClienteActual;

                if (clienteActual == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No hay cliente seleccionado en sesión"
                    });
                }

                string[] nombre = clienteActual.cta_denominacion
                                .Split([' '], StringSplitOptions.RemoveEmptyEntries);

                // Mapear datos para el frontend
                var clienteParaEdicion = new
                {
                    id = clienteActual.cta_id,
                    tipoDocumento = clienteActual.tdoc_id ?? string.Empty,
                    numeroDocumento = clienteActual.cta_documento ?? string.Empty,
                    apellido = nombre[0] ?? string.Empty,
                    nombre = string.Join(" ", nombre.Skip(1)) ?? string.Empty,
                    domicilio = clienteActual.cta_domicilio ?? string.Empty,
                    email = clienteActual.cta_email ?? string.Empty,
                    movil = clienteActual.cta_celu ?? string.Empty,
                    origen = clienteActual.Origen ?? string.Empty,
                    sexo = clienteActual.cta_sexo,

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
        /// ✅ ACTUALIZADO v2.0: Actualiza datos del Consumidor Final
        /// 
        /// CAMBIOS v2.0:
        /// - Agregado parámetro apellido
        /// - Agregado parámetro sexo
        /// - Integrado con servicio ConfirmaConsumidorFinal
        /// - Manejo completo de respuestas del backend
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> ActualizarConsumidorFinal(
            string id,          // ← Vacío si ABM=A, con valor si ABM=M
            string abm,
            string apellido,    // ← ✅ NUEVO en v2.0
            string nombre,      // ← ✅ NUEVO en v2.0
            string sexo,        // ← ✅ NUEVO en v2.0
            string tipoDocumento,
            string numeroDocumento,
            string domicilio,
            string email,
            string movil)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { ok = false, resultado = -1, mensaje = "Sesión expirada" });


                // ═══════════════════════════════════════════════════════════
                // ❶ VALIDAR PARÁMETROS OBLIGATORIOS
                // ═══════════════════════════════════════════════════════════

                if (string.IsNullOrWhiteSpace(tipoDocumento))
                {
                    return Json(new { ok = false, mensaje = "Tipo de documento requerido" });
                }

                if (string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return Json(new { ok = false, mensaje = "Número de documento requerido" });
                }

                if (string.IsNullOrWhiteSpace(apellido))
                {
                    return Json(new { ok = false, mensaje = "Apellido requerido" });
                }

                if (string.IsNullOrWhiteSpace(nombre))
                {
                    return Json(new { ok = false, mensaje = "Nombre requerido" });
                }

                if (string.IsNullOrWhiteSpace(sexo))
                {
                    return Json(new { ok = false, mensaje = "Sexo requerido" });
                }

                // ═══════════════════════════════════════════════════════════
                // ❷ VERIFICAR CLIENTE EN SESIÓN
                // ═══════════════════════════════════════════════════════════

                var clienteActual = ClienteActual;

                if (clienteActual == null)
                {
                    //significa que es un cliente nuevo.                    
                    clienteActual = new();
                    clienteActual.Origen = "F";
                }

                if (clienteActual.Origen?.ToUpper() != "F")
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "Solo se pueden actualizar Consumidores Finales"
                    });
                }

                // ═══════════════════════════════════════════════════════════
                // ❸ CONSTRUIR DTO CON DATOS DEL FORMULARIO
                // ═══════════════════════════════════════════════════════════

                var request = new ClienteRequestDto
                {
                    // ✅ ACTUALIZADO: Usar parámetro explícito con validación
                    Abm = !string.IsNullOrWhiteSpace(abm) && abm.ToUpper() == "M"
                            ? "M"
                            : "A",

                    // ✅ DATOS DE USUARIO (desde sesión)
                    UsuId = UserName,
                    AdmId = AdministracionId,

                    // ✅ DATOS DEL DOCUMENTO
                    TdocId = tipoDocumento,
                    CtaDocumento = numeroDocumento.Trim(),

                    // ✅ DATOS PERSONALES
                    CtaApellido = apellido.Trim().ToUpper(),
                    CtaNombre = nombre.Trim().ToUpper(),
                    Sexo = sexo.ToUpper(),

                    // ✅ DATOS DE CONTACTO (opcionales)
                    CtaDomicilio = domicilio?.Trim().ToUpper() ?? string.Empty,
                    CtaEmail = email?.Trim().ToLower() ?? string.Empty,
                    CtaCelu = movil?.Trim() ?? string.Empty
                };

                _logger?.LogInformation(
                    "Actualizando Consumidor Final - Modo: {Modo}, Doc: {TipoDoc} {NumDoc}, Nombre: {Apellido}, {Nombre}",
                    request.Abm, request.TdocId, request.CtaDocumento, request.CtaApellido, request.CtaNombre
                );

                // ═══════════════════════════════════════════════════════════
                // ❹ LLAMAR AL SERVICIO BACKEND
                // ═══════════════════════════════════════════════════════════

                var tokenAcceso = TokenCookie;

                if (string.IsNullOrEmpty(tokenAcceso))
                {
                    _logger?.LogError("Token de acceso no disponible");
                    return Json(new { ok = false, mensaje = "Error de autenticación" });
                }

                var resultado = await _cajaServicio.ConfirmaConsumidorFinal(request, tokenAcceso);

                // ═══════════════════════════════════════════════════════════
                // ❺ PROCESAR RESPUESTA DEL BACKEND
                // ═══════════════════════════════════════════════════════════

                if (!resultado.Ok)
                {
                    _logger?.LogWarning(
                        "Error al actualizar CF: {Mensaje}",
                        resultado.Mensaje
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado.Mensaje ?? "Error al actualizar el cliente"
                    });
                }

                // ═══════════════════════════════════════════════════════════
                // ❻ ACTUALIZAR CLIENTE EN SESIÓN
                // ═══════════════════════════════════════════════════════════

                // Construir nombre completo (Apellido, Nombre)
                var nombreCompleto = $"{apellido.Trim().ToUpper()}, {nombre.Trim().ToUpper()}";

                clienteActual.cta_denominacion = nombreCompleto;
                clienteActual.cta_domicilio = domicilio?.Trim().ToUpper() ?? "";
                clienteActual.cta_email = email?.Trim().ToLower() ?? "";
                clienteActual.cta_celu = movil?.Trim() ?? "";
                clienteActual.cta_sexo = sexo;

                // Actualizar datos de documento
                clienteActual.tdoc_id = tipoDocumento;
                clienteActual.cta_documento = numeroDocumento.Trim();

                ClienteActual = clienteActual;

                _logger?.LogInformation(
                    "Consumidor Final actualizado exitosamente: {ClienteNombre}",
                    nombreCompleto
                );

                // ═══════════════════════════════════════════════════════════
                // ❼ RETORNAR RESPUESTA EXITOSA
                // ═══════════════════════════════════════════════════════════

                return Json(new
                {
                    ok = true,
                    mensaje = "Consumidor Final actualizado correctamente",
                    cliente = new
                    {
                        id = clienteActual.cta_id,
                        nombre = nombreCompleto,
                        apellido = apellido.Trim().ToUpper(),
                        nombreSolo = nombre.Trim().ToUpper(),
                        sexo = sexo.ToUpper(),
                        tipoDocumento = tipoDocumento,
                        numeroDocumento = numeroDocumento.Trim(),
                        domicilio = clienteActual.cta_domicilio,
                        email = clienteActual.cta_email,
                        movil = clienteActual.cta_celu
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error inesperado al actualizar Consumidor Final - Doc: {TipoDoc} {NumDoc}",
                    tipoDocumento, numeroDocumento
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Error inesperado al actualizar el cliente. Por favor, intente nuevamente."
                });
            }
        }
    }
}






