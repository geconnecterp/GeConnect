using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Servicios.Reportes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios
{
    public class ReportService : Servicio<EntidadBase>, IReportService
    {
        private readonly Dictionary<InfoReporte, IGeneradorReporte> _generadoresReporte;


        public ReportService(IUnitOfWork uow, IConsultaServicio consSv,
             IOptions<EmpresaGeco> empresa) : base(uow)
        {

            // Se inicializa el diccionario de generadores de reportes
            _generadoresReporte = new Dictionary<InfoReporte, IGeneradorReporte>
            {
                { InfoReporte.R001_InfoCtaCte, new R001_InformeCuentaCorriente(uow,consSv,empresa) },
                { InfoReporte.R002_InfoVenc, new R002_InformeVencimiento(uow,consSv) }
            };
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
