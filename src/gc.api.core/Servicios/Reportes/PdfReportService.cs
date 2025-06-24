using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Asientos;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios.Reportes
{
    public class ReportService : Servicio<EntidadBase>, IReportService
    {
        private readonly Dictionary<InfoReporte, IGeneradorReporte> _generadoresReporte;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork uow, IConsultaServicio consSv,
            IAsientoTemporalServicio asiento,
            IApiLMayorServicio apiLMayor,
            IApiLDiarioServicio ldSv,
            IApiSumaSaldoServicio apiBSS,
             IOptions<EmpresaGeco> empresa, ICuentaServicio ctaSv, ILogger<ReportService> logger) : base(uow)
        {

            // Se inicializa el diccionario de generadores de reportes
            _generadoresReporte = new Dictionary<InfoReporte, IGeneradorReporte>
            {
                { InfoReporte.R001_InfoCtaCte, new R001_InformeCuentaCorriente(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R002_InfoVenc, new R002_InformeVencimiento(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R003_InfoCmpte, new R003_InformeComprobantes(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R004_InfoCmpteDet, new R004_InformeComprobanteDetalle(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R005_InfoOPago, new R005_InformeOPago(uow,consSv,empresa,ctaSv,logger) },
                { InfoReporte.R006_InfoOPagoDet, new R006_InformeOPagoDetalle(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R007_InfoRecProv, new R007_InformeRecepcionProveedor(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R008_InfoRecProvDet, new R008_InformeRecepcionProveedorDetalle(uow,consSv,empresa,ctaSv, logger) },
                { InfoReporte.R009_InfoAsientos, new R009_InformeDeAsientos(uow,asiento,empresa,ctaSv, logger) },
                { InfoReporte.R010_InfoDetalleAsiento, new R010_DetalleDeAsiento(uow,asiento,empresa,ctaSv, logger) },
                { InfoReporte.R011_LibroMayorContable, new R011_LibroMayorContable(uow,apiLMayor,empresa,ctaSv, logger) },
                { InfoReporte.R012_ResumenLibroMayorContable, new R012_ResumenLibroMayorContable(uow,apiLMayor,empresa,ctaSv, logger) },
                { InfoReporte.R013_LibroDiarioXCuenta, new R013_LibroDiarioXCuenta(uow,ldSv,empresa,ctaSv, logger) },
                { InfoReporte.R014_BalanceSumasSaldos, new R014_BalanceSumasSaldos(uow,apiBSS,empresa,ctaSv, logger) },
                { InfoReporte.R015_LibroDiarioResumen, new R015_LibroDiarioResumen(uow,ldSv,empresa,ctaSv, logger) },
            }; 
            _logger = logger;
        }

        public string GenerarReporteFormatoExcel(ReporteSolicitudDto solicitud)
        {
            string base64 = string.Empty;

            if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
            {
                base64 = generador.GenerarXls(solicitud);
            }
            else
            {
                StringBuilder str = new StringBuilder();
                str.Append("No se pudo identificar el XLS a generar.");
                foreach (var param in solicitud.Parametros)
                {
                    str.Append($"{param.Key}: {param.Value}");
                }
                throw new Exception(str.ToString());
            }

            return base64;
        }

        public string GenerarReporteFormatoTxt(ReporteSolicitudDto solicitud)
        {
            string base64 = string.Empty;

            if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
            {
                base64 = generador.GenerarTxt(solicitud);
            }
            else
            {
                StringBuilder str = new StringBuilder();
                str.Append("No se pudo identificar el TXT a generar.");
                foreach (var param in solicitud.Parametros)
                {
                    str.Append($"{param.Key}: {param.Value}");
                }
                throw new Exception(str.ToString());
            }

            return base64;
        }

        public string GenerateReportAsBase64(ReporteSolicitudDto solicitud)
        {
            try
            {
                string base64 = string.Empty;

                if (_generadoresReporte.TryGetValue(solicitud.Reporte, out var generador))
                {
                    base64 = generador.Generar(solicitud);
                }
                else
                {
                    using (var ms = new MemoryStream())
                    { //genera un pdf generico
                        Document document = new Document();
                        PdfWriter.GetInstance(document, ms);
                        document.Open();

                        document.Add(new Paragraph("No se pudo identificar el reporte a generar."));
                        foreach (var param in solicitud.Parametros)
                        {
                            document.Add(new Paragraph($"{param.Key}: {param.Value}"));
                        }
                        document.Close();
                        base64 = Convert.ToBase64String(ms.ToArray());
                    }
                }

                return base64;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
