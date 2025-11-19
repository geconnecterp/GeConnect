using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R047_Etiquetas02Precios : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiEtiquetaServicio _etSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R047_Etiquetas02Precios(IUnitOfWork uow, IApiEtiquetaServicio servicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _etSv = servicio;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Descripción","Oferta"  };
            _campos = new List<string> { "codigo", "descripcion","oferta" };
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            PdfWriter? writer = null;
            Document pdf;

            try
            {
                var ms = new MemoryStream();
                #region Obteniendo registros desde la base de datos
                string adm_desc;
                string adm_id;
                List<EtiquetaDto> etiquetas = ObtenerDatos(solicitud, out adm_desc, out adm_id);
                #endregion

                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true, 5f, 20f, 5f, 5f);
                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10,7);

                #endregion

                // Definir fuentes para diferentes secciones
                var fuenteDescripcion = HelperPdf.DefineFontWithStyle("Arial", 11, Font.BOLD, 0, 0, 0);
                var fuentePrecioDecimal = HelperPdf.DefineFontWithStyle("Arial Black", 26, Font.BOLD, 0, 0, 0);
                var fuentePrecioDecimal2 = HelperPdf.DefineFontWithStyle("Arial Black", 20, Font.BOLD, 0, 0, 0);
                var fuenteMini = HelperPdf.DefineFontWithStyle("Arial", 8, Font.NORMAL, 0, 0, 0);
                var fuente3o9 = HelperPdf.DefineFontWithStyleFromFile(_empresaGeco.Font3o9Name, 14, Font.NORMAL, 0, 0, 0);
                pdf.Open();

                #region Generar etiquetas en formato 2x6
                int etiquetasPorPagina = 12;              

                for (int pagina = 0; pagina < Math.Ceiling((double)etiquetas.Count / etiquetasPorPagina); pagina++)
                {
                    if (pagina > 0)
                    {
                        pdf.NewPage();
                    }

                    PdfPTable tablaPrincipal = HelperPdf.GeneraTabla(2, [50f,50f], 100,0, 0);


                    int inicioIndex = pagina * etiquetasPorPagina;
                    int finIndex = Math.Min(inicioIndex + etiquetasPorPagina, etiquetas.Count);

                    for (int i = inicioIndex; i < finIndex; i++)
                    {
                        var etiqueta = etiquetas[i];

                        // Crear celda de etiqueta
                        PdfPCell celdaEtiqueta = GenerarCeldaEtiqueta(
                            etiqueta,
                            logo,
                            fuenteDescripcion,
                            fuentePrecioDecimal,
                            fuentePrecioDecimal2,
                            fuenteMini,
                            fuente3o9
                        );
                        celdaEtiqueta.Border = Rectangle.BOX;
                        celdaEtiqueta.BorderWidth = 0.5f;
                        celdaEtiqueta.BorderColor = BaseColor.Black;

                        tablaPrincipal.AddCell(celdaEtiqueta);
                    }

                    // Rellenar celdas vacías si es necesario
                    int celdasUsadas = finIndex - inicioIndex;
                    int celdasFaltantes = etiquetasPorPagina - celdasUsadas;

                    for (int i = 0; i < celdasFaltantes; i++)
                    {
                        PdfPCell celdaVacia = new PdfPCell(new Phrase(""));
                        celdaVacia.Border = Rectangle.BOX;
                        celdaVacia.BorderWidth = 0.5f;
                        celdaVacia.BorderColor = BaseColor.LightGray;
                        celdaVacia.MinimumHeight = 105f; // Altura aproximada por etiqueta
                        tablaPrincipal.AddCell(celdaVacia);
                    }

                    pdf.Add(tablaPrincipal);
                }
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
                _logger.LogError(ex, "Error en R032");
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }
        }

        /// <summary>
        /// Genera una celda individual de etiqueta con todos los datos
        /// </summary>
        private PdfPCell GenerarCeldaEtiqueta(
            EtiquetaDto etiqueta,
            Image logo,
            Font fuenteDescripcion,
            Font fuentePrecioDecimal,
            Font fuentePrecioDecimal2,
            Font fuenteMini,
            Font fuente3o9
            )
        {
            // Tabla interna para la etiqueta (1 columna)
            PdfPTable tablaEtiqueta = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);
            
            // FILA 1: Logo y Fecha, sin iva, leyenda y precio
            tablaEtiqueta.AddCell(GenerarFilaPrecio1(etiqueta, fuentePrecioDecimal, fuenteMini, fuenteDescripcion,logo));
            // FILA 2: Precio
            tablaEtiqueta.AddCell(GenerarFilaPrecio2(etiqueta, fuentePrecioDecimal2, fuenteMini, fuenteDescripcion));

            // FILA 3: Descripción del producto
            tablaEtiqueta.AddCell(GenerarFilaDescripcion(etiqueta.p_desc, fuenteDescripcion));


            // FILA 4: Código, Código de barras y Código barrado
            tablaEtiqueta.AddCell(GenerarFilaCodigos(
                etiqueta.p_id,
                etiqueta.p_id_barrado,
                fuenteMini,
                fuente3o9
            ));

            // Celda contenedora
            PdfPCell celdaContenedora = new PdfPCell(tablaEtiqueta);
            celdaContenedora.Border = Rectangle.NO_BORDER;
            celdaContenedora.BorderWidth = 0.5f;
            celdaContenedora.BorderColor = BaseColor.LightGray;
            celdaContenedora.Padding = 3f;
            celdaContenedora.MinimumHeight = 105f; // Altura para 7 filas en A4

            return celdaContenedora;
        }

        private PdfPTable GenerarFilaPrecio2(EtiquetaDto etiqueta, Font fuentePrecioDecimal, Font fuenteMini, Font fuenteDescripcion)
        {
            PdfPTable tablaEtiqueta = HelperPdf.GeneraTabla(2, [50f, 50f], 100, 0, 0);

            PdfPTable subtabla = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);
            subtabla.AddCell(GenerarFilaSinIva(etiqueta.p_pneto2, fuenteMini));
            subtabla.AddCell(GenerarFilaDescripcion(etiqueta.p_pvta_leyenda2, fuenteDescripcion));
            PdfPCell celdaSubtabla = new(subtabla);
            celdaSubtabla.PaddingTop = 0f;
            celdaSubtabla.PaddingBottom = 0f;
            celdaSubtabla.PaddingRight = 5f;
            celdaSubtabla.Border = Rectangle.NO_BORDER;

            tablaEtiqueta.AddCell(celdaSubtabla);
 
            string precioTexto = etiqueta.p_pvta2.ToString("$#,##0.00", new System.Globalization.CultureInfo("es-AR"));

            PdfPCell celda = new PdfPCell(new Phrase(precioTexto, fuentePrecioDecimal));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = Element.ALIGN_RIGHT;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.PaddingTop = 0f;
            celda.PaddingBottom = 0f;
            celda.PaddingRight = 5f;

            tablaEtiqueta.AddCell(celda);
            return tablaEtiqueta;
        }

        private PdfPCell GenerarFilaLogoFecha(Image logo, Font fuenteMini)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(2, [10f, 90f], 100, 0, 0);

            // Logo a la izquierda
            //PdfPCell celdaLogo = new PdfPCell(logo, true);
            //celdaLogo.Border = Rectangle.NO_BORDER;
            //celdaLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            //celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
            //celdaLogo.FixedHeight = 12f;
            //tabla.AddCell(celdaLogo);
            PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
            tabla.AddCell(celdaLogo);

            // Fecha a la derecha
            string fecha = DateTime.Now.ToString("dd/MM/yy");
            PdfPCell celdaFecha = new PdfPCell(new Phrase(fecha, fuenteMini));
            celdaFecha.Border = Rectangle.NO_BORDER;
            celdaFecha.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaFecha.VerticalAlignment = Element.ALIGN_MIDDLE;
            tabla.AddCell(celdaFecha);

            PdfPCell celda = new PdfPCell(tabla);
            celda.Border = Rectangle.NO_BORDER;
            celda.PaddingBottom = 0f;

            return celda;
        }

        /// <summary>
        /// FILA 2: Precio
        /// </summary>
        private PdfPTable GenerarFilaPrecio1(EtiquetaDto etiqueta, Font fuentePrecio, Font fuenteMini, Font fuenteDescripcion, Image logo)
        {
            PdfPTable tablaEtiqueta = HelperPdf.GeneraTabla(2, [50f,50f], 100, 0, 0);
            

            PdfPTable subtabla = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);
            // Logo y fecha
            subtabla.AddCell(GenerarFilaLogoFecha(logo, fuenteMini));
            subtabla.AddCell(GenerarFilaSinIva(etiqueta.p_pneto, fuenteMini));
            subtabla.AddCell(GenerarFilaDescripcion(etiqueta.p_pvta_leyenda, fuenteDescripcion));
            PdfPCell celdaSubtabla = new(subtabla);
            celdaSubtabla.PaddingTop = 0f;
            celdaSubtabla.PaddingBottom = 0f;
            celdaSubtabla.PaddingRight = 5f;
            celdaSubtabla.Border = Rectangle.NO_BORDER;

            tablaEtiqueta.AddCell(celdaSubtabla);

            string precioTexto = etiqueta.p_pvta.ToString("$#,##0.00", new System.Globalization.CultureInfo("es-AR"));

            PdfPCell celda = new PdfPCell(new Phrase(precioTexto, fuentePrecio));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = Element.ALIGN_RIGHT;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.PaddingTop = 1f;
            celda.PaddingBottom = 1f;
            celda.PaddingRight = 5f;

            tablaEtiqueta.AddCell(celda);
            return tablaEtiqueta;
        }

        /// <summary>
        /// FILA 3: S/IVA
        /// </summary>
        private PdfPCell GenerarFilaSinIva(decimal pNeto, Font fuenteMini)
        {
            string texto = $"S/IVA: ${pNeto:0.00}";

            PdfPCell celda = new PdfPCell(new Phrase(texto, fuenteMini));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = Element.ALIGN_LEFT;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.PaddingBottom = 2f;
            celda.PaddingRight = 5f;

            return celda;
        }

        /// <summary>
        /// FILA 4: Descripción del producto
        /// </summary>
        private PdfPCell GenerarFilaDescripcion(string descripcion, Font fuenteDescripcion)
        {
            // Limitar la descripción a un máximo de caracteres para que entre en la celda
            string descCorta = descripcion.Length > 35
                ? descripcion.Substring(0, 32) + "..."
                : descripcion;

            PdfPCell celda = new PdfPCell(new Phrase(descCorta, fuenteDescripcion));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = Element.ALIGN_LEFT;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.PaddingTop = 2f;
            celda.PaddingBottom = 2f;
            celda.PaddingLeft = 2f;

            return celda;
        }

        /// <summary>
        /// FILA 5: Códigos (ID, Código de barras, ID barrado)
        /// </summary>
        private PdfPCell GenerarFilaCodigos(
            string pId,
            string pIdBarrado,
            Font fuenteMini,
            Font fuente3o9)
        {
            var fuenteMiniPlus = HelperPdf.DefineFontWithStyle("Arial", 10, Font.NORMAL, 0, 0, 0);


            PdfPTable tabla = new PdfPTable(3);
            tabla.WidthPercentage = 100;
            tabla.SetWidths(new float[] { 25f, 50f, 25f });

            // Código a la izquierda
            PdfPCell celdaCodigo = new PdfPCell(new Phrase(pId, fuenteMini));
            celdaCodigo.Border = Rectangle.NO_BORDER;
            celdaCodigo.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaCodigo.VerticalAlignment = Element.ALIGN_BOTTOM;
            celdaCodigo.PaddingLeft = 2f;
            tabla.AddCell(celdaCodigo);

            if (!string.IsNullOrEmpty(pIdBarrado))
            {
                // Código de barras en el centro (con asteriscos)
                string codigoBarras = $"*{pIdBarrado}*";
                //armando la Phrase con chunks
                Phrase phraseBarras =
                [
                    // Asterisco inicial con fuente normal
                    //new Chunk("*", fuenteMiniPlus),
                    // Código en fuente 3of9 (esto genera las barras)
                    new Chunk(codigoBarras, fuente3o9),
                    // Asterisco final con fuente normal
                    //new Chunk("*", fuenteMiniPlus),
                ];

                PdfPCell celdaBarras = new(phraseBarras)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_BOTTOM
                };
                tabla.AddCell(celdaBarras);

                // Código barrado a la derecha
                PdfPCell celdaBarrado = new PdfPCell(new Phrase(pIdBarrado, fuenteMini));
                celdaBarrado.Border = Rectangle.NO_BORDER;
                celdaBarrado.HorizontalAlignment = Element.ALIGN_RIGHT;
                celdaBarrado.VerticalAlignment = Element.ALIGN_BOTTOM;
                celdaBarrado.PaddingRight = 2f;
                tabla.AddCell(celdaBarrado);
            }

            PdfPCell celda = new PdfPCell(tabla);
            celda.Border = Rectangle.NO_BORDER;
            celda.PaddingTop = 2f;

            return celda;
        }

        private List<EtiquetaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string adm_desc, out string adm_id)
        {
            //buscamos los parametros
            var prods = solicitud.Parametros.GetValueOrDefault("json_p", "{}").ToString() ?? "{}";
            var et = solicitud.Parametros.GetValueOrDefault("etiqueta", "0").ToString().ToInt();
            var adm = solicitud.Parametros.GetValueOrDefault("adm_id", "").ToString() ?? "";
            var usu = solicitud.Parametros.GetValueOrDefault("usu_id", "").ToString() ?? "";

            var a = adm.Split("#");
            adm_id = a.Length > 0 ? a[0] : "";
            adm_desc = a.Length > 1 ? a[1] : "";

            List<EtiquetaDto> etiquetas = [];

            //invocamos el servicio para obtener los datos para la etiqueta
            etiquetas = _etSv.ObtenerDatosParaEtiqueta(prods, et, adm, usu);

            return etiquetas;
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }
    }
}
