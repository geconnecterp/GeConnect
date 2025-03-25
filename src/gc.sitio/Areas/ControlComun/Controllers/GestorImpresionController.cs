using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.ViewModels;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class GestorImpresionController : ControladorBase
    {
        private readonly AppSettings _setting;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDocManagerServicio _docMSv;


        public GestorImpresionController(IOptions<AppSettings> options, IHttpContextAccessor accessor,ILogger<GestorImpresionController> logger,
            IDocManagerServicio docManager) 
            :base(options,accessor,logger)
        {
            _setting = options.Value;
            _accessor = accessor;
            _docMSv = docManager;
        }

        [HttpPost]
        public IActionResult OrquestadorDeModulos(string modulo, params string[] parametros)
        {
            RespuestaGenerica<EntidadBase> response = new();

            //esta action tendrá como funcion enviar al componente de impresión, la configuración
            //del modulo respecto a la funcionalidad que tendrá permitida
            try
            {
                //el VM se va cargado en el Modulo origen a medida que se van ejecutando las consultas.
                var docMgr = DocumentManager;
                return View("~/areas/ControlComun/views/GestorImpresion/_docManagerModal.cshtml", docMgr);
            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {

                string msg = "Error en la obtención de la configuración para el Gestor Documental.";
                _logger.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public JsonResult PresentarArchivos()
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var arbol = ArchivosCargadosModulo;
                var jarbol = JsonConvert.SerializeObject(arbol);
                return Json(new { error = false, warn = false, arbol = jarbol });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

    }
}
