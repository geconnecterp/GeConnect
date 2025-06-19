using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R014_BalanceSumasSaldos : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiSumaSaldoServicio _apiBss;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R014_BalanceSumasSaldos(IUnitOfWork uow,
            IApiSumaSaldoServicio bssSv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _apiBss = bssSv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Cuenta", "Denominación", "Debe", "Haber", "Saldo", "Saldo Ant.", "Saldo Suma" };
            _campos = new List<string> { "Cuenta", "Denominacion", "Debe", "Haber", "Saldo", "Saldo_ant", "Saldo_suma" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {

            float[] anchos;
            string aux = "";
            PdfWriter? writer = null;
            Document pdf;
            try
            {
                var ms = new MemoryStream();

                #region Obteniendo registros desde la base de datos

                var registros = ObtenerDatos(solicitud, out aux);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron Asientos. Deberia reformular los parámetros de consulta.");
                }
                solicitud.Titulo = aux;
                var regs = registros.Select(x => new
                {
                   Cuenta = x.Ccb_id,
                    Denominacion = x.Ccb_desc,
                    x.Debe,
                    x.Haber,
                    x.Saldo,
                    Saldo_ant = x.Saldo_anterior,
                    x.Saldo_suma
                }).ToList();

                #endregion
                #region Scripts PDF

                anchos = [10f, 30f, 12f, 12f, 12f, 12f, 12f];
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
                    { "Saldo", regs.Sum(x => x.Saldo) },
                    { "Saldo_ant", regs.Sum(x => x.Saldo_ant) },
                    { "Saldo_suma", regs.Sum(x => x.Saldo_suma) }
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

                throw new NegocioException($"Se produjo un error al intentar generar la Impresión del Asiento {aux}. Para mayores datos ver el log.");
            }

        }


        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<BSumaSaldoRegDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Cuenta = x.Ccb_id,
                Denominacion = x.Ccb_desc,
                x.Debe,
                x.Haber,
                x.Saldo,
                Saldo_ant = x.Saldo_anterior,
                x.Saldo_suma
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<BSumaSaldoRegDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Cuenta = x.Ccb_id,
                Denominacion = x.Ccb_desc,
                x.Debe,
                x.Haber,
                x.Saldo,
                Saldo_ant = x.Saldo_anterior,
                x.Saldo_suma
            }).ToList();
            #endregion
            return GeneraFileXLS(regs, _titulos, _campos);
        }



        private List<BSumaSaldoRegDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string aux)
        {
            var eje_nro = solicitud.Parametros.GetValueOrDefault("eje_nro", "");           
            var incluirTemporales = solicitud.Parametros.GetValueOrDefault("incluirTemporales", "").ToBoolean();
            var rango = solicitud.Parametros.GetValueOrDefault("rango", "").ToBoolean();
            var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
            var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();

            aux = $"Balance de Sumas y Saldos\r\nDesde: {desde.ToShortDateString()} al {hasta.ToShortDateString()}";
            var req = new LibroFiltroDto
            {
                eje_nro = eje_nro.ToInt(),
                incluirTemporales = incluirTemporales,
                rango = rango,
                desde = desde,
                hasta = hasta
            };

            return _apiBss.ObtenerBalanceSumaSaldos(req);
        }
    }
}
