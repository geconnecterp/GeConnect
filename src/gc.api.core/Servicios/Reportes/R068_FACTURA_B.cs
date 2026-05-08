using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R068_FACTURA_B : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiProductoFactServicio _factServicio;
        private readonly EmpresaGeco _empresaGeco;
        private readonly ILogger _logger;

        public R068_FACTURA_B(
            IUnitOfWork uow,
            IApiProductoFactServicio factServicio,
            IOptions<EmpresaGeco> empresa,
            ILogger logger) : base(uow)
        {
            _factServicio = factServicio;
            _empresaGeco = empresa.Value;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            PdfWriter? writer = null;
            Document pdf;

            try
            {
                var ms = new MemoryStream();

                #region Obtener Datos Separados
                var encabezado = ObtenerDatosFactura(solicitud);
                var productos = ObtenerDetalleProductos(solicitud);
                var datosPercepciones = ObtenerDatosPercepciones(solicitud);

                if (encabezado == null)
                {
                    throw new NegocioException("No se encontraron datos para la factura solicitada.");
                }

                if (productos == null || !productos.Any())
                {
                    throw new NegocioException("No se encontraron productos para la factura solicitada.");
                }
                #endregion

                #region Inicialización PDF
                pdf = HelperPdf.GenerarInstanciaAndInit(
                    ref writer,
                    out ms,
                    HojaSize.A4,
                    true,
                    20f, 20f, 20f, 20f
                );

                HelperPdf.ConfigurarPieDePaginaPersonalizado(
                    writer,
                    solicitud.Observacion
                );

                var logo = HelperPdf.CargaLogo(
                    solicitud.LogoPath,
                    20,
                    pdf.PageSize.Height - 10,
                    15
                );
                #endregion

                #region Fuentes
                var fuenteTitulo = HelperPdf.FontTituloBigBoldPredeterminado();
                var fuenteSubtitulo = HelperPdf.FontTituloPredeterminado();
                var fuenteNormal = HelperPdf.FontNormalPredeterminado();
                var fuenteNormalBold = HelperPdf.FontNormalPredeterminado(true);
                var fuenteChica = HelperPdf.FontChicoPredeterminado();
                #endregion

                pdf.Open();

                #region Cabecera Estática (Solo Página 1)
                GenerarCabeceraFactura(
                    pdf,
                    encabezado,
                    logo,
                    fuenteTitulo,
                    fuenteSubtitulo,
                    fuenteNormal,
                    fuenteChica
                );

                GenerarDatosEmisorReceptor(
                    pdf,
                    encabezado,
                    fuenteNormal,
                    fuenteNormalBold,
                    fuenteChica
                );
                #endregion

                #region Detalle Dinámico + Pie (Con Paginación Inteligente)
                GenerarDetalleProductosConPaginacion(
                    pdf,
                    writer,
                    productos,
                    datosPercepciones,
                    encabezado,
                    fuenteChica,
                    fuenteNormal,
                    fuenteNormalBold
                );
                #endregion

                pdf.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en R068_FACTURA_B");
                throw new NegocioException(
                    "Se produjo un error al generar la Factura B. Ver log para más detalles."
                );
            }
        }

        #region Métodos de Generación con Paginación

        /// <summary>
        /// Genera el detalle de productos con control de paginación automático
        /// y renderiza el pie al final de todos los productos (Factura B - Simplificada)
        /// </summary>
        private void GenerarDetalleProductosConPaginacion(
            Document pdf,
            PdfWriter writer,
            List<FeDetResDto> productos,
            List<FePerResDto> datosPercepciones,
            FeResDto encabezado,
            Font fuenteChica,
            Font fuenteNormal,
            Font fuenteNormalBold)
        {
            pdf.Add(new Paragraph(" ", fuenteChica));

            // Crear tabla de productos (Factura B: columnas simplificadas)
            float[] anchos = new float[] { 10f, 50f, 10f, 15f, 15f };
            PdfPTable tablaProductos = new PdfPTable(5);
            tablaProductos.WidthPercentage = 100;
            tablaProductos.SetWidths(anchos);

            // Encabezados de tabla
            string[] encabezados = new string[]
            {
                "Código",
                "Producto/Servicio",
                "Cantidad",
                "Precio\nUnitario",
                "Sub Total"
            };

            foreach (var enc in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(enc, fuenteNormalBold));
                celda.BackgroundColor = BaseColor.LightGray;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                celda.Padding = 3f;
                tablaProductos.AddCell(celda);
            }

            // Calcular altura estimada del pie (más simple que Factura A)
            float alturaPie = CalcularAlturaPie(datosPercepciones);

            // Agregar productos con control de espacio
            foreach (var producto in productos)
            {
                // Verificar si necesitamos nueva página
                if (NecesitaNuevaPagina(writer, pdf, alturaPie))
                {
                    // Agregar tabla actual al documento
                    pdf.Add(tablaProductos);

                    // Nueva página
                    pdf.NewPage();

                    // Recrear tabla con encabezados
                    tablaProductos = new PdfPTable(5);
                    tablaProductos.WidthPercentage = 100;
                    tablaProductos.SetWidths(anchos);

                    foreach (var enc in encabezados)
                    {
                        PdfPCell celda = new PdfPCell(new Phrase(enc, fuenteNormalBold));
                        celda.BackgroundColor = BaseColor.LightGray;
                        celda.HorizontalAlignment = Element.ALIGN_CENTER;
                        celda.VerticalAlignment = Element.ALIGN_MIDDLE;
                        celda.Padding = 3f;
                        tablaProductos.AddCell(celda);
                    }
                }

                // Agregar fila del producto
                // En Factura B el precio incluye IVA
                decimal precioConIva = (producto.cmd_pvta) + ((producto.cmd_pvta) * (producto.iva_alicuota ) / 100);
                
                AgregarCeldaProducto(tablaProductos, producto.p_id ?? "", fuenteChica, Element.ALIGN_CENTER);
                AgregarCeldaProducto(tablaProductos, producto.p_desc ?? "", fuenteChica, Element.ALIGN_LEFT);
                AgregarCeldaProducto(tablaProductos, (producto.cmd_cantidad ).ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, precioConIva.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, (producto.cmd_subtotal_con_iva).ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
            }

            // Agregar última tabla
            pdf.Add(tablaProductos);

            // Renderizar pie estático simplificado
            GenerarResumenTotales(
                pdf,
                datosPercepciones,
                encabezado,
                fuenteNormal,
                fuenteNormalBold
            );

            // Código de barras CAE
            if (!string.IsNullOrEmpty(encabezado.cm_cae))
            {
                GenerarCodigoBarrasCAE(
                    pdf,
                    encabezado.cm_cae,
                    encabezado.cm_cae_vto,
                    fuenteChica
                );
            }
        }

        /// <summary>
        /// Calcula la altura estimada del pie de factura B (más simple que A)
        /// </summary>
        private float CalcularAlturaPie(List<FePerResDto> datosPercepciones)
        {
            float altura = 0;

            // Espacio antes del resumen
            altura += 20f;

            // Tabla de totales simplificada (4 líneas fijas)
            altura += 60f;

            // CAE
            altura += 50f;

            // Margen de seguridad
            altura += 30f;

            return altura;
        }

        /// <summary>
        /// Determina si se necesita una nueva página
        /// </summary>
        private bool NecesitaNuevaPagina(PdfWriter writer, Document pdf, float alturaPie)
        {
            float posicionActual = writer.GetVerticalPosition(true);
            float espacioDisponible = posicionActual - pdf.BottomMargin;

            // Altura de una fila de producto + pie
            float alturaFilaProducto = 20f;
            float espacioNecesario = alturaFilaProducto + alturaPie;

            return espacioDisponible < espacioNecesario;
        }

        #endregion

        #region Métodos de Cabecera

        private void GenerarCabeceraFactura(
            Document pdf,
            FeResDto encabezado,
            Image logo,
            Font fuenteTitulo,
            Font fuenteSubtitulo,
            Font fuenteNormal,
            Font fuenteChica)
        {
            PdfPTable tablaCabecera = new PdfPTable(3);
            tablaCabecera.WidthPercentage = 100;
            tablaCabecera.SetWidths(new float[] { 30f, 40f, 30f });

            // Columna 1: Logo y Razón Social
            PdfPTable tablaLogoRazon = new PdfPTable(1);
            tablaLogoRazon.WidthPercentage = 100;

            PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
            celdaLogo.Border = Rectangle.NO_BORDER;
            celdaLogo.HorizontalAlignment = Element.ALIGN_CENTER;
            tablaLogoRazon.AddCell(celdaLogo);

            AgregarCeldaSinBorde(
                tablaLogoRazon,
                $"Razón Social: {encabezado.emp_razon_social}",
                fuenteChica,
                Element.ALIGN_LEFT
            );

            PdfPCell celdaColumna1 = new PdfPCell(tablaLogoRazon);
            celdaColumna1.Border = Rectangle.BOX;
            celdaColumna1.Padding = 5f;
            tablaCabecera.AddCell(celdaColumna1);

            // Columna 2: Letra
            PdfPTable tablaLetra = new PdfPTable(1);
            tablaLetra.WidthPercentage = 100;

            PdfPCell celdaLetra = new PdfPCell(new Phrase(encabezado.tco_letra ?? "", fuenteTitulo));
            celdaLetra.Border = Rectangle.NO_BORDER;
            celdaLetra.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLetra.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaLetra.PaddingTop = 10f;
            tablaLetra.AddCell(celdaLetra);

            PdfPCell celdaTipo = new PdfPCell(new Phrase($"COD. {encabezado.tco_id}", fuenteChica));
            celdaTipo.Border = Rectangle.NO_BORDER;
            celdaTipo.HorizontalAlignment = Element.ALIGN_CENTER;
            tablaLetra.AddCell(celdaTipo);

            PdfPCell celdaContenedoraLetra = new PdfPCell(tablaLetra);
            celdaContenedoraLetra.Border = Rectangle.BOX;
            celdaContenedoraLetra.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaContenedoraLetra);

            // Columna 3: Datos del comprobante
            PdfPTable tablaDatos = new PdfPTable(1);
            tablaDatos.WidthPercentage = 100;

            AgregarCeldaSinBorde(tablaDatos, "TICKET FACTURA", fuenteSubtitulo, Element.ALIGN_CENTER);
            AgregarCeldaSinBorde(tablaDatos, $"Punto de Venta: {encabezado.adm_id}    Comp. Nro: {encabezado.cm_compte}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Emisión: {encabezado.cm_fecha.ToString("dd/MM/yyyy")}", fuenteChica, Element.ALIGN_LEFT);

            PdfPCell celdaContenedoraDatos = new PdfPCell(tablaDatos);
            celdaContenedoraDatos.Border = Rectangle.BOX;
            celdaContenedoraDatos.Padding = 5f;
            tablaCabecera.AddCell(celdaContenedoraDatos);

            pdf.Add(tablaCabecera);
        }

        private void GenerarDatosEmisorReceptor(
            Document pdf,
            FeResDto encabezado,
            Font fuenteNormal,
            Font fuenteNormalBold,
            Font fuenteChica)
        {
            pdf.Add(new Paragraph(" ", fuenteChica));

            PdfPTable tablaDatosGenerales = new PdfPTable(1);
            tablaDatosGenerales.WidthPercentage = 100;

            // Datos del Emisor en una fila
            PdfPTable tablaEmisor = new PdfPTable(2);
            tablaEmisor.WidthPercentage = 100;
            tablaEmisor.SetWidths(new float[] { 25f, 75f });

            AgregarFilaDatos(
                tablaEmisor,
                "Domicilio Comercial:",
                $"{encabezado.emp_domicilio}, {encabezado.adm_direccion}",
                fuenteNormalBold,
                fuenteNormal
            );
            AgregarFilaDatos(
                tablaEmisor,
                "Condición frente al IVA:",
                encabezado.afip_desc_emp ?? "",
                fuenteNormalBold,
                fuenteNormal
            );

            PdfPCell celdaEmisor = new PdfPCell(tablaEmisor);
            celdaEmisor.Border = Rectangle.BOX;
            celdaEmisor.Padding = 5f;
            tablaDatosGenerales.AddCell(celdaEmisor);

            pdf.Add(tablaDatosGenerales);

            // Datos del Cliente
            pdf.Add(new Paragraph(" ", fuenteChica));

            PdfPTable tablaCliente = new PdfPTable(2);
            tablaCliente.WidthPercentage = 100;
            tablaCliente.SetWidths(new float[] { 25f, 75f });

            AgregarFilaDatos(tablaCliente, "CUIT:", encabezado.cm_cuit ?? "", fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Apellido y Nombre / Razón Social:", encabezado.cm_nombre ?? "", fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición frente al IVA:", encabezado.afip_desc_cli ?? "", fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Domicilio Comercial:", encabezado.cm_domicilio ?? "", fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición de Venta:", "", fuenteNormalBold, fuenteNormal);

            PdfPCell celdaCliente = new PdfPCell(tablaCliente);
            celdaCliente.Border = Rectangle.BOX;
            celdaCliente.Padding = 5f;

            PdfPTable wrapper = new PdfPTable(1);
            wrapper.WidthPercentage = 100;
            wrapper.AddCell(celdaCliente);

            pdf.Add(wrapper);
        }

        #endregion

        #region Métodos de Pie

        private void GenerarResumenTotales(
            Document pdf,
            List<FePerResDto> datosPercepciones,
            FeResDto encabezado,
            Font fuenteNormal,
            Font fuenteNormalBold)
        {
            pdf.Add(new Paragraph(" ", fuenteNormal));

            // Tabla de totales alineada a la derecha (simplificada para Factura B)
            PdfPTable tablaTotales = new PdfPTable(2);
            tablaTotales.WidthPercentage = 50;
            tablaTotales.HorizontalAlignment = Element.ALIGN_RIGHT;
            tablaTotales.SetWidths(new float[] { 60f, 40f });

            // Sub Total
            decimal subTotal = (encabezado.cm_gravado ) + 
                             (encabezado.cm_no_gravado ) + 
                             (encabezado.cm_exento ) + 
                             (encabezado.cm_iva );
            
            AgregarLineaTotal(
                tablaTotales,
                "Sub Total:",
                subTotal,
                fuenteNormal,
                Element.ALIGN_RIGHT
            );

            // Importe Otros Tributos (Percepciones)
            decimal totalPercepciones = datosPercepciones?.Sum(p => p.percepcion ?? 0) ?? 0;
            AgregarLineaTotal(
                tablaTotales,
                "Importe Otros Tributos:",
                totalPercepciones,
                fuenteNormal,
                Element.ALIGN_RIGHT
            );

            // Recargo/Descuento
            decimal recDto = encabezado.cm_dto;
            AgregarLineaTotal(
                tablaTotales,
                "Rec/Dto:",
                recDto,
                fuenteNormal,
                Element.ALIGN_RIGHT
            );

            // Total
            AgregarLineaTotal(
                tablaTotales,
                "Importe Total:",
                encabezado.cm_total ,
                fuenteNormalBold,
                Element.ALIGN_RIGHT,
                BaseColor.LightGray
            );

            pdf.Add(tablaTotales);
        }

        private void GenerarCodigoBarrasCAE(
            Document pdf,
            string cae,
            DateTime? caeVencimiento,
            Font fuenteChica)
        {
            pdf.Add(new Paragraph(" ", fuenteChica));

            try
            {
                var fuente3o9 = HelperPdf.DefineFontWithStyleFromFile(
                    _empresaGeco.Font3o9Name,
                    14,
                    Font.NORMAL,
                    0, 0, 0
                );

                PdfPTable tablaCAE = new PdfPTable(2);
                tablaCAE.WidthPercentage = 100;
                tablaCAE.SetWidths(new float[] { 70f, 30f });

                string codigoBarras = $"*{cae}*";
                Phrase phraseBarras = new Phrase();
                phraseBarras.Add(new Chunk(codigoBarras, fuente3o9));

                PdfPCell celdaBarras = new PdfPCell(phraseBarras);
                celdaBarras.Border = Rectangle.NO_BORDER;
                celdaBarras.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaBarras.VerticalAlignment = Element.ALIGN_MIDDLE;
                tablaCAE.AddCell(celdaBarras);

                PdfPTable tablaInfo = new PdfPTable(1);
                tablaInfo.WidthPercentage = 100;

                AgregarCeldaSinBorde(tablaInfo, "Pág. 1 of 1", fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaSinBorde(tablaInfo, $"CAE N°: {cae}", fuenteChica, Element.ALIGN_RIGHT);

                if (caeVencimiento.HasValue)
                {
                    AgregarCeldaSinBorde(tablaInfo, $"Fecha de Vto. de CAE: {caeVencimiento.Value:dd/MM/yyyy}", fuenteChica, Element.ALIGN_RIGHT);
                }

                PdfPCell celdaInfo = new PdfPCell(tablaInfo);
                celdaInfo.Border = Rectangle.NO_BORDER;
                tablaCAE.AddCell(celdaInfo);

                pdf.Add(tablaCAE);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo generar el código de barras para CAE: {CAE}", cae);
            }
        }

        #endregion

        #region Métodos Auxiliares

        private void AgregarCeldaSinBorde(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 2f;
            tabla.AddCell(celda);
        }

        private void AgregarFilaDatos(PdfPTable tabla, string etiqueta, string valor, Font fuenteEtiqueta, Font fuenteValor)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuenteEtiqueta));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaEtiqueta.Padding = 2f;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(valor, fuenteValor));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaValor.Padding = 2f;
            tabla.AddCell(celdaValor);
        }

        private void AgregarCeldaProducto(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.BOTTOM_BORDER;
            celda.BorderColor = BaseColor.LightGray;
            celda.HorizontalAlignment = alineacion;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.Padding = 3f;
            tabla.AddCell(celda);
        }

        private void AgregarLineaTotal(PdfPTable tabla, string etiqueta, decimal valor, Font fuente, int alineacion, BaseColor? backgroundColor = null)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuente));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = alineacion;
            celdaEtiqueta.Padding = 3f;
            if (backgroundColor != null)
                celdaEtiqueta.BackgroundColor = backgroundColor;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(valor.ToString("N2"), fuente));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaValor.Padding = 3f;
            if (backgroundColor != null)
                celdaValor.BackgroundColor = backgroundColor;
            tabla.AddCell(celdaValor);
        }

        #endregion

        #region Métodos de Obtención de Datos

        private FeResDto ObtenerDatosFactura(ReporteSolicitudDto solicitud)
        {
            try
            {
                var reqDto = new FeReqDto
                {
                    tco_id = solicitud.Parametros.GetValueOrDefault("tco_id", "")?.ToString() ?? "",
                    cm_compte = solicitud.Parametros.GetValueOrDefault("cm_compte", "")?.ToString() ?? "",
                    cm_repetido = solicitud.Parametros.GetValueOrDefault("cm_repetido", "0")?.ToString() ?? "0"
                };

                var lista = _factServicio.ObtenerFE(reqDto);
                return lista?.FirstOrDefault()
                    ?? throw new NegocioException("No se encontró el encabezado de la factura.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de factura");
                throw new NegocioException("No se pudieron obtener los datos de la factura. Ver log para detalles.");
            }
        }

        private List<FeDetResDto> ObtenerDetalleProductos(ReporteSolicitudDto solicitud)
        {
            try
            {
                var reqDto = new FeReqDto
                {
                    tco_id = solicitud.Parametros.GetValueOrDefault("tco_id", "")?.ToString() ?? "",
                    cm_compte = solicitud.Parametros.GetValueOrDefault("cm_compte", "")?.ToString() ?? "",
                    cm_repetido = solicitud.Parametros.GetValueOrDefault("cm_repetido", "0")?.ToString() ?? "0"
                };

                return _factServicio.ObtenerFEDetalle(reqDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle de productos");
                throw new NegocioException("No se pudieron obtener los productos de la factura. Ver log para detalles.");
            }
        }

        private List<FePerResDto> ObtenerDatosPercepciones(ReporteSolicitudDto solicitud)
        {
            try
            {
                var reqDto = new FeReqDto
                {
                    tco_id = solicitud.Parametros.GetValueOrDefault("tco_id", "")?.ToString() ?? "",
                    cm_compte = solicitud.Parametros.GetValueOrDefault("cm_compte", "")?.ToString() ?? "",
                    cm_repetido = solicitud.Parametros.GetValueOrDefault("cm_repetido", "0")?.ToString() ?? "0"
                };

                return _factServicio.ObtenerFEPer(reqDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de percepciones");
                return new List<FePerResDto>();
            }
        }

        #endregion

        #region Implementación de IGeneradorReporte

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La generación de factura en formato TXT no está implementada.");
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La generación de factura en formato XLS no está implementada.");
        }

        #endregion
    }
}
