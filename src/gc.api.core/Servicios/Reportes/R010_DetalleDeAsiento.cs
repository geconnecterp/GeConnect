using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Reportes
{
    public class R010_DetalleDeAsiento : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IAsientoTemporalServicio _asTempSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R010_DetalleDeAsiento(IUnitOfWork uow, IAsientoTemporalServicio atempsv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _asTempSv = atempsv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Cuenta", "Descr.Cuenta", "Concepto", "Debe", "Haber" };
            _campos = new List<string> { "Ccb_id", "Ccb_desc", "Dia_desc", "Debe", "Haber" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {

            float[] anchos;
            string ctaId = "";
            PdfWriter? writer = null;
            Document pdf;
            try
            {
                var ms = new MemoryStream();

                #region Obteniendo registros desde la base de datos

                AsientoDetalleDto asiento = ObtenerDatos(solicitud, out ctaId);

                if (asiento == null || asiento.Detalles.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }

                #region Defino el titulo del reporte
                string tipo = asiento.esTemporal.HasValue && asiento.esTemporal.Value ? "Temporal" : "Definitivo";
                string tituloRepo = $"Detalle de Asiento {tipo}\r\nMovimiento N°: {asiento.Dia_movi}\r\nUsuario: {asiento.usu_apellidoynombre} ";
                solicitud.Titulo = tituloRepo;
                #endregion

                #endregion
                #region Scripts PDF

                anchos = [10f, 30f, 30f, 15f, 15f];
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                HelperPdf.ConfigurarPieDePaginaPersonalizado(writer, solicitud.Observacion);

                // writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);
                Image? logo;
                if (!string.IsNullOrEmpty(solicitud.LogoPath))
                {
                    logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);
                }
                else
                {
                    logo = null;
                }

                #endregion

                #region Generación de Cabecera               

                PdfPTable tabla;
                Phrase phrase;
                CargarCabecera(solicitud, chico, titulo, logo,_empresaGeco, out tabla, out phrase);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                    //BorderWidthBottom = 1,   

                };

                pdf.Header = header;
                #endregion

                pdf.Open();


                tabla = HelperPdf.GeneraTabla(2, [20f, 80f], 100, 10, 10);
                #region Cabecera Info de Asiento
                //fila 1
                tabla.AddCell(HelperPdf.CeldaSinBorde("Fecha:", subtitulo, Element.ALIGN_RIGHT));
                tabla.AddCell(HelperPdf.CeldaSinBorde(asiento.Dia_fecha.ToShortDateString(), subtitulo, Element.ALIGN_LEFT));
                //fila 2
                tabla.AddCell(HelperPdf.CeldaSinBorde("Tipo Asiento:", subtitulo, Element.ALIGN_RIGHT));
                tabla.AddCell(HelperPdf.CeldaSinBorde(asiento.Dia_lista, subtitulo, Element.ALIGN_LEFT));
                // fila 3
                tabla.AddCell(HelperPdf.CeldaSinBorde("Descripción:", subtitulo, Element.ALIGN_RIGHT));
                tabla.AddCell(HelperPdf.CeldaSinBorde(asiento.Dia_desc_asiento, subtitulo, Element.ALIGN_LEFT));

                pdf.Add(tabla);
                #endregion


                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                var totales = new Dictionary<string, decimal>{
                    { "Debe", asiento.TotalDebe},
                    {"Haber",asiento.TotalHaber }
                    };

                HelperPdf.GenerarListadoDesdeLista(pdf, asiento.Detalles, _campos, anchos, chico, false, true, totales);

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
                _logger.LogError(ex, "Error en R001");

                throw new NegocioException($"Se produjo un error al intentar generar la Impresión del Asiento {ctaId}. Para mayores datos ver el log.");
            }

        }

    
        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La exportación a TXT no se implementará.");
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La exportación a XLS no se implementará.");

        }



        private AsientoDetalleDto ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId)
        {
            ctaId = "";
            //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
            string dia_movi = solicitud.Parametros.GetValueOrDefault("dia_movi", "").ToString() ?? "";
            ctaId = dia_movi;
            return _asTempSv.ObtenerAsientoDetalle(dia_movi,true);
        }
    }
}
