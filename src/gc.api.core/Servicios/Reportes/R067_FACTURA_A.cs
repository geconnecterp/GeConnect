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
    public class R067_FacturaA : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiProductoFactServicio _factServicio;
        private readonly EmpresaGeco _empresaGeco;
        private readonly ILogger _logger;

        public R067_FacturaA(
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
                var datosIva = ObtenerDatosIva(solicitud);
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
                    datosIva,
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
                _logger.LogError(ex, "Error en R048_FacturaA");
                throw new NegocioException(
                    "Se produjo un error al generar la Factura A. Ver log para más detalles."
                );
            }
        }

        #region Métodos de Generación con Paginación

        private void GenerarDetalleProductosConPaginacion(
            Document pdf,
            PdfWriter writer,
            List<FeDetResDto> productos,
            List<FeIvaResDto> datosIva,
            List<FePerResDto> datosPercepciones,
            FeResDto encabezado,
            Font fuenteChica,
            Font fuenteNormal,
            Font fuenteNormalBold)
        {
            pdf.Add(new Paragraph(" ", fuenteChica));

            float[] anchos = new float[] { 8f, 32f, 8f, 10f, 10f, 5f, 8f, 10f, 9f };
            PdfPTable tablaProductos = new PdfPTable(9);
            tablaProductos.WidthPercentage = 100;
            tablaProductos.SetWidths(anchos);

            string[] encabezados = new string[]
            {
                "Código", "Producto/Servicio", "Cantidad",
                "Precio\nUnitario", "Subtotal", "II",
                "Alí.\nIVA", "Boni. con\nIVA", "Sub Total\ncon IVA"
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

            float alturaPie = CalcularAlturaPie(datosIva, datosPercepciones);

            foreach (var producto in productos)
            {
                if (NecesitaNuevaPagina(writer, pdf, alturaPie))
                {
                    pdf.Add(tablaProductos);
                    pdf.NewPage();

                    tablaProductos = new PdfPTable(9);
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

                AgregarCeldaProducto(tablaProductos, producto.p_id, fuenteChica, Element.ALIGN_CENTER);
                AgregarCeldaProducto(tablaProductos, producto.p_desc, fuenteChica, Element.ALIGN_LEFT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_cantidad.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_pvta.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_subtotal.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_ii.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.iva_alicuota.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_boni.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
                AgregarCeldaProducto(tablaProductos, producto.cmd_subtotal_con_iva.ToString("N2"), fuenteChica, Element.ALIGN_RIGHT);
            }

            pdf.Add(tablaProductos);

            GenerarResumenIvaYTotales(
                pdf,
                datosIva,
                datosPercepciones,
                encabezado,
                fuenteNormal,
                fuenteNormalBold,
                fuenteChica
            );

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

        private float CalcularAlturaPie(List<FeIvaResDto> datosIva, List<FePerResDto> datosPercepciones)
        {
            float altura = 0;
            altura += 20f;
            int filasTributos = (datosPercepciones?.Count ?? 0) + 2;
            altura += filasTributos * 15f;
            int filasTotales = (datosIva?.Count ?? 0) + 1;
            altura += filasTotales * 15f;
            altura += 50f;
            altura += 30f;
            return altura;
        }

        private bool NecesitaNuevaPagina(PdfWriter writer, Document pdf, float alturaPie)
        {
            float posicionActual = writer.GetVerticalPosition(true);
            float espacioDisponible = posicionActual - pdf.BottomMargin;
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

            PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
            celdaLogo.Border = Rectangle.BOX;
            celdaLogo.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaLogo);

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

            PdfPTable tablaDatos = new PdfPTable(1);
            tablaDatos.WidthPercentage = 100;

            AgregarCeldaSinBorde(tablaDatos, "TICKET FACTURA", fuenteSubtitulo, Element.ALIGN_CENTER);
            AgregarCeldaSinBorde(tablaDatos, $"Punto de Venta: {encabezado.adm_id} Comp. Nro: {encabezado.cm_compte}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Emisión: {encabezado.cm_fecha:dd/MM/yyyy}", fuenteChica, Element.ALIGN_LEFT);

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

            PdfPTable tablaDatos = new PdfPTable(2);
            tablaDatos.WidthPercentage = 100;
            tablaDatos.SetWidths(new float[] { 50f, 50f });

            PdfPTable tablaEmisor = new PdfPTable(2);
            tablaEmisor.WidthPercentage = 100;
            tablaEmisor.SetWidths(new float[] { 35f, 65f });

            AgregarFilaDatos(tablaEmisor, "Razón Social:", encabezado.emp_razon_social, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaEmisor, "Domicilio Comercial:", encabezado.emp_domicilio, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaEmisor, "Condición frente al IVA:", encabezado.afip_desc_emp, fuenteNormalBold, fuenteNormal);

            PdfPCell celdaEmisor = new PdfPCell(tablaEmisor);
            celdaEmisor.Border = Rectangle.BOX;
            celdaEmisor.Padding = 5f;
            tablaDatos.AddCell(celdaEmisor);

            PdfPTable tablaReceptor = new PdfPTable(2);
            tablaReceptor.WidthPercentage = 100;
            tablaReceptor.SetWidths(new float[] { 35f, 65f });

            AgregarFilaDatos(tablaReceptor, "CUIT:", encabezado.cm_cuit, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaReceptor, "Ingresos Brutos:", encabezado.emp_ib_nro, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaReceptor, "Fecha de Inicio de Actividades:",
                encabezado.emp_inicio_act.ToString("dd/MM/yyyy"), fuenteNormalBold, fuenteNormal);

            PdfPCell celdaReceptor = new PdfPCell(tablaReceptor);
            celdaReceptor.Border = Rectangle.BOX;
            celdaReceptor.Padding = 5f;
            tablaDatos.AddCell(celdaReceptor);

            pdf.Add(tablaDatos);

            pdf.Add(new Paragraph(" ", fuenteChica));

            PdfPTable tablaCliente = new PdfPTable(2);
            tablaCliente.WidthPercentage = 100;
            tablaCliente.SetWidths(new float[] { 20f, 80f });

            AgregarFilaDatos(tablaCliente, "CUIT:", encabezado.cm_cuit, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Apellido y Nombre / Razón Social:", encabezado.cm_nombre, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición frente al IVA:", encabezado.afip_desc_cli, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Domicilio Comercial:", encabezado.cm_domicilio, fuenteNormalBold, fuenteNormal);
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

        private void GenerarResumenIvaYTotales(
            Document pdf,
            List<FeIvaResDto> datosIva,
            List<FePerResDto> datosPercepciones,
            FeResDto encabezado,
            Font fuenteNormal,
            Font fuenteNormalBold,
            Font fuenteChica)
        {
            pdf.Add(new Paragraph(" ", fuenteChica));

            PdfPTable tablaResumen = new PdfPTable(2);
            tablaResumen.WidthPercentage = 100;
            tablaResumen.SetWidths(new float[] { 50f, 50f });

            PdfPTable tablaPercepciones = new PdfPTable(4);
            tablaPercepciones.WidthPercentage = 100;
            tablaPercepciones.SetWidths(new float[] { 50f, 17f, 17f, 16f });

            PdfPCell celdaTitulo = new PdfPCell(new Phrase("Otros Tributos", fuenteNormalBold));
            celdaTitulo.Colspan = 4;
            celdaTitulo.BackgroundColor = BaseColor.LightGray;
            celdaTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaTitulo.Padding = 3f;
            tablaPercepciones.AddCell(celdaTitulo);

            AgregarEncabezadosTributos(tablaPercepciones, fuenteNormalBold);

            foreach (var percepcion in datosPercepciones.OrderBy(p => p.orden))
            {
                AgregarFilaTributo(
                    tablaPercepciones,
                    percepcion.imp_des ?? "",
                    percepcion.Base ?? 0,
                    percepcion.ali ?? 0,
                    percepcion.percepcion ?? 0,
                    fuenteChica
                );
            }

            PdfPCell celdaPerc = new PdfPCell(tablaPercepciones);
            celdaPerc.Border = Rectangle.BOX;
            celdaPerc.Padding = 5f;
            tablaResumen.AddCell(celdaPerc);

            PdfPTable tablaTotales = new PdfPTable(2);
            tablaTotales.WidthPercentage = 100;
            tablaTotales.SetWidths(new float[] { 70f, 30f });

            foreach (var lineaIva in datosIva.OrderBy(i => i.orden))
            {
                AgregarLineaTotal(
                    tablaTotales,
                    lineaIva.concepto ?? "",
                    lineaIva.importe ?? 0,
                    fuenteNormal,
                    Element.ALIGN_RIGHT
                );
            }

            AgregarLineaTotal(
                tablaTotales,
                "Importe Total:",
                encabezado.cm_total,
                fuenteNormalBold,
                Element.ALIGN_RIGHT,
                BaseColor.LightGray
            );

            PdfPCell celdaTot = new PdfPCell(tablaTotales);
            celdaTot.Border = Rectangle.BOX;
            celdaTot.Padding = 5f;
            tablaResumen.AddCell(celdaTot);

            pdf.Add(tablaResumen);
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
                AgregarCeldaSinBorde(tablaInfo, "CAE N°:", fuenteChica, Element.ALIGN_RIGHT);

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

        private void AgregarEncabezadosTributos(PdfPTable tabla, Font fuente)
        {
            string[] encabezados = { "Descripción", "Base", "Ali", "Importe" };

            foreach (var encabezado in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(encabezado, fuente));
                celda.BackgroundColor = new BaseColor(230, 230, 230);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 3f;
                tabla.AddCell(celda);
            }
        }

        private void AgregarFilaTributo(PdfPTable tabla, string descripcion, decimal baseImp, decimal ali, decimal importe, Font fuente)
        {
            AgregarCeldaProducto(tabla, descripcion, fuente, Element.ALIGN_LEFT);
            AgregarCeldaProducto(tabla, baseImp.ToString("N2"), fuente, Element.ALIGN_RIGHT);
            AgregarCeldaProducto(tabla, ali.ToString("N2"), fuente, Element.ALIGN_RIGHT);
            AgregarCeldaProducto(tabla, importe.ToString("N2"), fuente, Element.ALIGN_RIGHT);
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

        private List<FeIvaResDto> ObtenerDatosIva(ReporteSolicitudDto solicitud)
        {
            try
            {
                var reqDto = new FeReqDto
                {
                    tco_id = solicitud.Parametros.GetValueOrDefault("tco_id", "")?.ToString() ?? "",
                    cm_compte = solicitud.Parametros.GetValueOrDefault("cm_compte", "")?.ToString() ?? "",
                    cm_repetido = solicitud.Parametros.GetValueOrDefault("cm_repetido", "0")?.ToString() ?? "0"
                };

                return _factServicio.ObtenerFEIva(reqDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de IVA");
                return new List<FeIvaResDto>();
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