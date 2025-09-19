using gc.api.core.Contratos.Servicios.Ofertas;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Ofertas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace gc.api.Controllers.Ofertas
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiOfertaController : ControllerBase
    {
        private readonly IApiOfertaServicio _ofertaSv;
        private readonly ILogger<ApiOfertaController> _logger;
        public ApiOfertaController(IApiOfertaServicio apiOfertaServicio, ILogger<ApiOfertaController> logger)
        {
            _ofertaSv = apiOfertaServicio;
            _logger = logger;
        }

        [HttpGet("conocer-estado-oferta")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<string>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult<ProductoResponsePVtaMargen> ConocerEstadoOferta(string p_id, string admId, string lp_id)
        {
            if (string.IsNullOrEmpty(p_id) || string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(lp_id))
            {
                return BadRequest("Alguno de los parametros para conocer el estado de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ConocerEstadoOferta(p_id, admId, lp_id);

            return Ok(new ApiResponse<string>(resultado));
        }

        [HttpGet("buscar-canales")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<CanalDto>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult<List<CanalDto>> BuscarCanales()
        {
            var resultado = _ofertaSv.BuscarCanales();
            return Ok(new ApiResponse<List<CanalDto>>(resultado));
        }

        [HttpPost("confirmacion-alta-oferta")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<RespuestaDto>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public ActionResult<RespuestaDto> ConfirmacionAltaOferta(AbmPlusGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Json2)
                || string.IsNullOrEmpty(req.Json3) || string.IsNullOrEmpty(req.Usuario)
                || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la confirmacion del alta de la oferta ha faltado. Verifique");
            }

            ParamOferta? param = JsonConvert.DeserializeObject<ParamOferta>(req.Json3);
            var hoy = DateTime.Today;
            if (param == null || param.Precio <= 0 ||
                param.Desde == default || param.Hasta == default ||
                param.TopeVta < 0 || param.Hasta < param.Desde ||
                param.Desde < hoy || param.Hasta > param.Desde.AddDays(30-1))
            {
                return BadRequest("Alguno de los parametros para la confirmacion del alta de la oferta ha faltado o es incorrecto. Verifique");
            }
            var resultado = _ofertaSv.ConfirmacionAltaOferta(req, param);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpGet("obtener-estado-oferta-producto")]
        public ActionResult<List<OfertaEstadoDto>> ObtenerEstadoOfertaProducto(string p_id)
        {
            if (string.IsNullOrEmpty(p_id))
            {
                return BadRequest("El parametro p_id es obligatorio.");
            }
            var resultado = _ofertaSv.ObtenerEstadoOfertaProducto(p_id);
            return Ok(new ApiResponse<List<OfertaEstadoDto>>(resultado));
        }

        [HttpGet("obtener-ofertas-sin-activar")]
        public ActionResult<List<OfertaSinActivarDto>> ObtenerOfertasSinActivar(string admId, string lp_id)
        {
            if (string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(lp_id))
            {
                return BadRequest("Alguno de los parametros para obtener las ofertas sin activar ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ObtenerOfertasSinActivar(admId, lp_id);
            return Ok(new ApiResponse<List<OfertaSinActivarDto>>(resultado));
        }

        [HttpPost("activacion-de-oferta")]
        public ActionResult<RespuestaDto> ActivacionDeOferta(AbmPlusGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la activacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ActivacionDeOferta(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("actualizar-oferta-vencida-sin-activar")]
        public ActionResult<RespuestaDto> ActualizarOfertaVencidaSinActivar(AbmGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la actualizacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ActualizarOfertaVencidaSinActivar(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }
    }
}
