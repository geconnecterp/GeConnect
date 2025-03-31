using ClosedXML.Excel;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.ViewModels;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Collections;
using System.ComponentModel;
using System.Net.Mail;
using System.Net;
using System.Reflection;
using System.Text;
using System.Net.Mail;
using System.Net;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace gc.sitio.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class GestorImpresionController : ControladorBase
    {
        private readonly AppSettings _setting;
        private readonly IHttpContextAccessor _accessor;
        private readonly IDocManagerServicio _docMSv;
        private readonly IWebHostEnvironment _env;


        public GestorImpresionController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<GestorImpresionController> logger,
            IDocManagerServicio docManager, IWebHostEnvironment env)
            : base(options, accessor, logger)
        {
            _setting = options.Value;
            _accessor = accessor;
            _docMSv = docManager;
            _env = env;
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

        [HttpPost]
        public JsonResult GeneradorArchivo(string formato, string archivoBase64, string nodoId, string moduloId, string tipo)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                MemoryStream ms = new MemoryStream(Convert.FromBase64String(archivoBase64));
                string fileName = $"{moduloId}_{nodoId}_{DateTime.Now.Ticks}";
                string fileUrl = string.Empty;

                #region Verificación de la ruta 
                var path = Path.Combine(_env.WebRootPath, _setting.FolderArchivo);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                #endregion

                switch (formato)
                {
                    case "P":
                        fileUrl = GuardarArchivoPDF(ms, fileName, nodoId, moduloId, path);
                        break;
                    case "X":
                        fileUrl = ExportarAExcel(nodoId, fileName, moduloId, tipo, path);
                        break;
                    case "T":
                        fileUrl = ExportarATxt(nodoId, fileName, moduloId, tipo, path);
                        break;
                    default:
                        break;
                }

                return Json(new { error = false, warn = false, fileUrl = fileUrl, fileName = $"{fileName}.{formato.ToLower()}" });
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

        private string GuardarArchivoPDF(MemoryStream ms, string fileName, string nodoId, string moduloId, string path)
        {
            string filePath = Path.Combine(path, $"{fileName}.pdf");
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                ms.WriteTo(fileStream);
            }
            return $"/{_setting.FolderArchivo}/{fileName}.pdf";
        }

        private string ExportarAExcel(string nodoId, string fileName, string moduloId, string tipo, string path)
        {
            var listType = Type.GetType(tipo);
            // Recuperar los datos de la variable de sesión
            var datos = _context.HttpContext.Session.GetObjectFromJson($"datos_{moduloId}_{nodoId}", listType);

            // Convertir los datos a Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");

                // Obtener el tipo genérico (el tipo de los elementos de la lista)
                Type elementType = listType.GetGenericArguments().FirstOrDefault();
                if (elementType == null)
                    throw new ArgumentException("No se pudo obtener el tipo de los elementos de la lista");

                // Obtener las propiedades públicas del tipo de elemento
                var properties = elementType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // Verificar que data sea una lista
                if (!(datos is IList listData))
                    throw new ArgumentException("El parámetro data debe ser una lista");

                // Obtener los nombres de las propiedades como encabezados
                var headers = properties.Select(p => p.Name).ToList();

                //carga cabecera
                int i = 0;
                foreach (var item in headers)
                {
                    worksheet.Cell(1, i + 1).Value = item;
                    i++;
                }



                // Agregar datos
                var listaDatos = (IEnumerable<object>)datos;
                int fila = 2;
                foreach (var item in listaDatos)
                {
                    i = 0;
                    foreach (PropertyInfo prop in properties)
                    {
                        var valor = prop.GetValue(item);
                        worksheet.Cell(fila, i + 1).Value = valor != null ? valor.ToString() : string.Empty;
                        i++;
                    }

                    //for (int i = 0; i < propiedades.Length; i++)
                    //{
                    //    var valor = propiedades[i].GetValue(item);
                    //    worksheet.Cell(fila, i + 1).Value = valor != null?valor.ToString():string.Empty;
                    //}
                    fila++;
                }

                var filePath = Path.Combine(path, $"{fileName}.xlsx");
                workbook.SaveAs(filePath);
                return $"/{_setting.FolderArchivo}/{fileName}.xlsx";
            }
        }

        private string ExportarATxt(string nodoId, string fileName, string moduloId, string tipo, string path)
        {
            var tipoObjeto = Type.GetType(tipo);
            // Recuperar los datos de la variable de sesión
            var datos = _context.HttpContext.Session.GetObjectFromJson($"datos_{moduloId}_{nodoId}", tipoObjeto);

            // Convertir los datos a TXT
            var sb = new StringBuilder();
            foreach (var item in (IEnumerable<object>)datos)
            {
                sb.AppendLine(string.Join("\t", item.GetType().GetProperties().Select(p => p.GetValue(item, null))));
            }
            var filePath = Path.Combine(path, $"{fileName}.txt");
            System.IO.File.WriteAllText(filePath, sb.ToString());
            return $"/{_setting.FolderArchivo}/{fileName}.txt";
        }



        [HttpPost]
        public JsonResult EnviarEmail(List<ArchivoSendDto> archivos, string emailTo, string emailSubject, string emailBody)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var message = new MailMessage();
                message.To.Add(new MailAddress(emailTo));
                message.Subject = emailSubject;
                message.Body = emailBody;
                message.IsBodyHtml = true;

                foreach (var archivo in archivos)
                {
                    var archivoBytes = Convert.FromBase64String(archivo.archivoBase64);
                    var archivoStream = new MemoryStream(archivoBytes);
                    message.Attachments.Add(new Attachment(archivoStream, archivo.nombre, "application/pdf"));
                }

                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.tuservidor.com";
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential("tuemail@tuservidor.com", "tucontraseña");
                    smtp.EnableSsl = true;
                    smtp.Send(message);
                }

                return Json(new { error = false, warn = false });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EnviarWhatsApp(List<ArchivoSendDto> archivos, string whatsappTo, string whatsappMessage)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if(archivos.Count == 0)
                {
                    // Construir la URL de la API de WhatsApp
                    var url = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(whatsappMessage)}";
                    return Json(new { error = false, warn = false,url = url, msg=$"Mensaje enviado a {whatsappTo} satisfactoriamente" });
                }
                else
                {
                    var cuentaActual = CuentaComercialSeleccionada;
                    var ahora = DateTime.Now.Ticks;
                    // Guardar los archivos en el servidor
                    var fileLinks = new List<string>();
                    var path = Path.Combine(_env.WebRootPath, _setting.FolderArchivo);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    foreach (var archivo in archivos)
                    {
                        var archivoBytes = Convert.FromBase64String(archivo.archivoBase64);
                        var filePath = Path.Combine(path, $"{archivo.nombre}-{cuentaActual.Cta_Id}-{ahora}.pdf");
                        System.IO.File.WriteAllBytes(filePath, archivoBytes);
                        fileLinks.Add($"{_setting.RutaBase}/{_setting.FolderArchivo}/{archivo.nombre}");
                    }

                    // Construir el mensaje con enlaces a los archivos
                    var messageWithLinks = $"{whatsappMessage}\n\nArchivos:\n" + string.Join("\n", fileLinks);
                    var url = $"https://api.whatsapp.com/send?phone={whatsappTo}&text={Uri.EscapeDataString(messageWithLinks)}";
                    return Json(new { error = false, warn = false, url });
                }
                //TwilioClient.Init(_setting.WspAccountSID, _setting.WspAuthToken);

                //var mediaUrls = new List<Uri>();
                //foreach (var archivo in archivos)
                //{
                //    mediaUrls.Add(new Uri("data:application/pdf;base64," + archivo.archivoBase64));
                //}

                //// Limpiar el número de teléfono
                //whatsappTo = Regex.Replace(whatsappTo, @"[\s\-\.\(\)]", "");

                //var message = MessageResource.Create(
                //    body: whatsappMessage,
                //    from: new Twilio.Types.PhoneNumber($"whatsapp:{_setting.WspNroTelefono}"),
                //    to: new Twilio.Types.PhoneNumber("whatsapp:" + whatsappTo),
                //    mediaUrl: mediaUrls
                //);

                //return Json(new { error = false, warn = false });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }

        public class ArchivoSendDto
        {
            public string archivoBase64 { get; set; }
            public string nombre { get; set; }
        }

    }
}
