using gc.caja.Controllers;
using gc.caja.Models;
using gc.caja.core.Autorizaciones;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Productos.Precio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace gc.caja.Areas.Facturacion.Controllers;

[Area("Facturacion")]
[Authorize]
public sealed class ListaPrecioController : ControladorBaseCaja
{
    private const int DerechoCambioListaPrecio = 6;
    private const string ClaveOperacion = "FACTURACION_CAMBIO_LP";

    private readonly IProductoFactServicio _productoFactServicio;
    private readonly IAutorizacionRemotaOrquestador _orquestador;
    private readonly CambioListaPrecioOptions _cambioOptions;

    public ListaPrecioController(
        IOptions<AppSettings> options,
        IHttpContextAccessor contexto,
        ILogger<ListaPrecioController> logger,
        IProductoFactServicio productoFactServicio,
        IAutorizacionRemotaOrquestador orquestador,
        IOptions<CambioListaPrecioOptions> cambioOptions)
        : base(options, contexto, logger)
    {
        _productoFactServicio = productoFactServicio;
        _orquestador = orquestador;
        _cambioOptions = cambioOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> SolicitarCambio(
        [FromBody] SolicitarCambioListaPrecioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = ValidarOperacionInicial();
            if (error is not null)
            {
                return error;
            }

            var lpSolicitadaId = request?.LpId?.Trim() ?? string.Empty;
            var lpActualId = LP_Id.Trim();
            if (string.IsNullOrWhiteSpace(lpSolicitadaId))
            {
                return BadRequest(new { ok = false, mensaje = "Debe seleccionar una lista de precios." });
            }

            if (string.Equals(lpSolicitadaId, lpActualId, StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { ok = false, mensaje = "La lista seleccionada ya está activa." });
            }

            var listas = await ObtenerCatalogoAsync(cancellationToken);
            var actual = BuscarLista(listas, lpActualId)
                ?? throw new InvalidOperationException("La lista de precios activa no existe en el catálogo.");
            var solicitada = BuscarLista(listas, lpSolicitadaId);
            if (solicitada is null)
            {
                return BadRequest(new { ok = false, mensaje = "La lista de precios seleccionada no existe." });
            }

            var caja = CajaActual;
            var cliente = ClienteActual!;
            var coTipo = DeterminarCoTipo(cliente.Origen);
            var cierre = FormatearCierre(caja.Caja.caja_nro_cierre);
            var idExterno = $"{caja.CajaId}.{caja.Caja.caja_nro_proceso}.{cierre}.{DateTime.Now.Ticks}";

            var contexto = new JObject
            {
                ["tipoAutorizacion"] = "LP",
                ["aplicacion"] = "gc.caja",
                ["modulo"] = "FACTURACION",
                ["coTipo"] = coTipo,
                ["cajaId"] = caja.CajaId,
                ["cajaNroProceso"] = caja.Caja.caja_nro_proceso,
                ["cajaNroCierre"] = cierre,
                ["cta_id"] = cliente.cta_id,
                ["origen"] = cliente.Origen?.ToUpperInvariant(),
                ["listaPrecioActual"] = CrearListaContexto(actual),
                ["listaPrecioSolicitada"] = CrearListaContexto(solicitada)
            };

            var solicitud = new CrearAutorizacionRemotaSolicitud
            {
                UsuarioSolicitante = UserName,
                IdSolicitudExterna = idExterno,
                DerCodigo = DerechoCambioListaPrecio,
                TimeoutSegundos = _cambioOptions.TimeoutSegundos,
                DecisionPorDefecto = "RECHAZADO",
                CodigoResolucionPorDefecto = "LP_TIMEOUT",
                MensajeResolucionPorDefecto = "La solicitud de cambio de lista de precios expiró.",
                Contexto = contexto
            };

            var vigente = await _orquestador.IniciarAsync(
                ClaveOperacion,
                solicitud,
                $"LP-{Guid.NewGuid():N}",
                TokenCookie,
                cancellationToken);

            return Ok(new
            {
                ok = true,
                idSolicitud = vigente.IdSolicitud,
                claveOperacion = ClaveOperacion,
                timeoutSegundos = _cambioOptions.TimeoutSegundos,
                urlEstado = Url.Action(
                    "Estado",
                    "AutorizacionRemota",
                    new { area = string.Empty, claveOperacion = ClaveOperacion, idSolicitud = vigente.IdSolicitud }),
                listaSolicitada = new { id = solicitada.lp_id.Trim(), descripcion = solicitada.lp_desc }
            });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { ok = false, mensaje = ex.Message });
        }
        catch (NegocioException ex)
        {
            _logger?.LogWarning(ex, "La API rechazo la solicitud de cambio de lista de precios.");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { ok = false, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "No se pudo solicitar el cambio de lista de precios.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { ok = false, mensaje = "No se pudo crear la solicitud de autorización." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AplicarCambio(
        [FromBody] AplicarCambioListaPrecioRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || request.IdSolicitud == Guid.Empty)
            {
                return BadRequest(new { ok = false, mensaje = "La solicitud es inválida." });
            }

            var error = ValidarOperacionInicial();
            if (error is not null)
            {
                return error;
            }

            var consulta = await _orquestador.ConsultarAsync(
                ClaveOperacion,
                request.IdSolicitud,
                TokenCookie,
                cancellationToken);

            if (!consulta.Vigente)
            {
                return Conflict(new { ok = false, mensaje = "La solicitud ya no está vigente." });
            }

            var solicitud = consulta.Solicitud!;
            var resolucion = solicitud.Resolucion;
            if (solicitud.Estado != "RESUELTO" ||
                !string.Equals(resolucion?.Decision, "APROBADO", StringComparison.OrdinalIgnoreCase) ||
                resolucion?.EsResolucionPorDefecto != false)
            {
                return Conflict(new { ok = false, mensaje = "La solicitud no fue aprobada por un administrador." });
            }

            var contexto = JObject.Parse(solicitud.ContextoJson);
            ValidarContexto(contexto);
            var lpSolicitadaId = contexto["listaPrecioSolicitada"]?["id"]?.Value<string>()?.Trim()
                ?? throw new InvalidOperationException("La solicitud no contiene la lista requerida.");

            var listas = await ObtenerCatalogoAsync(cancellationToken);
            var solicitada = BuscarLista(listas, lpSolicitadaId)
                ?? throw new InvalidOperationException("La lista autorizada ya no existe en el catálogo.");

            LP_Id = solicitada.lp_id.Trim();
            var cliente = ClienteActual!;
            cliente.lp_id = LP_Id;
            ClienteActual = cliente;
            _orquestador.Completar(ClaveOperacion, request.IdSolicitud);

            _logger?.LogInformation(
                "Lista de precios {ListaPrecio} aplicada por autorización {IdSolicitud}.",
                LP_Id,
                request.IdSolicitud);

            return Ok(new
            {
                ok = true,
                lista = new { id = LP_Id, descripcion = solicitada.lp_desc }
            });
        }
        catch (UnauthorizedException ex)
        {
            return Unauthorized(new { ok = false, mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogWarning(ex, "Se rechazó la aplicación de la autorización {IdSolicitud}.", request?.IdSolicitud);
            return Conflict(new { ok = false, mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "No se pudo aplicar el cambio de lista de precios.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { ok = false, mensaje = "No se pudo aplicar la lista de precios autorizada." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> FinalizarNoAprobada(
        [FromBody] AplicarCambioListaPrecioRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.IdSolicitud == Guid.Empty)
        {
            return BadRequest(new { ok = false, mensaje = "La solicitud es inválida." });
        }

        try
        {
            var consulta = await _orquestador.ConsultarAsync(
                ClaveOperacion,
                request.IdSolicitud,
                TokenCookie,
                cancellationToken);
            var terminal = consulta.Solicitud?.Estado is "RESUELTO" or "EXPIRADO";
            var aprobada = string.Equals(
                consulta.Solicitud?.Resolucion?.Decision,
                "APROBADO",
                StringComparison.OrdinalIgnoreCase);

            if (consulta.Vigente && terminal && !aprobada)
            {
                _orquestador.Completar(ClaveOperacion, request.IdSolicitud);
            }

            return Ok(new { ok = true });
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "No se pudo liberar la solicitud no aprobada {IdSolicitud}.", request.IdSolicitud);
            return Ok(new { ok = true });
        }
    }

    private IActionResult? ValidarOperacionInicial()
    {
        if (!VerificarAutenticacion(out _))
        {
            return Unauthorized(new { ok = false, mensaje = "La sesión ha expirado." });
        }

        if (ClienteActual is null)
        {
            return BadRequest(new { ok = false, mensaje = "Debe identificar un cliente." });
        }

        if (string.IsNullOrWhiteSpace(LP_Id))
        {
            return Conflict(new { ok = false, mensaje = "No hay una lista de precios activa." });
        }

        if (ProductosSeleccionados.Count > 0 || FacturaProductos.Count > 0)
        {
            return Conflict(new
            {
                ok = false,
                mensaje = "La lista de precios sólo puede cambiarse antes de cargar productos."
            });
        }

        var caja = CajaActual;
        if (string.IsNullOrWhiteSpace(caja.CajaId) ||
            string.IsNullOrWhiteSpace(caja.Caja.caja_nro_proceso) ||
            string.IsNullOrWhiteSpace(caja.Caja.caja_nro_cierre))
        {
            return Conflict(new { ok = false, mensaje = "La caja no tiene proceso y cierre válidos." });
        }

        try
        {
            _ = DeterminarCoTipo(ClienteActual.Origen);
            _ = FormatearCierre(caja.Caja.caja_nro_cierre);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { ok = false, mensaje = ex.Message });
        }

        return null;
    }

    private async Task<List<PrecioListaDto>> ObtenerCatalogoAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var resultado = await _productoFactServicio.ObtenerListasPrecios(TokenCookie);
        if (!resultado.Ok)
        {
            throw new InvalidOperationException(
                resultado.Mensaje ?? "No se pudo consultar el catálogo de listas de precios.");
        }

        return resultado.ListaEntidad ?? [];
    }

