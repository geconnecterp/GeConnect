using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Areas.GesImpresion.Models;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.GesImpresion.Controllers
{
    [Area("GesImpresion")]
    public class DocManagerController : ControladorBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<DocManagerController> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly DocsManager _docsManager;
        private readonly IDocManagerServicio _docManagerServicio;
        private readonly AppSettings _setting;
        private readonly EmpresaGeco _empresaGeco;

        public DocManagerController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<DocManagerController> logger, IHttpClientFactory clientFactory,
            IOptions<DocsManager> docsManager, IOptions<EmpresaGeco> empresa, IDocManagerServicio docManager) : base(options, accessor, logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _accessor = accessor;
            _setting = options.Value;
            _docsManager = docsManager.Value;
            _docManagerServicio = docManager;
            _empresaGeco = empresa.Value;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult OrquestadorDeModulos(string modulo)//, params string[] parametros)
        {
            RespuestaGenerica<EntidadBase> response = new();

            //esta acction tendrá como funcion enviar al componente de impresión, la configuración
            //del modulo respecto a la funcionalidad que tendrá permitida
            try
            {
                var mc = _docsManager.Modulos.Find(x => x.Modulo == modulo);
                if (mc == null)
                {
                    throw new NegocioException("No se encontró la configuración para el módulo solicitado.");
                }
                else
                {
                    //resguardamos el modulo que originó la intención de operar
                    ModuloDM = modulo;
                }
                var docMgr = new DocumentManagerViewModel
                {
                    Titulo = mc.Titulo,
                    ShowPrintOption = mc.Print,
                    ShowExportOption = mc.Export,
                    ShowEmailOption = mc.Email,
                    ShowWhatsAppOption = mc.Whatsapp,
                };

                //if (mc.Print)
                //{
                //    docMgr.PrintModel = new PrintDocumentViewModel
                //    {
                //        Documents = new List<DocumentViewModel>() { new DocumentViewModel
                //      {
                //          Description = "Documento de prueba",
                //          Id = "1",
                //          IsSelected = true,
                //            Name = "Documento de prueba",
                //            Type = DocumentType.Original
                //      } },
                //        ShowDuplicates = mc.ImprimeDuplicado,

                //    };

                //}
                return View("~/areas/GesImpresion/views/DocManager/_documentManagerModal.cshtml", docMgr);
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
        public JsonResult GeneradorArchivo(string formato)
        {
            string rutaArchivo = string.Empty;
            try
            {                
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                switch (formato)
                {
                    case "P":
                        var cuenta = CuentaComercialSeleccionada;
                        PrintRequestDto<ConsCtaCteDto> ctaCteFile = new PrintRequestDto<ConsCtaCteDto>
                        {
                            ModuloImpresion = Modulo.CUENTA_CORRIENTE,
                            Cabecera = new DatosCabeceraDto
                            {
                               NombreEmpresa = _empresaGeco.Nombre,
                                Direccion = _empresaGeco.Direccion,
                                CUIT = _empresaGeco.CUIT,
                                IIBB = _empresaGeco.IngresosBrutos,
                                Sucursal =AdministracionId,
                                TituloDocumento = $"{ModuloDM}_{cuenta.Cta_Id}_{DateTime.Now.Ticks}"

                            },
                            Pie=new DatosPieDto
                            {
                                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                                Usuario = UserName,
                                Observaciones = "CUENTA CORRIENTE - Documento generado por el sistema de Gestión Comercial"
                            },
                            Cuerpo = new DatosCuerpoDto<ConsCtaCteDto>
                            {
                                CtaId = cuenta.Cta_Id,
                                RazonSocial = cuenta.Cta_Denominacion,
                                Domicilio = cuenta.Cta_Domicilio,
                                CUIT = cuenta.Cta_Documento,
                                Contacto = "-",
                                Datos = CuentaCorrienteBuscada
                            },
                        };
                        string res = _docManagerServicio.GenerarArchivoPDF(ctaCteFile);
                        break;
                    case "T":
                        break;
                    case "X":
                        break;
                    default:
                        break;
                }
                return Json(new { error = false, warn = false, rutafile = rutaArchivo });
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
