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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;
using System.IO.Pipelines;

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
        private readonly IWebHostEnvironment _env;

        public DocManagerController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<DocManagerController> logger, IHttpClientFactory clientFactory,
            IOptions<DocsManager> docsManager, IOptions<EmpresaGeco> empresa,
            IDocManagerServicio docManager, IWebHostEnvironment env) : base(options, accessor, logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _accessor = accessor;
            _setting = options.Value;
            _docsManager = docsManager.Value;
            _docManagerServicio = docManager;
            _empresaGeco = empresa.Value;
            _env = env;
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
        public JsonResult GeneradorArchivo(string formato, string modulo)
        {
            string rutaArchivo = string.Empty;

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                var mc = _docsManager.Modulos.Find(x => x.Modulo == modulo);
                MemoryStream ms = new MemoryStream();
                switch (formato)
                {
                    case "P":
                        SeleccionaTipoInformePDF(mc, out ms);

                        break;
                    case "T":
                        break;
                    case "X":
                        break;
                    default:
                        break;
                }
                string base64Pdf = Convert.ToBase64String(ms.ToArray());
                return Json(new { error = false, warn = false, pdfBase64 = base64Pdf });

                //return Json(new { error = false, warn = false, rutafile = rutaArchivo });
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

        private void SeleccionaTipoInformePDF(ModuloDocMngr modulo, out MemoryStream ms)
        {
            ms = new MemoryStream();
            string observacion;
            //se distingue el modulo y con ello el conjunto de datos resguardados en memoria.
            switch (modulo.Modulo)
            {
                case "CCtaCte":
                    observacion = "CUENTA CORRIENTE - Documento generado por el sistema de Gestión Comercial";

                    GeneraReporteSegunDatos(modulo, CuentaCorrienteBuscada, observacion, out ms);
                    break;
                case "CVencimiento":
                    observacion = "VENCIMIENTOS DE COMPROBANTES - Documento generado por el sistema de Gestión Comercial";
                    var lsVto = VencimientosBuscados;
                    var vDatos = lsVto.Select(x => new
                    {
                        Descripcion = $"{x.Tco_desc}-{x.Cm_compte}",
                        Cuota = x.Cm_compte_cuota,
                        Est = x.Cv_estado,
                        FechaCmpte = x.Cv_fecha_carga.ToShortDateString(),
                        FechaVto = x.Cv_fecha_vto.ToShortDateString(),
                        Importe = x.Cv_importe
                    }).ToList();
                    GeneraReporteSegunDatos(modulo, vDatos, observacion, out ms);
                    break;
                case "CComprobante":
                    break;
                case "COrdenesPagos":
                    break;
                case "CRecepcionProv":
                    break;
                default:
                    break;
            }


        }

        private void GeneraReporteSegunDatos<T>(ModuloDocMngr modulo, List<T> listado, string observacion, out MemoryStream ms)
        {
            var cuenta = CuentaComercialSeleccionada;

            PrintRequestDto<T> request = new PrintRequestDto<T>();
            switch (modulo.Modulo)
            {
                case "CCtaCte":
                    request.ModuloImpresion = Modulo.CUENTA_CORRIENTE;
                    break;
                case "CVencimiento":
                    request.ModuloImpresion = Modulo.VENCIMIENTO_COMPROBANTES;
                    break;
                case "CComprobante":
                    request.ModuloImpresion = Modulo.COMPROBANTES;
                    break;
                case "COrdenesPagos":
                    request.ModuloImpresion = Modulo.ORDEN_DE_PAGO;
                    break;
                case "CRecepcionProv":
                    request.ModuloImpresion = Modulo.RECEPCION_PROVEEDORES;
                    break;
                default:
                    break;
            }
            string logoPath = Path.Combine(_env.WebRootPath, "img", "gc.png");
            request.Cabecera = new DatosCabeceraDto
            {
                NombreEmpresa = _empresaGeco.Nombre,
                Direccion = _empresaGeco.Direccion,
                CUIT = _empresaGeco.CUIT,
                IIBB = _empresaGeco.IngresosBrutos,
                Sucursal = AdministracionId,
                TituloDocumento = $"{ModuloDM}_{cuenta.Cta_Id}_{DateTime.Now.Ticks}",
                Logo = logoPath,
            };
            request.Pie = new DatosPieDto
            {
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                Usuario = UserName,
                Observaciones = observacion
            };
            request.Cuerpo = new DatosCuerpoDto<T>
            {
                CtaId = cuenta.Cta_Id,
                RazonSocial = cuenta.Cta_Denominacion,
                Domicilio = cuenta.Cta_Domicilio,
                CUIT = cuenta.Cta_Documento,
                Contacto = "-",
                Datos = listado,

            };

            _docManagerServicio.GenerarArchivoPDF(request, out ms);

        }
    }
}
