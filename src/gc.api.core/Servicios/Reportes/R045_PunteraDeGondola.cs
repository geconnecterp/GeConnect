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
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, false,20f,100f,15f,15f);

                // Definir fuentes para diferentes secciones
                var fuenteDescripcion = HelperPdf.DefineFontWithStyle("Arial", 48, Font.BOLD, 0, 0, 0);
                var fuentePrecioSimbolo = HelperPdf.DefineFontWithStyle("Arial Black", 140, Font.BOLD, 0, 0, 0);
                var fuentePrecioDecimal = HelperPdf.DefineFontWithStyle("Arial Black", 80, Font.BOLD, 0, 0, 0);
                var fuenteInferior = HelperPdf.DefineFontWithStyle("Arial", 10, Font.NORMAL, 0, 0, 0);

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

                    // ===== SECCIÓN SUPERIOR: DESCRIPCIÓN DEL PRODUCTO =====
                    GenerarSeccionDescripcion(pdf, etiqueta.p_desc, fuenteDescripcion);

                    // ===== SECCIÓN CENTRAL: PRECIO =====
                    GenerarSeccionPrecio(pdf, etiqueta.p_pvta, fuentePrecioSimbolo, fuentePrecioDecimal);

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
        /// Genera la sección superior con la descripción del producto
        /// </summary>
        private void GenerarSeccionDescripcion(Document pdf, string descripcion, Font fuente)
        {
            // Crear tabla de 1 columna para centrar el texto
            PdfPTable tabla = HelperPdf.GeneraTabla(1, [100f], 100,250, 0);

            var parrafo = HelperPdf.GeneraParrafo(descripcion, fuente,
                                                  Element.ALIGN_RIGHT, 0, 0);

            var celda = HelperPdf.GeneraCelda(parrafo, false, BaseColor.White,
                                              Element.ALIGN_RIGHT);
            celda.Border = Rectangle.NO_BORDER;
            celda.VerticalAlignment = Element.ALIGN_BOTTOM;
            celda.MinimumHeight = 325f;

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
            int parteEntera = (int)Math.Floor(precio);
            int parteDecimal = (int)((precio - parteEntera) * 100);

            // Crear tabla de 1 fila para el precio
            PdfPTable tablaPrecio = HelperPdf.GeneraTabla(1, [ 100f ], 100, 0, 25);

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
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_BOTTOM,
                MinimumHeight = 150f
            };

            tablaPrecio.AddCell(celdaPrecio);
            pdf.Add(tablaPrecio);
        }

        /// <summary>
        /// Genera la sección inferior con códigos y administración
        /// </summary>
        private void GenerarSeccionInferior(Document pdf, string codigo, string barrado,
                                            string admId, string admDesc, Font fuente)
        {
            // Crear tabla con 2 columnas: izquierda para códigos, derecha para admin
            PdfPTable tablaInferior = HelperPdf.GeneraTabla(2, [ 50f, 50f ],
                                                             100, 40, 0);

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
            celdaIzq.PaddingLeft = 20f;

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
            celdaDer.PaddingRight = 20f;

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
