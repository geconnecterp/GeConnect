using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Ofertas;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R045_PunteraDeGondola : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IApiEtiquetaServicio _etSv;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R045_PunteraDeGondola(IUnitOfWork uow, IApiEtiquetaServicio servicio,
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
            float[] anchos;

            PdfWriter? writer = null;
            Document pdf;

            try
            {
                var ms = new MemoryStream();
                #region Obteniendo registros desde la base de datos
                string adm_desc;
                string adm_id;
                List<EtiquetaDto> etiquetas = ObtenerDatos(solicitud,out adm_desc,out adm_id);

               

                #endregion
                #region instanciamos el pdf
                // Una puntera ocupa exactamente media hoja A4 (formato A5 apaisado).
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A5, false,
                                                        14f, 14f, 10f, 10f);

                // Definir fuentes para diferentes secciones
                var fuenteLeyenda = HelperPdf.DefineFontWithStyle("Arial", 45, Font.BOLD, 0, 0, 0);
                var fuenteDescripcion = HelperPdf.DefineFontWithStyle("Arial", 25, Font.BOLD, 0, 0, 0);
                var fuentePrecioSimbolo = HelperPdf.DefineFontWithStyle("Arial Black", 82, Font.BOLD, 0, 0, 0);
                var fuentePrecioDecimal = HelperPdf.DefineFontWithStyle("Arial Black", 47, Font.BOLD, 0, 0, 0);
                var fuenteOferta = HelperPdf.DefineFontWithStyle("Arial", 14, Font.BOLD, 0, 0, 0);
                var fuenteInferior = HelperPdf.DefineFontWithStyle("Arial", 8, Font.NORMAL, 0, 0, 0);

                pdf.Open();

                #region Generar etiquetas - una por página
                for (int i = 0; i < etiquetas.Count; i++)
                {
                    var etiqueta = etiquetas[i];

                    // Si no es la primera etiqueta, crear nueva página
                    if (i > 0)
                    {
                        pdf.NewPage();
                    }

                    // ===== SECCIÓN SUPERIOR: LEYENDA DE VENTA / OFERTA =====
                    GenerarSeccionLeyenda(pdf, ObtenerLeyendaVenta(etiqueta), fuenteLeyenda);

                    // ===== DESCRIPCIÓN DEL PRODUCTO =====
                    GenerarSeccionDescripcion(pdf, etiqueta.p_desc, fuenteDescripcion);

                    // ===== SECCIÓN CENTRAL: PRECIO =====
                    GenerarSeccionPrecio(pdf, etiqueta.p_pvta, fuentePrecioSimbolo, fuentePrecioDecimal);

                    // ===== INFORMACIÓN DE LA OFERTA =====
                    if (EsOfertaConPrecioAnterior(etiqueta))
                    {
                        GenerarSeccionOferta(pdf, etiqueta.p_pvta_real, etiqueta.vigencia, fuenteOferta);
                    }

                    // ===== SECCIÓN INFERIOR: CÓDIGOS Y ADMINISTRACIÓN =====
                    GenerarSeccionInferior(pdf, etiqueta.p_id, etiqueta.p_id_barrado,
                                           adm_id, adm_desc, fuenteInferior);
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
        /// Genera la leyenda comercial destacada (Oferta o tipo de oferta).
        /// </summary>
        private static void GenerarSeccionLeyenda(Document pdf, string leyenda, Font fuente)
        {
            var tabla = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);
            var parrafo = HelperPdf.GeneraParrafo(leyenda?.Trim() ?? string.Empty, fuente,
                                                  Element.ALIGN_CENTER, 0, 0);
            var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.White,
                                              Element.ALIGN_CENTER);
            celda.Border = Rectangle.NO_BORDER;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.MinimumHeight = 58f;
            tabla.AddCell(celda);
            pdf.Add(tabla);
        }

        /// <summary>
        /// Genera la sección superior con la descripción del producto
        /// </summary>
        private void GenerarSeccionDescripcion(Document pdf, string descripcion, Font fuente)
        {
            // Crear tabla de 1 columna para centrar el texto
            PdfPTable tabla = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);

            var parrafo = HelperPdf.GeneraParrafo(descripcion, fuente,
                                                  Element.ALIGN_CENTER, 0, 0);

            var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.White,
                                              Element.ALIGN_CENTER);
            celda.Border = Rectangle.NO_BORDER;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.MinimumHeight = 82f;

            tabla.AddCell(celda);
            pdf.Add(tabla);
        }

        /// <summary>
        /// Genera la sección central con el precio destacado
        /// </summary>
        private void GenerarSeccionPrecio(Document pdf, decimal precio,
                                           Font fuenteSimbolo, Font fuenteDecimal)
        {
            // Separar parte entera y decimal
            var precioRedondeado = Math.Round(precio, 2, MidpointRounding.AwayFromZero);
            int parteEntera = (int)Math.Truncate(precioRedondeado);
            int parteDecimal = (int)Math.Abs((precioRedondeado - parteEntera) * 100);

            // Crear tabla de 1 fila para el precio
            PdfPTable tablaPrecio = HelperPdf.GeneraTabla(1, [ 100f ], 100, 0, 0);

            // Crear frase combinando símbolo $, precio y decimales
            Phrase frasePrecio =
            [
                new Chunk("$ ", fuenteSimbolo),
                new Chunk(parteEntera.ToString(), fuenteSimbolo),
                new Chunk($".{parteDecimal:00}", fuenteDecimal),
            ];

            var celdaPrecio = new PdfPCell(frasePrecio)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                MinimumHeight = 112f
            };

            tablaPrecio.AddCell(celdaPrecio);
            pdf.Add(tablaPrecio);
        }

        private static bool EsOferta(EtiquetaDto etiqueta)
        {
            return char.ToUpperInvariant(etiqueta.es_oferta) == 'S'
                   || string.Equals(etiqueta.oferta?.Trim(), "S",
                                    StringComparison.OrdinalIgnoreCase);
        }

        private static string ObtenerLeyendaVenta(EtiquetaDto etiqueta)
        {
            // El SP histórico expone p_pvta_leyenda. Se admite también el nuevo
            // nombre p_vta_leyenda para mantener compatibilidad entre versiones.
            var leyenda = !string.IsNullOrWhiteSpace(etiqueta.p_vta_leyenda)
                ? etiqueta.p_vta_leyenda
                : etiqueta.p_pvta_leyenda;

            if (!EsOferta(etiqueta))
            {
                return leyenda?.Trim().ToUpperInvariant() ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(leyenda))
            {
                return "OFERTA";
            }

            var leyendaNormalizada = leyenda.Trim().ToUpperInvariant();
            return leyendaNormalizada.StartsWith("OFERTA", StringComparison.Ordinal)
                ? leyendaNormalizada
                : $"OFERTA {leyendaNormalizada}";
        }

        private static bool EsOfertaConPrecioAnterior(EtiquetaDto etiqueta)
        {
            return EsOferta(etiqueta) && etiqueta.p_pvta_real > etiqueta.p_pvta;
        }

        /// <summary>
        /// Informa el precio habitual y la vigencia cuando el precio impreso es una oferta.
        /// </summary>
        private static void GenerarSeccionOferta(Document pdf, decimal precioReal,
                                                 DateTime? vigencia, Font fuente)
        {
            var textoVigencia = vigencia.HasValue
                ? $"Vigente hasta: {vigencia.Value:dd/MM/yy}"
                : "Hasta agotar disponibilidad";
            var texto = $"Precio sin Oferta: $ {precioReal:N2} - {textoVigencia}";

            var tabla = HelperPdf.GeneraTabla(1, [100f], 100, 0, 0);
            var parrafo = HelperPdf.GeneraParrafo(texto, fuente, Element.ALIGN_CENTER, 0, 0);
            var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.White,
                                              Element.ALIGN_CENTER);
            celda.Border = Rectangle.NO_BORDER;
            celda.VerticalAlignment = Element.ALIGN_MIDDLE;
            celda.MinimumHeight = 36f;
            tabla.AddCell(celda);
            pdf.Add(tabla);
        }

        /// <summary>
        /// Genera la sección inferior con códigos y administración
        /// </summary>
        private void GenerarSeccionInferior(Document pdf, string codigo, string barrado,
                                            string admId, string admDesc, Font fuente)
        {
            // Crear tabla con 2 columnas: izquierda para códigos, derecha para admin
            PdfPTable tablaInferior = HelperPdf.GeneraTabla(2, [ 50f, 50f ],
                                                             100, 4, 0);

            // ===== COLUMNA IZQUIERDA: CÓDIGOS =====
            var parrafoIzq = HelperPdf.GeneraParrafo(
                $"{codigo}                      {barrado}",
                fuente,
                Element.ALIGN_LEFT,
                0, 0
            );

            var celdaIzq = HelperPdf.GeneraCelda(parrafoIzq, false, BaseColor.White,
                                                 Element.ALIGN_LEFT);
            celdaIzq.Border = Rectangle.NO_BORDER;
            celdaIzq.VerticalAlignment = Element.ALIGN_BOTTOM;
            celdaIzq.PaddingLeft = 5f;

            // ===== COLUMNA DERECHA: ADMINISTRACIÓN =====
            var parrafoDer = HelperPdf.GeneraParrafo(
                $"({admId}) {admDesc}",
                fuente,
                Element.ALIGN_RIGHT,
                0, 0
            );

            var celdaDer = HelperPdf.GeneraCelda(parrafoDer, false, BaseColor.White,
                                                 Element.ALIGN_RIGHT);
            celdaDer.Border = Rectangle.NO_BORDER;
            celdaDer.VerticalAlignment = Element.ALIGN_BOTTOM;
            celdaDer.PaddingRight = 5f;

            tablaInferior.AddCell(celdaIzq);
            tablaInferior.AddCell(celdaDer);

            pdf.Add(tablaInferior);
        }

        private List<EtiquetaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string adm_desc,out string adm_id)
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
            etiquetas = _etSv.ObtenerDatosParaEtiqueta(prods,et,adm, usu);
                        
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
