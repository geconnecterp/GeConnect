using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Libros;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R016_BalanceGeneral : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiBalanceGeneralServicio _apiBgr;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;
        public R016_BalanceGeneral(IUnitOfWork uow,
            IApiBalanceGeneralServicio bgrSv,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _apiBgr = bgrSv;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Cuenta", "Denominación", "Id Padre", "Debe", "Haber", "Saldo" };
            _campos = new List<string> { "Cuenta", "Denominacion", "IdPadre", "Debe", "Haber", "Saldo" };
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
                    throw new NegocioException($"No se encontraron datos para el Balance General. Deberia reformular los parámetros de consulta.");
                }

                // Organizamos las cuentas en estructura jerárquica con el formato exacto del ejemplo
                var registrosOrganizados = OrganizarArbolCuentasFormateado(registros);

                #endregion
                #region Scripts PDF

                // Ajustamos los anchos para que se parezcan al ejemplo
                anchos = new float[] { 60f, 20f, 20f };
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
                };

                pdf.Header = header;
                #endregion

                pdf.Open();

                #region Carga del Listado
                Paragraph subText;
                subText = HelperPdf.GeneraParrafo(solicitud.SubTitulo, titulo, Element.ALIGN_LEFT, 3, 3);
                pdf.Add(subText);

                // Títulos exactos como en el ejemplo
                List<string> titulos = new List<string> { "Denominación", "Debe", "Haber" };
                List<string> campos = new List<string> { "Denominacion", "Debe", "Haber" };

                HelperPdf.GeneraCabeceraLista(pdf, titulos, anchos, normalBold);

                var totales = new Dictionary<string, decimal>
        {
            { "Debe", registros.Sum(x => x.Debe) },
            { "Haber", registros.Sum(x => x.Haber) }
        };

                // Usamos los campos específicos para la presentación
                HelperPdf.GenerarListadoDesdeLista(pdf, registrosOrganizados, campos, anchos, chico, true, true, totales);

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
                _logger.LogError(ex, "Error en R016");
                throw new NegocioException($"Se produjo un error al intentar generar el Balance General. Para mayores datos ver el log.");
            }
        }

        /// <summary>
        /// Clase para almacenar los registros formateados del balance general
        /// </summary>
        public class BalanceGeneralFormateado
        {
            public string Denominacion { get; set; } = "";
            public decimal Debe { get; set; }
            public decimal Haber { get; set; }
        }

        /// <summary>
        /// Versión mejorada que usa una clase específica en lugar de objetos dinámicos
        /// </summary>
        private List<BalanceGeneralFormateado> OrganizarArbolCuentasFormateado(List<BalanseGrDto> registros)
        {
            var resultado = new List<BalanceGeneralFormateado>();

            // Encontrar las cuentas raíz (ccb_id_padre = "00000000")
            var raices = registros.Where(r => r.Ccb_id_padre == "00000000").OrderBy(r => r.Ccb_id).ToList();

            // Procesar cada raíz y sus descendientes con el formato exacto del ejemplo
            foreach (var raiz in raices)
            {
                resultado.Add(new BalanceGeneralFormateado
                {
                    Denominacion = $"({raiz.Ccb_id}) {raiz.Ccb_desc.ToUpper()}",
                    Debe = raiz.Debe,
                    Haber = raiz.Haber
                });

                // Procesar recursivamente los hijos
                ProcesarHijosFormateado(raiz.Ccb_id, registros, resultado, 1);
            }

            return resultado;
        }

        /// <summary>
        /// Procesa recursivamente los hijos usando la clase específica
        /// </summary>
        private void ProcesarHijosFormateado(string idPadre, List<BalanseGrDto> registros,
            List<BalanceGeneralFormateado> resultado, int nivel)
        {
            var hijos = registros
                .Where(r => r.Ccb_id_padre == idPadre && r.Ccb_id != idPadre)
                .OrderBy(r => r.Ccb_id)
                .ToList();

            foreach (var hijo in hijos)
            {
                string indentacion = new string(' ', nivel * 4);

                resultado.Add(new BalanceGeneralFormateado
                {
                    Denominacion = $"{indentacion}({hijo.Ccb_id}) {hijo.Ccb_desc}",
                    Debe = hijo.Debe,
                    Haber = hijo.Haber
                });

                ProcesarHijosFormateado(hijo.Ccb_id, registros, resultado, nivel + 1);
            }
        }


        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<BalanseGrDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Cuenta = x.Ccb_id,
                Denominacion = x.Ccb_desc,
                IdPadre = x.Ccb_id_padre,
                x.Debe,
                x.Haber,
                Saldo = x.Debe - x.Haber
            }).ToList();
            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            List<BalanseGrDto> registros = ObtenerDatos(solicitud, out ctaId);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la {solicitud.Reporte} {ctaId}.");
            }

            var regs = registros.Select(x => new
            {
                Cuenta = x.Ccb_id,
                Denominacion = x.Ccb_desc,
                IdPadre = x.Ccb_id_padre,
                x.Debe,
                x.Haber,
                Saldo = x.Debe - x.Haber
            }).ToList();
            #endregion
            return GeneraFileXLS(regs, _titulos, _campos);
        }

        private List<BalanseGrDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string id)
        {

            var eje_nro = solicitud.Parametros.GetValueOrDefault("eje_nro", "");

            id = eje_nro;
            return _apiBgr.ObtenerBalanceGeneral(eje_nro.ToInt());
        }


    }
}
