using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.Controllers.Ofertas
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPresupuestoController : ControllerBase
    {
        private readonly ILogger<ApiPresupuestoController> _logger;
        private readonly IApiPresupuetoServicio _presuSv;

        public ApiPresupuestoController(ILogger<ApiPresupuestoController> logger, IApiPresupuetoServicio servicio)
        {
            _logger = logger;
            _presuSv = servicio;
        }

        // Buscar lista paginada de presupuestos, devolviendo ApiResponse con Metadata
        [HttpPost("buscar-presupuestos")]
        public IActionResult BuscarPresupuestos(QueryFilters filtro)
        {
            const string msgError = "Error en la invocación de la API - Búsqueda de Presupuestos";
            try
            {
                if (filtro == null)
                {
                    return BadRequest("No se recepcionó el filtro de la búsqueda de Presupuestos.");
                }

                var request = MapToRequest(filtro);
                var resultados = _presuSv.ObtenerListaPresupuestos(request);

                var response = new ApiResponse<List<PresupuestoListDto>>(resultados)
                {
                    Meta = BuildMetadata(resultados, filtro)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        // Obtiene datos de un presupuesto por id
        [HttpGet("presupuesto/{id}")]
        public IActionResult ObtenerPresupuesto(string id)
        {
            const string msgError = "Error en la invocación de la API - Obtener Presupuesto";
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Debe indicar el identificador del presupuesto.");
                }

                var datos = _presuSv.ObtenerPresupuesto(id);
                return Ok(new ApiResponse<List<PresupuestoDto>>(datos));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        // Obtiene el detalle de un presupuesto por id
        [HttpGet("presupuesto/detalle/{id}")]
        public IActionResult ObtenerDetallePresupuesto(string id)
        {
            const string msgError = "Error en la invocación de la API - Obtener Detalle de Presupuesto";
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Debe indicar el identificador del presupuesto.");
                }

                var detalle = _presuSv.ObtenerDetallePresupuesto(id);
                return Ok(new ApiResponse<List<PresupuestoProductoDto>>(detalle));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        // Obtiene el detalle de un presupuesto por id
        [HttpGet("presupuesto/detalle/actualizado/{id}")]
        public IActionResult ObtenerDetallePresupuestoActualizado(string id)
        {
            const string msgError = "Error en la invocación de la API - Obtener Detalle de Presupuesto";
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest("Debe indicar el identificador del presupuesto.");
                }

                var detalle = _presuSv.ObtenerDetallePresupuestoActualizado(id);
                return Ok(new ApiResponse<List<PresupuestoProductoDto>>(detalle));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        // Obtiene los estados de presupuesto
        [HttpGet("estados")]
        public IActionResult ObtenerEstadosPresupuesto()
        {
            const string msgError = "Error en la invocación de la API - Obtener Estados de Presupuesto";
            try
            {
                var estados = _presuSv.ObtenerEstadosPresupuesto();
                return Ok(new ApiResponse<List<PresupE>>(estados));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        // Obtiene los estados de presupuesto
        [HttpGet("tipos")]
        public IActionResult ObtenerTiposPresupuesto()
        {
            const string msgError = "Error en la invocación de la API - Obtener Tipos de Presupuesto";
            try
            {
                var tipos = _presuSv.ObtenerTiposPresupuesto();
                return Ok(new ApiResponse<List<PresupT>>(tipos));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msgError);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = true, msg = msgError });
            }
        }

        [HttpPost("presupuesto/confirmar")]
        public IActionResult ConfirmarPresupuesto(AbmPlusGenDto req)
        {
            if(req == null)
            {
                return BadRequest("No se recepcionó la información para confirmar el presupuesto.");
            }
            var respuesta = _presuSv.ConfirmarPresupuesto(req);
            return Ok(new ApiResponse<RespuestaDto>(respuesta));
        }

        // Mapea filtros a request del SP (minimizando asignaciones innecesarias)
        private static PresupuestoRequest MapToRequest(QueryFilters filtro)
        {
            return new PresupuestoRequest
            {
                Registros = filtro.Registros ?? 0,
                Pagina = filtro.Pagina ?? 0,
                Desde = filtro.FechaD ?? DateTime.MinValue,
                Hasta = filtro.FechaH ?? DateTime.MaxValue,
                cli_list = ToCsv(filtro.Rel01),
                usu_list = ToCsv(filtro.Rel02),
                pree_list = filtro.Rel03 != null && filtro.Rel03.Count > 0 ? string.Join(",", filtro.Rel03.Select(x => x.Id)) : null,
                adm_list = filtro.Rel04 != null && filtro.Rel04.Count > 0 ? string.Join(",", filtro.Rel04.Select(x => x.Id)) : null
            };
        }

        // Construye metadata del grid en base al primer elemento (evita recorrer la colección)
        private static MetadataGrid? BuildMetadata(List<PresupuestoListDto>? lista, QueryFilters filtro)
        {
            if (lista == null || lista.Count == 0)
            {
                return new MetadataGrid
                {
                    TotalCount = 0,
                    PageSize = filtro.Registros ?? 0,
                    CurrentPage = filtro.Pagina ?? 0,
                    TotalPages = 0,
                    HasNextPage = false,
                    HasPreviousPage = false,
                    NextPageUrl = null,
                    PreviousPageUrl = null
                };
            }

            var reg = lista[0];
            var pageSize = filtro.Registros ?? 0;
            var currentPage = filtro.Pagina ?? 0;
            var totalCount = reg.Total_registros;
            var totalPages = reg.Total_paginas;

            return new MetadataGrid
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                HasNextPage = currentPage < totalPages,
                HasPreviousPage = currentPage > 1,
                NextPageUrl = null,
                PreviousPageUrl = null
            };
        }

        private static string? ToCsv(List<string>? values)
        {
            if (values == null || values.Count == 0) return null;
            return string.Join(",", values);
        }
    }
}
