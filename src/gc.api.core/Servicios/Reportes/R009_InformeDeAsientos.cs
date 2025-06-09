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
    public class R009_InformeDeAsientos : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IAsientoTemporalServicio _asTempSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R009_InformeDeAsientos(IUnitOfWork uow, IAsientoTemporalServicio atempsv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _asTempSv = atempsv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Nro.Mov.", "Fecha", "Descripcion", "Carga", "Estado" };
            _campos = new List<string> { "Movimiento", "Fecha", "Descripcion", "Carga", "Estado" };
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

                var asientos = ObtenerDatos(solicitud, out ctaId);

                if (asientos == null || asientos.Count == 0)
                {
                    throw new NegocioException($"No se encontraron Asientos. Deberia reformular los parámetros de consulta.");
                }

                var regs = asientos.Select(x => new
                {
                    Movimiento = string.IsNullOrEmpty(x.dia_movi) ? "0000-00000000" : x.dia_movi,
                    Fecha = x.dia_fecha.ToShortDateString(),
                    Descripcion = $"({x.dia_tipo}) {x.dia_desc_asiento}",
                    Carga = x.dia_fecha_sistema,
                    Estado = x.revisable //? "X" : "OK"
                }).ToList();

                #endregion
                #region Scripts PDF

                anchos = [15f, 10f, 55f, 10f, 10f];
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

                PdfPTable tabla = GeneraCabeceraPDF2(solicitud, chico, titulo, logo,_empresaGeco);

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

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);

                
                HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico,true,false,null,true,HelperPdf.BooleanDisplayFormat.XOk,false);

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
            List<AsientoGridDto> asientos = ObtenerDatos(solicitud, out ctaId);

            if (asientos == null || asientos.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = asientos.Select(x => new
            {
                Movimiento = string.IsNullOrEmpty(x.dia_movi) ? "0000-00000000" : x.dia_movi,
                Fecha = x.dia_fecha.ToShortDateString(),
                Descripcion = $"({x.dia_tipo}) {x.dia_desc_asiento}",
                Carga = x.dia_fecha_sistema,
                Estado = x.revisable ? "X" : "OK"
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }



        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<AsientoGridDto> asientos = ObtenerDatos(solicitud, out ctaId);

            if (asientos == null || asientos.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = asientos.Select(x => new
            {
                Movimiento = string.IsNullOrEmpty(x.dia_movi) ? "0000-00000000" : x.dia_movi,
                Fecha = x.dia_fecha.ToShortDateString(),
                Descripcion = $"({x.dia_tipo}) {x.dia_desc_asiento}",
                Carga = x.dia_fecha_sistema,
                Estado = x.revisable ? "X" : "OK"
            }).ToList();
            #endregion
            return GeneraFileXLS(regs, _titulos, _campos);
        }



        private List<AsientoGridDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
        {

            var Eje_nro = solicitud.Parametros.GetValueOrDefault("Eje_nro", "");
            var Movi = solicitud.Parametros.GetValueOrDefault("Movi", "").ToBoolean();
            var Movi_like = solicitud.Parametros.GetValueOrDefault("Movi_like", "") ?? "";
            var Usu = solicitud.Parametros.GetValueOrDefault("Usu", "").ToBoolean();
            var Usu_like = solicitud.Parametros.GetValueOrDefault("Usu_like", "") ?? "";
            var Tipo = solicitud.Parametros.GetValueOrDefault("Tipo", "").ToBoolean();
            var Tipo_like = solicitud.Parametros.GetValueOrDefault("Tipo_like", "") ?? "";
            var Rango = solicitud.Parametros.GetValueOrDefault("Rango", "").ToBoolean();
            var Desde = solicitud.Parametros.GetValueOrDefault("Desde", "").ToDateTime();
            var Hasta = solicitud.Parametros.GetValueOrDefault("Hasta", "").ToDateTime();


            //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
            string dia_movi = solicitud.Parametros.GetValueOrDefault("dia_movi", "") ?? "";
            id = Movi_like;
            return _asTempSv.ObtenerAsientos(new QueryAsiento
            {
                Movi_like = Movi_like,
                Eje_nro = Eje_nro.ToInt(),
                Movi = Movi,
                Usu = Usu,
                Usu_like = Usu_like,
                Tipo = Tipo,
                Tipo_like = Tipo_like,
                Rango = Rango,
                Desde = Desde,
                Hasta = Hasta,
                TotalRegistros = 999999999,
                Paginas = 1
            });
        }
    }
}
