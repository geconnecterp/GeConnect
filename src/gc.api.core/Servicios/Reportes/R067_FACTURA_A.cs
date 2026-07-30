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
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
    public class R067_FacturaA : Servicio<EntidadBase>, IGeneradorReporte
    {
        private const int FilasPaginaComun = 30;
        private const int FilasUltimaPagina = 20;
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

                var logo = HelperPdf.CargaLogo(
                    solicitud.LogoPath,
                    20,
                    pdf.PageSize.Height - 10,
                    15
                );
                #endregion

                #region Fuentes
                var fuenteTitulo = CrearFuente(18, true);
                var fuenteSubtitulo = CrearFuente(8, true);
                var fuenteNormal = CrearFuente(7, false);
                var fuenteNormalBold = CrearFuente(7, true);
                var fuenteChica = CrearFuente(6, false);
                var leyendaImpresion = FacturaBase.ObtenerLeyendaImpresion(solicitud);
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
                    fuenteChica,
                    leyendaImpresion
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
                    logo,
                    fuenteTitulo,
                    fuenteSubtitulo,
                    fuenteChica,
                    leyendaImpresion,
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
                _logger.LogError(ex, "Error en R067_FacturaA");
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
            Image logo,
            Font fuenteTitulo,
            Font fuenteSubtitulo,
            Font fuenteChica,
            string leyendaImpresion,
            Font fuenteNormal,
            Font fuenteNormalBold)
        {
            var totalPaginas = FacturaBase.CalcularTotalPaginas(productos.Count, FilasPaginaComun, FilasUltimaPagina);
            var indiceProducto = 0;

            for (var pagina = 1; pagina <= totalPaginas; pagina++)
            {
                if (pagina > 1)
                {
                    pdf.NewPage();
                    GenerarCabeceraFactura(pdf, encabezado, logo, fuenteTitulo, fuenteSubtitulo, fuenteNormal, fuenteChica, leyendaImpresion);
                    GenerarDatosEmisorReceptor(pdf, encabezado, fuenteNormal, fuenteNormalBold, fuenteChica);
                }

                pdf.Add(new Paragraph(" ", fuenteChica) { SpacingAfter = 0f });

                var filasPagina = FacturaBase.ObtenerCantidadFilasPagina(pagina, totalPaginas, FilasPaginaComun, FilasUltimaPagina);
                var productosPagina = productos.Skip(indiceProducto).Take(filasPagina).ToList();
                indiceProducto += productosPagina.Count;

                pdf.Add(CrearTablaProductosFacturaA(productosPagina, fuenteChica, fuenteNormalBold));

                if (pagina == totalPaginas)
                {
                    FacturaBase.DibujarPieFacturaA(
                        writer,
                        pdf,
                        encabezado,
                        datosIva,
                        datosPercepciones,
                        pagina,
                        totalPaginas,
                        fuenteNormal,
                        fuenteNormalBold,
                        fuenteChica,
                        _logger
                    );
                }
            }
        }

        private static PdfPTable CrearTablaProductosFacturaA(List<FeDetResDto> productos, Font fuenteChica, Font fuenteNormalBold)
        {
            float[] anchos = new float[] { 8f, 32f, 8f, 10f, 10f, 5f, 8f, 10f, 9f };
            string[] encabezados = new string[]
            {
                "Código", "Producto/Servicio", "Cantidad",
                "Precio\nUnitario", "Subtotal", "II",
                "Alí.\nIVA", "Boni. con\nIVA", "Sub Total\ncon IVA"
            };

            var tablaProductos = FacturaBase.CrearTablaProductos(anchos, encabezados, fuenteNormalBold);

            foreach (var producto in productos)
            {
                FacturaBase.AgregarCeldaProducto(tablaProductos, producto.p_id, fuenteChica, Element.ALIGN_CENTER);
                FacturaBase.AgregarCeldaProducto(tablaProductos, producto.p_desc, fuenteChica, Element.ALIGN_LEFT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_cantidad), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_pvta), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_subtotal), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_ii), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.iva_alicuota), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_boni), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tablaProductos, FacturaBase.FormatearImporte(producto.cmd_subtotal_con_iva), fuenteChica, Element.ALIGN_RIGHT);
            }

            return tablaProductos;
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
            Font fuenteChica,
            string leyendaImpresion)
        {
            FacturaBase.GenerarLeyendaComprobante(pdf, leyendaImpresion);

            PdfPTable tablaCabecera = new PdfPTable(3);
            tablaCabecera.WidthPercentage = 100;
            tablaCabecera.SetWidths(new float[] { 44f, 10f, 46f });

            PdfPTable tablaEmpresa = new PdfPTable(1);
            tablaEmpresa.WidthPercentage = 100;

            PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
            celdaLogo.Border = Rectangle.NO_BORDER;
            celdaLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaLogo.FixedHeight = 30f;
            tablaEmpresa.AddCell(celdaLogo);

            AgregarCeldaSinBorde(
                tablaEmpresa,
                $"Razón Social: {encabezado.emp_razon_social}",
                fuenteChica,
                Element.ALIGN_LEFT
            );
            AgregarCeldaSinBorde(
                tablaEmpresa,
                $"Domicilio Comercial: {ObtenerDomicilioEmpresa(encabezado)}",
                fuenteChica,
                Element.ALIGN_LEFT
            );
            AgregarCeldaSinBorde(
                tablaEmpresa,
                $"Condición frente al IVA: {encabezado.afip_desc_emp}",
                fuenteChica,
                Element.ALIGN_LEFT
            );

            PdfPCell celdaEmpresa = new PdfPCell(tablaEmpresa);
            celdaEmpresa.Border = Rectangle.BOX;
            celdaEmpresa.Padding = 3f;
            celdaEmpresa.VerticalAlignment = Element.ALIGN_MIDDLE;
            tablaCabecera.AddCell(celdaEmpresa);

            PdfPTable tablaLetra = new PdfPTable(1);
            tablaLetra.WidthPercentage = 100;

            PdfPCell celdaLetra = new PdfPCell(new Phrase(encabezado.tco_letra ?? "", fuenteTitulo));
            celdaLetra.Border = Rectangle.NO_BORDER;
            celdaLetra.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaLetra.VerticalAlignment = Element.ALIGN_MIDDLE;
            celdaLetra.PaddingTop = 8f;
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

            AgregarCeldaSinBorde(tablaDatos, ObtenerTituloComprobante(encabezado), fuenteSubtitulo, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Punto de Venta: {ObtenerPuntoVenta(encabezado)}          Comp. Nro: {ObtenerNumeroComprobante(encabezado)}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Emisión: {encabezado.cm_fecha:dd/MM/yyyy}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"CUIT: {encabezado.emp_cuit}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Ingresos Brutos: {encabezado.emp_ib_nro}", fuenteChica, Element.ALIGN_LEFT);
            AgregarCeldaSinBorde(tablaDatos, $"Fecha de Inicio de Actividades: {encabezado.emp_inicio_act:dd/MM/yyyy}", fuenteChica, Element.ALIGN_LEFT);

            PdfPCell celdaContenedoraDatos = new PdfPCell(tablaDatos);
            celdaContenedoraDatos.Border = Rectangle.BOX;
            celdaContenedoraDatos.Padding = 3f;
            celdaContenedoraDatos.VerticalAlignment = Element.ALIGN_MIDDLE;
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
            PdfPTable tablaCliente = new PdfPTable(2);
            tablaCliente.WidthPercentage = 100;
            tablaCliente.SetWidths(new float[] { 24f, 76f });

            AgregarFilaDatos(tablaCliente, "CUIT:", encabezado.cm_cuit, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Apellido y Nombre / Razón Social:", encabezado.cm_nombre, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición frente al IVA:", encabezado.afip_desc_cli, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Domicilio Comercial:", encabezado.cm_domicilio, fuenteNormalBold, fuenteNormal);
            AgregarFilaDatos(tablaCliente, "Condición de Venta:", "", fuenteNormalBold, fuenteNormal);

            PdfPCell celdaCliente = new PdfPCell(tablaCliente);
            celdaCliente.Border = Rectangle.BOX;
            celdaCliente.Padding = 3f;

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
            PdfPTable tablaResumen = new PdfPTable(2);
            tablaResumen.WidthPercentage = 100;
            tablaResumen.SetWidths(new float[] { 54f, 46f });

            PdfPTable tablaPercepciones = new PdfPTable(4);
            tablaPercepciones.WidthPercentage = 100;
            tablaPercepciones.SetWidths(new float[] { 50f, 17f, 17f, 16f });

            PdfPCell celdaTitulo = new PdfPCell(new Phrase("Otros Tributos", fuenteNormalBold));
            celdaTitulo.Colspan = 4;
            celdaTitulo.BackgroundColor = new BaseColor(235, 235, 235);
            celdaTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaTitulo.Padding = 2f;
            tablaPercepciones.AddCell(celdaTitulo);

            AgregarEncabezadosTributos(tablaPercepciones, fuenteNormalBold);

            if (datosPercepciones != null && datosPercepciones.Any())
            {
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
            }
            else
            {
                AgregarFilaTributo(tablaPercepciones, "", 0, 0, 0, fuenteChica);
            }

            AgregarLineaTributoTotal(
                tablaPercepciones,
                "Importe Otros Tributos:",
                datosPercepciones?.Sum(p => p.percepcion ?? 0) ?? 0,
                fuenteNormalBold
            );

            PdfPCell celdaPerc = new PdfPCell(tablaPercepciones);
            celdaPerc.Border = Rectangle.NO_BORDER;
            celdaPerc.Padding = 0f;
            celdaPerc.PaddingRight = 8f;
            tablaResumen.AddCell(celdaPerc);

            PdfPTable tablaTotales = new PdfPTable(2);
            tablaTotales.WidthPercentage = 100;
            tablaTotales.SetWidths(new float[] { 68f, 32f });

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
                new BaseColor(235, 235, 235)
            );

            PdfPCell celdaTot = new PdfPCell(tablaTotales);
            celdaTot.Border = Rectangle.NO_BORDER;
            celdaTot.Padding = 0f;
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

                AgregarCeldaSinBorde(tablaInfo, $"Pag {pdf.PageNumber}/{pdf.PageNumber}", fuenteChica, Element.ALIGN_RIGHT);
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

        private static Font CrearFuente(int size, bool bold = false)
        {
            return HelperPdf.DefineFontWithStyle(
                "Arial",
                size,
                bold ? Font.BOLD : Font.NORMAL,
                0,
                0,
                0
            );
        }

        private static string FormatearImporte(decimal importe)
        {
            return importe.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));
        }

        private static string ObtenerTituloComprobante(FeResDto encabezado)
        {
            var descripcion = (encabezado.tco_desc ?? string.Empty).Trim();

            if (descripcion.Contains("Factura", StringComparison.OrdinalIgnoreCase))
            {
                return "TICKET FACTURA";
            }

            return string.IsNullOrWhiteSpace(descripcion)
                ? "COMPROBANTE"
                : descripcion.ToUpperInvariant();
        }

        private static string ObtenerPuntoVenta(FeResDto encabezado)
        {
            var compte = (encabezado.cm_compte ?? string.Empty).Trim();
            var partes = compte.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length > 0)
            {
                return partes[0];
            }

            return (encabezado.caja_id ?? string.Empty).Trim();
        }

        private static string ObtenerNumeroComprobante(FeResDto encabezado)
        {
            var compte = (encabezado.cm_compte ?? string.Empty).Trim();
            var partes = compte.Split('-', StringSplitOptions.RemoveEmptyEntries);

            return partes.Length > 1 ? partes[1] : compte;
        }

        private static string ObtenerDomicilioEmpresa(FeResDto encabezado)
        {
            var domicilio = (encabezado.emp_domicilio ?? string.Empty).Trim();
            var administracion = (encabezado.adm_direccion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(domicilio) || domicilio == "...")
            {
                return administracion;
            }

            if (string.IsNullOrWhiteSpace(administracion))
            {
                return domicilio;
            }

            if (domicilio.Contains(administracion, StringComparison.OrdinalIgnoreCase))
            {
                return domicilio;
            }

            return $"{domicilio} - {administracion}";
        }

        private static void UbicarPieDeFactura(Document pdf, PdfWriter writer, float alturaPie)
        {
            var posicionActual = writer.GetVerticalPosition(true);
            var espacioDisponible = posicionActual - pdf.BottomMargin;

            if (espacioDisponible < alturaPie)
            {
                pdf.NewPage();
                posicionActual = writer.GetVerticalPosition(true);
                espacioDisponible = posicionActual - pdf.BottomMargin;
            }

            var espacioLibre = espacioDisponible - alturaPie;

            if (espacioLibre > 12f)
            {
                pdf.Add(new Paragraph(" ")
                {
                    Leading = espacioLibre,
                    SpacingBefore = 0f,
                    SpacingAfter = 0f
                });
            }
        }

        private void GenerarResumenOperacion(
            Document pdf,
            FeResDto encabezado,
            Image logo,
            Font fuenteTitulo,
            Font fuenteSubtitulo,
            Font fuenteChica,
            Font fuenteNormalBold)
        {
            PdfPTable tabla = new PdfPTable(6);
            tabla.WidthPercentage = 100;
            tabla.SetWidths(new float[] { 16f, 12f, 16f, 12f, 16f, 12f });

            AgregarCeldaResumenOperacion(tabla, "Subtotal:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumenOperacion(tabla, FormatearImporte(encabezado.cm_gravado), fuenteNormalBold, Element.ALIGN_RIGHT);
            AgregarCeldaResumenOperacion(tabla, "Percep.:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumenOperacion(tabla, FormatearImporte(encabezado.cm_percepciones), fuenteNormalBold, Element.ALIGN_RIGHT);
            AgregarCeldaResumenOperacion(tabla, "Rec/Dto.:", fuenteChica, Element.ALIGN_RIGHT);
            AgregarCeldaResumenOperacion(tabla, FormatearImporte(encabezado.cm_dto), fuenteNormalBold, Element.ALIGN_RIGHT);

            pdf.Add(tabla);
        }

        private static void AgregarCeldaResumenOperacion(
            PdfPTable tabla,
            string texto,
            Font fuente,
            int alineacion)
        {
            var celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.TOP_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 1f;
            tabla.AddCell(celda);
        }

        private void AgregarCeldaSinBorde(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.NO_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 1.2f;
            tabla.AddCell(celda);
        }

        private void AgregarFilaDatos(PdfPTable tabla, string etiqueta, string valor, Font fuenteEtiqueta, Font fuenteValor)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuenteEtiqueta));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaEtiqueta.Padding = 1.2f;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(valor, fuenteValor));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_LEFT;
            celdaValor.Padding = 1.2f;
            tabla.AddCell(celdaValor);
        }

        private void AgregarCeldaProducto(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = Rectangle.BOTTOM_BORDER;
            celda.BorderColor = BaseColor.LightGray;
            celda.HorizontalAlignment = alineacion;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.Padding = 1.5f;
            tabla.AddCell(celda);
        }

        private void AgregarEncabezadosTributos(PdfPTable tabla, Font fuente)
        {
            string[] encabezados = { "Descripción", "Base", "Ali", "Importe" };

            foreach (var encabezado in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(encabezado, fuente));
                celda.BackgroundColor = new BaseColor(245, 245, 245);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 1.5f;
                tabla.AddCell(celda);
            }
        }

        private void AgregarFilaTributo(PdfPTable tabla, string descripcion, decimal baseImp, decimal ali, decimal importe, Font fuente)
        {
            AgregarCeldaProducto(tabla, descripcion, fuente, Element.ALIGN_LEFT);
            AgregarCeldaProducto(tabla, FormatearImporte(baseImp), fuente, Element.ALIGN_RIGHT);
            AgregarCeldaProducto(tabla, FormatearImporte(ali), fuente, Element.ALIGN_RIGHT);
            AgregarCeldaProducto(tabla, FormatearImporte(importe), fuente, Element.ALIGN_RIGHT);
        }

        private void AgregarLineaTributoTotal(PdfPTable tabla, string etiqueta, decimal importe, Font fuente)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuente));
            celdaEtiqueta.Colspan = 3;
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaEtiqueta.Padding = 1.5f;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaImporte = new PdfPCell(new Phrase(FormatearImporte(importe), fuente));
            celdaImporte.Border = Rectangle.NO_BORDER;
            celdaImporte.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaImporte.Padding = 1.5f;
            tabla.AddCell(celdaImporte);
        }

        private void AgregarLineaTotal(PdfPTable tabla, string etiqueta, decimal valor, Font fuente, int alineacion, BaseColor? backgroundColor = null)
        {
            PdfPCell celdaEtiqueta = new PdfPCell(new Phrase(etiqueta, fuente));
            celdaEtiqueta.Border = Rectangle.NO_BORDER;
            celdaEtiqueta.HorizontalAlignment = alineacion;
            celdaEtiqueta.Padding = 1.8f;
            if (backgroundColor != null)
                celdaEtiqueta.BackgroundColor = backgroundColor;
            tabla.AddCell(celdaEtiqueta);

            PdfPCell celdaValor = new PdfPCell(new Phrase(FormatearImporte(valor), fuente));
            celdaValor.Border = Rectangle.NO_BORDER;
            celdaValor.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaValor.Padding = 1.8f;
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





