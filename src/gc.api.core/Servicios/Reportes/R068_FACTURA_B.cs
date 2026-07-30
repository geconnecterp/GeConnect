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
        private const int FilasPaginaComun = 34;
        private const int FilasUltimaPagina = 24;

        private readonly IApiProductoFactServicio _factServicio;
        private readonly ILogger _logger;

        public R068_FACTURA_B(
            IUnitOfWork uow,
            IApiProductoFactServicio factServicio,
            IOptions<EmpresaGeco> empresa,
            ILogger logger) : base(uow)
        {
            _factServicio = factServicio;
            _logger = logger;
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            PdfWriter? writer = null;
            Document pdf;

            try
            {
                var ms = new MemoryStream();
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

                var fuenteTitulo = FacturaBase.CrearFuente(18, true);
                var fuenteSubtitulo = FacturaBase.CrearFuente(8, true);
                var fuenteNormal = FacturaBase.CrearFuente(7, false);
                var fuenteNormalBold = FacturaBase.CrearFuente(7, true);
                var fuenteChica = FacturaBase.CrearFuente(6, false);
                var leyendaImpresion = FacturaBase.ObtenerLeyendaImpresion(solicitud);

                pdf.Open();

                GenerarPaginasFacturaB(
                    pdf,
                    writer!,
                    encabezado,
                    productos,
                    datosPercepciones,
                    logo,
                    fuenteTitulo,
                    fuenteSubtitulo,
                    fuenteNormal,
                    fuenteNormalBold,
                    fuenteChica,
                    leyendaImpresion,
                    _logger
                );

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
                throw new NegocioException("Se produjo un error al generar la Factura B. Ver log para más detalles.");
            }
        }

        private static void GenerarPaginasFacturaB(
            Document pdf,
            PdfWriter writer,
            FeResDto encabezado,
            List<FeDetResDto> productos,
            List<FePerResDto> datosPercepciones,
            Image logo,
            Font fuenteTitulo,
            Font fuenteSubtitulo,
            Font fuenteNormal,
            Font fuenteNormalBold,
            Font fuenteChica,
            string leyendaImpresion,
            ILogger logger)
        {
            var totalPaginas = FacturaBase.CalcularTotalPaginas(productos.Count, FilasPaginaComun, FilasUltimaPagina);
            var indiceProducto = 0;

            for (var pagina = 1; pagina <= totalPaginas; pagina++)
            {
                if (pagina > 1)
                {
                    pdf.NewPage();
                }

                FacturaBase.GenerarCabeceraFactura(pdf, encabezado, logo, fuenteTitulo, fuenteSubtitulo, fuenteNormal, fuenteChica, leyendaImpresion);
                FacturaBase.GenerarDatosCliente(pdf, encabezado, fuenteNormal, fuenteNormalBold);
                pdf.Add(new Paragraph(" ", fuenteChica) { SpacingAfter = 0f });

                var filasPagina = FacturaBase.ObtenerCantidadFilasPagina(pagina, totalPaginas, FilasPaginaComun, FilasUltimaPagina);
                var productosPagina = productos.Skip(indiceProducto).Take(filasPagina).ToList();
                indiceProducto += productosPagina.Count;

                pdf.Add(CrearTablaProductos(productosPagina, fuenteChica, fuenteNormalBold));

                if (pagina == totalPaginas)
                {
                    FacturaBase.DibujarPieFacturaB(
                        writer,
                        pdf,
                        encabezado,
                        datosPercepciones,
                        pagina,
                        totalPaginas,
                        fuenteNormal,
                        fuenteNormalBold,
                        fuenteChica,
                        logger
                    );
                }
            }
        }

        private static PdfPTable CrearTablaProductos(List<FeDetResDto> productos, Font fuenteChica, Font fuenteNormalBold)
        {
            float[] anchos = new float[] { 10f, 50f, 10f, 15f, 15f };
            string[] encabezados = new string[]
            {
                "Código",
                "Producto/Servicio",
                "Cantidad",
                "Precio\nUnitario",
                "Sub Total"
            };

            var tabla = FacturaBase.CrearTablaProductos(anchos, encabezados, fuenteNormalBold);

            foreach (var producto in productos)
            {
                var precioConIva = producto.cmd_pvta + (producto.cmd_pvta * producto.iva_alicuota / 100);

                FacturaBase.AgregarCeldaProducto(tabla, producto.p_id ?? string.Empty, fuenteChica, Element.ALIGN_CENTER);
                FacturaBase.AgregarCeldaProducto(tabla, producto.p_desc ?? string.Empty, fuenteChica, Element.ALIGN_LEFT);
                FacturaBase.AgregarCeldaProducto(tabla, FacturaBase.FormatearImporte(producto.cmd_cantidad), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tabla, FacturaBase.FormatearImporte(precioConIva), fuenteChica, Element.ALIGN_RIGHT);
                FacturaBase.AgregarCeldaProducto(tabla, FacturaBase.FormatearImporte(producto.cmd_subtotal_con_iva), fuenteChica, Element.ALIGN_RIGHT);
            }

            return tabla;
        }

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

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La generación de factura en formato TXT no está implementada.");
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException("La generación de factura en formato XLS no está implementada.");
        }
    }
}