    private void ValidarContexto(JObject contexto)
    {
        var caja = CajaActual;
        var cliente = ClienteActual!;
        var actualContexto = contexto["listaPrecioActual"]?["id"]?.Value<string>();
        var valido =
            contexto.Value<string>("tipoAutorizacion") == "LP" &&
            contexto.Value<string>("aplicacion") == "gc.caja" &&
            contexto.Value<string>("modulo") == "FACTURACION" &&
            contexto.Value<string>("coTipo") == DeterminarCoTipo(cliente.Origen) &&
            contexto.Value<string>("cajaId") == caja.CajaId &&
            contexto.Value<string>("cajaNroProceso") == caja.Caja.caja_nro_proceso &&
            contexto.Value<string>("cajaNroCierre") == FormatearCierre(caja.Caja.caja_nro_cierre) &&
            contexto.Value<string>("cta_id") == cliente.cta_id &&
            string.Equals(contexto.Value<string>("origen"), cliente.Origen, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(actualContexto, LP_Id, StringComparison.OrdinalIgnoreCase);

        if (!valido)
        {
            throw new InvalidOperationException("El contexto autorizado no coincide con la operación actual.");
        }
    }

    private static JObject CrearListaContexto(PrecioListaDto lista) => new()
    {
        ["id"] = lista.lp_id.Trim(),
        ["descripcion"] = lista.lp_desc
    };

    private static PrecioListaDto? BuscarLista(IEnumerable<PrecioListaDto> listas, string id) =>
        listas.FirstOrDefault(x => string.Equals(x.lp_id?.Trim(), id, StringComparison.OrdinalIgnoreCase));

    private static string DeterminarCoTipo(string? origen) => origen?.ToUpperInvariant() switch
    {
        "C" => "CR",
        "F" => "CF",
        _ => throw new InvalidOperationException("El origen del cliente no permite determinar el tipo de facturación.")
    };

    private static string FormatearCierre(string? cierre)
    {
        if (!int.TryParse(cierre, out var numero) || numero < 0 || numero > 99)
        {
            throw new InvalidOperationException("El número de cierre de caja es inválido.");
        }

        return numero.ToString("00");
    }
}

public sealed class AplicarCambioListaPrecioRequest
{
    public Guid IdSolicitud { get; set; }
}
