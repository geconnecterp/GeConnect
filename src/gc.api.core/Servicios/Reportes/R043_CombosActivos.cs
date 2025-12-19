using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.api.core.Servicios.Reportes
{
    public class R043_CombosActivos : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiPromoComboServicio _cmbSv;
        private readonly EmpresaGeco _empresaGeco;
        private List<string> _titulos;
        private List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R043_CombosActivos(IUnitOfWork uow, IApiPromoComboServicio servicio,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _cmbSv = servicio;
            _empresaGeco = empresa.Value;
            
            // Títulos y campos actualizados sin p_id_barrado
            _titulos = new List<string> { "Código", "Producto", "Cantidad", "Dto.%", "Dto.Imp" };
            _campos = new List<string> { "p_id", "p_desc", "cantidad", "dto_porc", "dto_imp" };
            
            _cuentaSv = consultaSv;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            PdfWriter? writer = null;
            Document? pdf = null;

            try
            {
                var ms = new MemoryStream();
                
                #region Obtener datos
                List<ComboRepoDto> registros = ObtenerDatos(solicitud, out string titulo);
                #endregion

                #region Inicializar PDF
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 15);

                // Anchos de columnas (5 columnas ahora)
                float[] anchos = [15f, 45f, 15f, 12.5f, 12.5f];

                // Fuentes
                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var titulo_font = HelperPdf.FontTituloPredeterminado();
                #endregion

                #region Generar Cabecera
                PdfPTable tablaCabecera = GeneraCabeceraPdf(solicitud, logo, chico, titulo_font, _empresaGeco);
                Phrase phrase = new Phrase();
                phrase.Add(tablaCabecera);

                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                };
                pdf.Header = header;
                #endregion

                pdf.Open();

                #region Generar contenido del reporte
                if (registros == null || !registros.Any())
                {
                    // Agregar mensaje de "sin datos"
                    AgregarMensajeSinDatos(pdf, normal);
                }
                else
                {
                    GenerarContenidoReporte(pdf, registros, titulo_font, normal, chico, anchos);
                }
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
                _logger.LogError(ex, "Error al generar R043_CombosActivos");
                throw new NegocioException("Se produjo un error al intentar generar el Reporte de Combos Activos. Para mayores datos ver el log.");
            }
            finally
            {
                pdf?.Dispose();
                writer?.Dispose();
            }
        }

        #region Métodos de generación de contenido

        private void GenerarContenidoReporte(
            Document pdf,
            List<ComboRepoDto> registros,
            Font tituloFont,
            Font normalFont,
            Font chicoFont,
            float[] anchos)
        {
            // Agrupar por combo para procesar ordenadamente
            var combosProcesados = registros
                .GroupBy(x => x.cmb_id)
                .OrderBy(g => g.Key);

            foreach (var combo in combosProcesados)
            {
                // 1. Cabecera del combo
                AgregarCabeceraCombo(pdf, combo.First(), tituloFont, normalFont);

                // 2. Procesar productos del combo
                ProcesarProductosDelCombo(pdf, combo.ToList(), chicoFont, normalFont, anchos);

                // 3. Espacio entre combos
                pdf.Add(new Paragraph(" ", chicoFont) { SpacingAfter = 10f });
            }
        }

        private void AgregarCabeceraCombo(
            Document pdf,
            ComboRepoDto combo,
            Font tituloFont,
            Font normalFont)
        {
            var tipoCombo = combo.cmb_tipo.Equals('C') ? "Combo" : "Promo";
            var icono = combo.cmb_tipo.Equals('C') ? "●" : "◆";

            // Tabla principal del combo
            PdfPTable tablaCombo = HelperPdf.GeneraTabla(1, new[] { 100f }, 100, 10, 0);
            
            // Fila 1: ID y Descripción
            var textoCombo = $"{icono} {combo.cmb_id} - {combo.cmb_desc}";
            PdfPCell celdaCombo = new PdfPCell(new Phrase(textoCombo, tituloFont))
            {
                BackgroundColor = new BaseColor(211, 211, 211),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8,
                Border = Rectangle.BOX
            };
            tablaCombo.AddCell(celdaCombo);
            pdf.Add(tablaCombo);

            // Tabla de detalles (3 columnas)
            PdfPTable tablaDetalles = HelperPdf.GeneraTabla(3, new[] { 33.33f, 33.33f, 33.34f }, 100, 0, 0);

            // Usuario
            AgregarCeldaDetalle(tablaDetalles, $"Usuario: {combo.usu_id}", normalFont, Element.ALIGN_LEFT);
            
            // Tipo
            AgregarCeldaDetalle(tablaDetalles, $"Tipo: {tipoCombo}", normalFont, Element.ALIGN_CENTER);
            
            // Vigencia
            var vigencia = $"{combo.cmb_desde:dd/MM/yyyy} al {combo.cmb_hasta:dd/MM/yyyy}";
            AgregarCeldaDetalle(tablaDetalles, $"Vigencia: {vigencia}", normalFont, Element.ALIGN_RIGHT);
            
            pdf.Add(tablaDetalles);

            // Tabla de Administración y Lista
            PdfPTable tablaInfo = HelperPdf.GeneraTabla(2, new[] { 50f, 50f }, 100, 0, 5);

            AgregarCeldaInfo(tablaInfo, $"Administración: ({combo.adm_id}) {combo.adm_nombre}", normalFont);
            AgregarCeldaInfo(tablaInfo, $"Lista: ({combo.lp_id}) {combo.lp_desc}", normalFont);

            pdf.Add(tablaInfo);
        }

        private void ProcesarProductosDelCombo(
            Document pdf,
            List<ComboRepoDto> productos,
            Font chicoFont,
            Font normalFont,
            float[] anchos)
        {
            // Cabecera de columnas
            AgregarCabeceraColumnas(pdf, anchos, normalFont);

            // Agrupar por p_id para identificar productos con sustitutos
            var productosAgrupados = productos
                .GroupBy(x => x.p_id)
                .OrderBy(g => g.Key);

            bool alternar = false;

            foreach (var grupoProducto in productosAgrupados)
            {
                var productoBase = grupoProducto.First();
                var tieneSustitutos = grupoProducto.Any(x => !string.IsNullOrEmpty(x.p_id_sustituto));

                if (tieneSustitutos)
                {
                    // Producto con sustitutos
                    AgregarCabeceraProductoBase(pdf, productoBase, normalFont);

                    // Listar sustitutos
                    foreach (var sustituto in grupoProducto.Where(x => !string.IsNullOrEmpty(x.p_id_sustituto)))
                    {
                        AgregarFilaSustituto(pdf, sustituto, chicoFont, anchos, alternar);
                        alternar = !alternar;
                    }
                }
                else
                {
                    // Producto simple
                    AgregarFilaProductoSimple(pdf, productoBase, chicoFont, anchos, alternar);
                    alternar = !alternar;
                }
            }
        }

        private void AgregarCabeceraColumnas(Document pdf, float[] anchos, Font fuente)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(_titulos.Count, anchos, 100, 5, 0);

            foreach (var titulo in _titulos)
            {
                var celda = new PdfPCell(new Phrase(titulo, fuente))
                {
                    BackgroundColor = BaseColor.White,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BorderColor = BaseColor.White
                };
                tabla.AddCell(celda);
            }

            pdf.Add(tabla);
        }

        private void AgregarCabeceraProductoBase(Document pdf, ComboRepoDto producto, Font fuente)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(1, new[] { 100f }, 100, 3, 0);

            var texto = $"▸ Producto Base: ({producto.p_id}) {producto.p_desc} | " +
                       $"Cant: {producto.cantidad:N0} | " +
                       $"Dto: {producto.dto_porc:N2}% (${producto.dto_imp:N2})";

            var celda = new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = new BaseColor(255, 250, 205),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 6,
                Border = Rectangle.BOX
            };

            tabla.AddCell(celda);
            pdf.Add(tabla);
        }

        private void AgregarFilaSustituto(
            Document pdf,
            ComboRepoDto sustituto,
            Font fuente,
            float[] anchos,
            bool alternar)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(_campos.Count, anchos, 100, 0, 0);
            var colorFondo = alternar ? BaseColor.White : new BaseColor(250, 250, 250);

            // Columna 1 y 2: Indicador de sustituto
            var textoSustituto = $"  ↳ Sustituto de {sustituto.p_id}: ({sustituto.p_id_sustituto}) {sustituto.p_desc_sustituto}";
            var celdaSustituto = new PdfPCell(new Phrase(textoSustituto, fuente))
            {
                Colspan = 2,
                BackgroundColor = colorFondo,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 4,
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = new BaseColor(220, 220, 220)
            };
            tabla.AddCell(celdaSustituto);

            // Cantidad
            AgregarCeldaValor(tabla, sustituto.cantidad.ToString("N0"), fuente, Element.ALIGN_RIGHT, colorFondo);

            // Descuento %
            AgregarCeldaValor(tabla, $"{sustituto.dto_porc:N2}%", fuente, Element.ALIGN_RIGHT, colorFondo);

            // Importe
            AgregarCeldaValor(tabla, $"${sustituto.dto_imp:N2}", fuente, Element.ALIGN_RIGHT, colorFondo);

            pdf.Add(tabla);
        }

        private void AgregarFilaProductoSimple(
            Document pdf,
            ComboRepoDto producto,
            Font fuente,
            float[] anchos,
            bool alternar)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(_campos.Count, anchos, 100, 0, 0);
            var colorFondo = alternar ? BaseColor.White : new BaseColor(250, 250, 250);

            // Código
            AgregarCeldaValor(tabla, producto.p_id, fuente, Element.ALIGN_CENTER, colorFondo);

            // Descripción
            AgregarCeldaValor(tabla, producto.p_desc, fuente, Element.ALIGN_LEFT, colorFondo);

            // Cantidad
            AgregarCeldaValor(tabla, producto.cantidad.ToString("N0"), fuente, Element.ALIGN_RIGHT, colorFondo);

            // Descuento %
            AgregarCeldaValor(tabla, $"{producto.dto_porc:N2}%", fuente, Element.ALIGN_RIGHT, colorFondo);

            // Importe
            AgregarCeldaValor(tabla, $"${producto.dto_imp:N2}", fuente, Element.ALIGN_RIGHT, colorFondo);

            pdf.Add(tabla);
        }

        #endregion

        #region Métodos auxiliares de celdas

        private void AgregarCeldaDetalle(PdfPTable tabla, string texto, Font fuente, int alineacion)
        {
            var celda = new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = new BaseColor(230, 230, 250),
                HorizontalAlignment = alineacion,
                Padding = 5,
                Border = Rectangle.BOX
            };
            tabla.AddCell(celda);
        }

        private void AgregarCeldaInfo(PdfPTable tabla, string texto, Font fuente)
        {
            var celda = new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = new BaseColor(245, 245, 245),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                Border = Rectangle.BOX
            };
            tabla.AddCell(celda);
        }

        private void AgregarCeldaValor(
            PdfPTable tabla,
            string texto,
            Font fuente,
            int alineacion,
            BaseColor colorFondo)
        {
            var celda = new PdfPCell(new Phrase(texto, fuente))
            {
                BackgroundColor = colorFondo,
                HorizontalAlignment = alineacion,
                Padding = 4,
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = new BaseColor(220, 220, 220)
            };
            tabla.AddCell(celda);
        }

        #endregion

        #region Métodos auxiliares generales

        private void AgregarMensajeSinDatos(Document pdf, Font fuente)
        {
            PdfPTable tabla = HelperPdf.GeneraTabla(1, new[] { 100f }, 100, 20, 20);
            
            var celda = new PdfPCell(new Phrase("No se encontraron combos activos con los criterios especificados.", fuente))
            {
                BackgroundColor = new BaseColor(255, 255, 200),
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                MinimumHeight = 100f,
                Padding = 20,
                Border = Rectangle.BOX,
                BorderColor = BaseColor.Black
            };
            
            tabla.AddCell(celda);
            pdf.Add(tabla);
        }

        private PdfPTable GeneraCabeceraPdf(
            ReporteSolicitudDto solicitud,
            Image logo,
            Font chico,
            Font titulo,
            EmpresaGeco empresa)
        {
            PdfPTable contenedor = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 30f, 60f], 100, 10, 20);

            // Logo
            PdfPCell celdaLogo = logo == null 
                ? new PdfPCell(new Paragraph("CA", titulo)) 
                : HelperPdf.GeneraCelda(logo, false);
            tabla.AddCell(celdaLogo);

            // Datos de la empresa
            PdfPTable subTabla = new(1) { WidthPercentage = 100 };
            subTabla.AddCell(HelperPdf.CrearCeldaTexto(empresa.Nombre, chico));
            subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{empresa.Responsabilidad} Ini.Act:{empresa.InicioActividades.ToShortDateString()}", chico));
            subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {empresa.CUIT} IB:{empresa.IngresosBrutos}", chico));
            subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{empresa.Direccion}, {empresa.Localidad}", chico));

            PdfPCell celdaSubTabla = new(subTabla)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            tabla.AddCell(celdaSubTabla);

            // Título del reporte
            PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 10f
            };
            tabla.AddCell(celdaTitulo);

            PdfPCell celdaContenedora = new PdfPCell(tabla)
            {
                Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                BorderWidthTop = 0.8f,
                BorderWidthBottom = 0.8f,
                BorderColorTop = BaseColor.Black,
                BorderColorBottom = BaseColor.Black,
                PaddingTop = 1f,
                PaddingBottom = 1f
            };
            
            contenedor.AddCell(celdaContenedora);
            return contenedor;
        }

        private List<ComboRepoDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
        {
            var adm = solicitud.Parametros.GetValueOrDefault("adm_id", "")?.ToString() ?? "0000";
            var lp = solicitud.Parametros.GetValueOrDefault("lp_id", "")?.ToString() ?? "001";
            var estado = solicitud.Parametros.GetValueOrDefault("cmb_estado", "")[0];
            var cmb = solicitud.Parametros.GetValueOrDefault("cmb_id", "")?.ToString() ?? "A";
            var carga = solicitud.Parametros.GetValueOrDefault("cmb_carga", "")?.ToDateTime() ?? new(2020, 01, 01);

            titulo = solicitud.Titulo;

            var combos = _cmbSv.ObtenerCombosRepo(new ComboReqDto
            {
                adm_id = adm,
                cmb_carga = carga,
                cmb_estado = estado,
                cmb_id = cmb,
                lp_id = lp
            });

            return combos;
        }

        #endregion

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
