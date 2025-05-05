using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R001_InformeCuentaCorriente : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;


        public R001_InformeCuentaCorriente(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            List<string> titulos;
            float[] anchos;
            string observacion = string.Empty;
            PdfWriter? writer = null;
            Document pdf;
            try
            {
                var ms = new MemoryStream();


                #region Obteniendo registros desde la base de datos
                //Se obtienen los parámetros del reporte
                //Se obtiene la cuenta del cliente (ctaId) y la fecha desde (fechaD)
                string ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
                DateTime fechaD;
                var fechaStr = solicitud.Parametros.GetValueOrDefault("fechaD", "").ToString();
                if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out fechaD))
                {
                    throw new NegocioException("La fecha desde es inválida.");
                }
                fechaD = fechaStr.ToDateTime();
                //Se obtiene el id del usuario (userId) y se asignan los valores de pag y reg
                string userId = solicitud.Parametros.GetValueOrDefault("userId", "").ToString() ?? "";
                int pag = 1;
                int reg = 99999999;

                var registros = _consultaServicio.ConsultarCuentaCorriente(ctaId, fechaD, userId, pag, reg);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros para la cuenta corriente {ctaId}.");
                }
                #endregion


                #region Scripts PDF

                titulos = new List<string> { "Fecha", "N° Mov", "Comprobante", "Concepto", "Debe", "Haber", "Saldo" };
                anchos = [10f, 10f, 20f, 30f, 10f, 10f, 10f];
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);


                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

                #endregion

                #region Generación de Cabecera               

                PdfPTable tabla = HelperPdf.GeneraTabla(4, [10f, 20f, 50f, 20f], 100, 10, 20);

                // Columna 1: Logo
                PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
                tabla.AddCell(celdaLogo);

                // Columna 2: Datos apilados y título
                PdfPTable subTabla = new PdfPTable(1);
                subTabla.WidthPercentage = 100;


                // Datos apilados
                subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
                subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} s:{solicitud.Administracion}", chico));
                subTabla.AddCell(HelperPdf.CrearCeldaTexto($"IIBB: {_empresaGeco.IngresosBrutos}", chico));
                subTabla.AddCell(HelperPdf.CrearCeldaTexto($"Dirección: {_empresaGeco.Direccion}", chico));

                PdfPCell celdaSubTabla = new PdfPCell(subTabla)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                tabla.AddCell(celdaSubTabla);

                // Columna 3: Título del informe
                PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingTop = 10f
                };
                tabla.AddCell(celdaTitulo);

                // Columna 4: Fecha
                string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                tabla.AddCell(celdaFechaHora);

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

                #endregion

                pdf.Close();
                #endregion

                return Convert.ToBase64String(ms.ToArray());

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception)
            {
                //_logger.Log(typeof(R001_InformeCuentaCorriente), Level.Error, $"Error al generar el informe de cuenta corriente: {ex.Message}", ex);
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }

        }
    }
}
