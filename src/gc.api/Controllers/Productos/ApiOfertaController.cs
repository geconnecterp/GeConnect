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

        [HttpGet("buscar-tipos-oferta")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<List<TipoOfertaDto>>))]
        public ActionResult<List<TipoOfertaDto>> BuscarTiposOferta()
        {
            var resultado = _ofertaSv.BuscarTiposOferta();
            return Ok(new ApiResponse<List<TipoOfertaDto>>(resultado));
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
                string.IsNullOrWhiteSpace(param.OftId) || param.OftId.Trim().Length != 1 ||
                param.Desde == default || param.Hasta == default ||
                param.TopeVta < 0 || param.Hasta < param.Desde ||
                param.Desde < hoy || param.Hasta > param.Desde.AddDays(30 - 1))
            {
                return BadRequest("Alguno de los parametros para la confirmacion del alta de la oferta ha faltado o es incorrecto. Verifique");
            }

            param.OftId = param.OftId.Trim();
            var tipoOfertaExiste = _ofertaSv.BuscarTiposOferta()
                .Any(t => string.Equals(t.oft_id?.Trim(), param.OftId, StringComparison.OrdinalIgnoreCase));
            if (!tipoOfertaExiste)
                return BadRequest("El tipo de oferta seleccionado no existe o ya no se encuentra disponible.");

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
        public ActionResult<List<OfertaDto>> ObtenerOfertasSinActivar(string admId, string lp_id)
        {
            if (string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(lp_id))
            {
                return BadRequest("Alguno de los parametros para obtener las ofertas sin activar ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ObtenerOfertas(admId, lp_id);
            return Ok(new ApiResponse<List<OfertaDto>>(resultado));
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
            if (req == null || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la actualizacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.ActualizarOfertaVencidaSinActivar(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("cargar-activas-a-sin-activar")]
        public ActionResult<RespuestaDto> CargarActivasASinActivar(AbmGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la carga de ofertas activas a sin activar ha faltado. Verifique");
            }
            var resultado = _ofertaSv.CargarActivasASinActivar(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("eliminar-ofertas")]
        public ActionResult<RespuestaDto> EliminarOfertas(AbmPlusGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la eliminacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.EliminarOfertas(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        //metodos de Ofertas Activas
        [HttpGet("obtener-ofertas-activas")]
        public ActionResult<List<OfertaDto>> ObtenerOfertasActivas(string admId, string lp_id)
        {
            if (string.IsNullOrEmpty(admId) || string.IsNullOrEmpty(lp_id))
            {
                return BadRequest("Alguno de los parametros para obtener las ofertas sin activar ha faltado. Verifique");
            }
            //el flag en false indica que traiga las ofertas activas
            var resultado = _ofertaSv.ObtenerOfertas(admId, lp_id,false);
            return Ok(new ApiResponse<List<OfertaDto>>(resultado));
        }

        [HttpPost("elimina-ofertas-activas")]
        public ActionResult<RespuestaDto> EliminaOfertasActivas(AbmPlusGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la eliminacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.EliminaOfertasActivas(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }

        [HttpPost("copiar-a-canal")]
        public ActionResult<RespuestaDto> CopiarACanal(AbmPlusGenDto req)
        {
            if (req == null || string.IsNullOrEmpty(req.Json) || string.IsNullOrEmpty(req.Json2) || string.IsNullOrEmpty(req.Objeto)
                || string.IsNullOrEmpty(req.Usuario) || string.IsNullOrEmpty(req.Administracion))
            {
                return BadRequest("Alguno de los parametros para la eliminacion de la oferta ha faltado. Verifique");
            }
            var resultado = _ofertaSv.CopiarACanal(req);
            return Ok(new ApiResponse<RespuestaDto>(resultado));
        }
    }
}
