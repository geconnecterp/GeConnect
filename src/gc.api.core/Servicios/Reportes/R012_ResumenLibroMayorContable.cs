using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Reportes
{
    public class R012_ResumenLibroMayorContable : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiLMayorServicio _lMayorSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R012_ResumenLibroMayorContable(IUnitOfWork uow,
            IApiLMayorServicio lmSv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _lMayorSv = lmSv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Fecha", "N° Movimiento", "Concepto", "Debe", "Haber", "Saldo" };
            _campos = new List<string> { "Fecha", "Movimiento", "Concepto", "Debe", "Haber", "Saldo" };
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

                var registros = ObtenerDatos(solicitud, out ctaId);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron Asientos. Deberia reformular los parámetros de consulta.");
                }

                var regs = registros.Select(x => new
                {
                    Fecha = x.Dia_fecha.ToShortDateString(),
                    Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                    Concepto = x.Dia_desc,
                    Debe = x.Dia_debe,
                    Haber = x.Dia_haber,
                    Saldo = x.Dia_saldo
                }).ToList();

                #endregion
                #region Scripts PDF

                anchos = [10f, 15f, 45f, 10f, 10f, 10f];
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);
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

                PdfPTable tabla = GeneraCabeceraPDF2(solicitud, chico, titulo, logo, _empresaGeco);

                // Convertir la tabla en un Phrase
                Phrase phrase = new Phrase();
                phrase.Add(tabla);

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

                #region Carga del Listado
                Paragraph subText;
                subText = HelperPdf.GeneraParrafo(solicitud.SubTitulo, titulo, Element.ALIGN_LEFT, 3, 3);
                pdf.Add(subText);

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                var totales = new Dictionary<string, decimal>
                {
                    { "Debe", regs.Sum(x => x.Debe) },
                    { "Haber", regs.Sum(x => x.Haber) },
                    //{ "Saldo", regs.Sum(x => x.Saldo) }
                };

                HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico, true, true, totales);

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
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<LMayorRegListaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Fecha = x.Dia_fecha.ToShortDateString(),
                Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                Concepto = x.Dia_desc,
                Debe = x.Dia_debe,
                Haber = x.Dia_haber,
                Saldo = x.Dia_saldo
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<LMayorRegListaDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Fecha = x.Dia_fecha.ToShortDateString(),
                Movimiento = string.IsNullOrEmpty(x.Dia_movi) ? "0000-00000000" : x.Dia_movi,
                Concepto = x.Dia_desc,
                Debe = x.Dia_debe,
                Haber = x.Dia_haber,
                Saldo = x.Dia_saldo
            }).ToList();
            #endregion
            return GeneraFileXLS(regs, _titulos, _campos);
        }



        private List<LMayorRegListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
        {

            var eje_nro = solicitud.Parametros.GetValueOrDefault("eje_nro", "");
            var ccb_id = solicitud.Parametros.GetValueOrDefault("ccb_id", "");
            var ccb_desc = solicitud.Parametros.GetValueOrDefault("ccb_desc", "");
            var incluirTemporales = solicitud.Parametros.GetValueOrDefault("incluirTemporales", "").ToBoolean();
            var rango = solicitud.Parametros.GetValueOrDefault("rango", "").ToBoolean();
            var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
            var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();



            //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
            string dia_movi = solicitud.Parametros.GetValueOrDefault("dia_movi", "") ?? "";
            id = ccb_id;
            return _lMayorSv.ObtenerLibroMayor(eje_nro.ToInt(),
                ccb_id,
                rango, desde, hasta, incluirTemporales, 99999, 1, "");
        }
    }
}
