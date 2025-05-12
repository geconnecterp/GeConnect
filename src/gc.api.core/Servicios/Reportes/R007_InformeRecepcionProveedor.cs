using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Reportes
{
    public class R007_InformeRecepcionProveedor : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R007_InformeRecepcionProveedor(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "RP N°", "Fecha", "Cmpte RP", "Cmpte CtaCte", "Sucursal", "OC", "CTL", "FAC", "VAL", "MOD", "COL" };
            _campos = new List<string> { "RpNro", "Fecha", "CmpteRp", "CmpteCC", "Sucursal", "Oc", "Ctl", "Fac", "Val", "Mod", "Col" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            float[] anchos;

            PdfWriter? writer = null;
            Document pdf;

            try
            {
                var ms = new MemoryStream();
                #region Obteniendo registros desde la base de datos
                string ctaId;
                string tit;
                List<ConsRecepcionProveedorDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }


                //buscando datos del cliente
                var c = _cuentaSv.GetCuentaComercialLista(ctaId, 'T');

                if (c == null || c.Count == 0)
                {
                    throw new NegocioException($"No se encontraron datos del cliente {ctaId}.");
                }
                var cliente = c[0];
                cliente.Monto = 0m;
                cliente.MontoEtiqueta = "";
                //COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
                tit += cliente.Cta_Denominacion;
                solicitud.Titulo = tit;
                solicitud.Cuenta = cliente;


                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
                    RpNro = x.Rp_compte,
                    Fecha = x.Rp_fecha,
                    CmpteRp = $"({x.Tco_id_rp}) {x.Cm_compte_rp}",
                    CmpteCC = $"({x.Tco_id}) {x.Cm_compte}",
                    Sucursal = x.Adm_nombre,
                    Oc = x.Oc_compte,
                    Ctl = x.Controlada,
                    Fac = x.Factura,
                    Val = x.Valorizada,
                    Mod = x.Modificada,
                    Col = x.Colector,

                }).ToList();

                #endregion
                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, false);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

                #endregion
                //****=============================****/
                //****  CAMBIAR ANCHOS DE COLUMNAS ****
                //****=============================****/
                anchos = [8f, 8f, 12f, 12f, 10f, 8f, 5f, 5f, 5f, 5f, 5f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera               
                PdfPTable tabla = GeneraCabeceraPdf(solicitud, logo, chico, titulo, _empresaGeco);
                Phrase phrase = new Phrase();
                phrase.Add(tabla);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                };

                pdf.Header = header;
                #endregion

                pdf.Open();

                #region Datos del Cliente o Proveedor

                HelperPdf.PresentarDatosCuentaTablaMarco(pdf, cliente, normal, normalBold);
                #endregion

                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                GenerarTablaRecepcionesProveedor(pdf, registros,anchos, chico);


                #endregion

                pdf.Close();
                #endregion

                return Convert.ToBase64String(ms.ToArray());

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //_logger.Log(typeof(R001_InformeCuentaCorriente), Level.Error, $"Error al generar el informe de cuenta corriente: {ex.Message}", ex);
                _logger.LogError(ex, "Error en R003");
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }
        }



        private List<ConsRecepcionProveedorDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            var fechaStr = solicitud.Parametros.GetValueOrDefault("fechaD", "").ToString();
            if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out DateTime fechaD))
            {
                throw new NegocioException("La fecha desde es inválida.");
            }
            fechaD = fechaStr.ToDateTime();

            fechaStr = solicitud.Parametros.GetValueOrDefault("fechaH", "").ToString();
            if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out DateTime fechaH))
            {
                throw new NegocioException("La fecha hasta es inválida.");
            }
            fechaH = fechaStr.ToDateTime();

            var admId = solicitud.Administracion.Split("#")[0];

            string userId = solicitud.Parametros.GetValueOrDefault("userId", "").ToString() ?? "";
            titulo = $"Recepción de Productos de Proveedor\r\nCuenta: ({ctaId}) Periodo: {fechaD.ToString("dd/MM/yy")} - {fechaH.ToString("dd/MM/yy")} ";
            return _consultaServicio.ConsultaRecepcionProveedor(ctaId, fechaD, fechaH, admId);

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsRecepcionProveedorDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                RpNro = x.Rp_compte,
                Fecha = x.Rp_fecha,
                CmpteRp = $"({x.Tco_id_rp}) {x.Cm_compte_rp}",
                CmpteCC = $"({x.Tco_id}) {x.Cm_compte}",
                Sucursal = x.Adm_nombre,
                Oc = x.Oc_compte,
                Ctl = x.Controlada,
                Fac = x.Factura,
                Val = x.Valorizada,
                Mod = x.Modificada,
                Col = x.Colector,

            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsRecepcionProveedorDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                RpNro = x.Rp_compte,
                Fecha = x.Rp_fecha,
                CmpteRp = $"({x.Tco_id_rp}) {x.Cm_compte_rp}",
                CmpteCC = $"({x.Tco_id}) {x.Cm_compte}",
                Sucursal = x.Adm_nombre,
                Oc = x.Oc_compte,
                Ctl = x.Controlada,
                Fac = x.Factura,
                Val = x.Valorizada,
                Mod = x.Modificada,
                Col = x.Colector,

            }).ToList();
            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }

        public static void GenerarTablaRecepcionesProveedor(Document pdf, List<ConsRecepcionProveedorDto> datos,float[] anchos, Font fuente)
        {
            var tabla = HelperPdf.GeneraTabla(11, anchos, 100, 5, 10);

            bool alternar = false;

            foreach (var item in datos ?? Enumerable.Empty<ConsRecepcionProveedorDto>())
            {
                var colorFondo = alternar ? BaseColor.White : new BaseColor(245, 245, 245);
                alternar = !alternar;

                tabla.AddCell(CeldaTexto(item.Rp_compte, fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rp_fecha.ToString("dd/MM/yyyy"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto($"({item.Tco_id_rp}) {item.Cm_compte_rp}", fuente, colorFondo));
                tabla.AddCell(CeldaTexto($"({item.Tco_id}) {item.Cm_compte}", fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Adm_nombre, fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Oc_compte, fuente, colorFondo));

                tabla.AddCell(CeldaCheck(item.Controlada, fuente, colorFondo, BaseColor.Green));
                tabla.AddCell(CeldaCheck(item.Factura, fuente, colorFondo, BaseColor.Red));
                tabla.AddCell(CeldaCheck(item.Valorizada, fuente, colorFondo, BaseColor.Blue));
                tabla.AddCell(CeldaCheck(item.Modificada, fuente, colorFondo, BaseColor.Cyan));
                tabla.AddCell(CeldaCheck(item.Colector, fuente, colorFondo, BaseColor.Gray));
            }

            pdf.Add(tabla);
        }

        

        private static PdfPCell CeldaCheck(bool activo, Font fuente, BaseColor fondo, BaseColor colorCheck)
        {
            string contenido = activo ? "SI" : "";
            var fontCheck = new Font(fuente);
            if (activo) fontCheck.Color = colorCheck;

            var celda = new PdfPCell(new Phrase(contenido, fontCheck))
            {
                BackgroundColor = fondo,
                Border = Rectangle.BOX,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 4
            };
            return celda;
        }
    }
}
