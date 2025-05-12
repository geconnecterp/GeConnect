using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace gc.api.core.Servicios.Reportes
{
    public class R008_InformeRecepcionProveedorDetalle : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R008_InformeRecepcionProveedorDetalle(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Código", "Producto", "Ord.Compra", "U.P.", "FR Bultos", "FR Cant.Sueltas", "FR Cantidad", "Rep. Saldo", "Rep. Cant.Sueltas", "Rep. Total" };
            _campos = new List<string> { "Codigo", "Producto", "OrdCompra", "Up", "FrBultos", "FrCantSueltas", "FrCantidad", "RepSaldo","RepCantSueltas","RepTotal" };
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
                string ctaId;
                string tit;
                List<ConsRecepcionProveedorDetalleDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }
            



                //buscando datos del cliente
                var c = _cuentaSv.GetCuentaComercialLista(ctaId, 'T');

                if (c == null || c.Count == 0)
                {
                    throw new NegocioException($"No se encontraron datos del cliente {ctaId}.");
                }
                var cliente = c[0];
                cliente.Monto = 0m;
                cliente.MontoEtiqueta = "";
                //COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
                tit += cliente.Cta_Denominacion;
                solicitud.Titulo = tit;
                solicitud.Cuenta = cliente;


                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
                    Codigo = x.P_id,
                    Producto = x.P_desc,
                    OrdCompra = x.Oc_compte,
                    Up = x.Rpd_unidad_pres,
                    FrBultos = x.Rpd_bulto_compte,
                    FrCantSueltas = x.Rpd_unidad_suelta_compte,
                    FrCantidad = x.Rpd_cantidad_compte,
                    RepSaldo = x.Rpd_bulto_recibidos,
                    RepCantSueltas = x.Rpd_unidad_suelta,
                    RepTotal = x.Rpd_Cantidad,
                }).ToList();

                #endregion
                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, false);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

                #endregion
                //****=============================****/
                //****  CAMBIAR ANCHOS DE COLUMNAS ****
                //****=============================****/
                anchos = [7f, 20f, 10f, 7f, 7f, 7f, 7f, 7f, 7f, 10f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera               
                PdfPTable tabla = GeneraCabeceraPdf(solicitud, logo, chico, titulo, _empresaGeco);
                Phrase phrase = new Phrase();
                phrase.Add(tabla);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new HeaderFooter(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                };

                pdf.Header = header;
                #endregion

                pdf.Open();

                //#region Datos del Cliente o Proveedor
                //tabla = HelperPdf.GeneraTabla(4, [20f, 70f, 5f, 5f], 100, 10, 10);
                ////hay que ir a buscar los datos del cliente para presentarlos en pantalla.
                //HelperPdf.CargarTablaClienteProveedor(pdf, c[0], normal, normalBold);
                //#endregion

                #region Carga del Listado

                GenerarTablaRecepcionProveedorDetalle(pdf, registros, anchos, chico);

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
                _logger.LogError(ex, "Error en R003");
                throw new NegocioException("Se produjo un error al intentar generar el Informe de Cuenta Corriente. Para mayores datos ver el log.");
            }
        }



        private List<ConsRecepcionProveedorDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            string cmptId = solicitud.Parametros.GetValueOrDefault("cmptId", "").ToString();
            titulo = $"Detalle Recepción de Productos \r\nProveedor: ({ctaId}) ";
            return _consultaServicio.ConsultaRecepcionProveedorDetalle(cmptId);
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsRecepcionProveedorDetalleDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                Codigo = x.P_id,
                Producto = x.P_desc,
                OrdCompra = x.Oc_compte,
                Up = x.Rpd_unidad_pres,
                FrBultos = x.Rpd_bulto_compte,
                FrCantSueltas = x.Rpd_unidad_suelta_compte,
                FrCantidad = x.Rpd_cantidad_compte,
                RepSaldo = x.Rpd_bulto_recibidos,
                RepCantSueltas = x.Rpd_unidad_suelta,
                RepTotal = x.Rpd_Cantidad,
            }).ToList();


            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsRecepcionProveedorDetalleDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                Codigo = x.P_id,
                Producto = x.P_desc,
                OrdCompra = x.Oc_compte,
                Up = x.Rpd_unidad_pres,
                FrBultos = x.Rpd_bulto_compte,
                FrCantSueltas = x.Rpd_unidad_suelta_compte,
                FrCantidad = x.Rpd_cantidad_compte,
                RepSaldo = x.Rpd_bulto_recibidos,
                RepCantSueltas = x.Rpd_unidad_suelta,
                RepTotal = x.Rpd_Cantidad,
            }).ToList();

            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }


        public static void GenerarTablaRecepcionProveedorDetalle(Document pdf, List<ConsRecepcionProveedorDetalleDto> datos,float[] anchos, Font fuente)
        {
            var tabla = HelperPdf.GeneraTabla(10, anchos, 100, 5, 10);

            var fontNegrita = new Font(fuente);
            fontNegrita.SetStyle(Font.BOLD);

            // Primera fila del header
            tabla.AddCell(HeaderCell("Código", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Producto", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Orden de Compra", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Unidad de Presentación", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Factura / Remito", 3, fontNegrita, new BaseColor(0, 123, 255))); // Bootstrap Primary
            tabla.AddCell(HeaderCell("Recepción", 3, fontNegrita, new BaseColor(108, 117, 125))); // Bootstrap Secondary

            // Segunda fila del header
            tabla.AddCell(BlankCell(2)); // Salta columnas 1 y 2
            tabla.AddCell(HeaderCell("Compra", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Presentación", 1, fontNegrita));
            tabla.AddCell(HeaderCell("Bultos", 1, fontNegrita, new BaseColor(0, 123, 255)));
            tabla.AddCell(HeaderCell("Cant. Sueltas", 1, fontNegrita, new BaseColor(0, 123, 255)));
            tabla.AddCell(HeaderCell("Cantidad", 1, fontNegrita, new BaseColor(0, 123, 255)));
            tabla.AddCell(HeaderCell("Bultos", 1, fontNegrita, new BaseColor(108, 117, 125)));
            tabla.AddCell(HeaderCell("Cant. Sueltas", 1, fontNegrita, new BaseColor(108, 117, 125)));
            tabla.AddCell(HeaderCell("Total", 1, fontNegrita, new BaseColor(108, 117, 125)));

            var camposSinFormatear = new List<string> { "P_id" };
            // Detalles
            bool alternar = false;
            foreach (var item in datos ?? Enumerable.Empty<ConsRecepcionProveedorDetalleDto>())
            {
                var colorFondo = alternar ? BaseColor.White : new BaseColor(245, 245, 245);
                alternar = !alternar;

                tabla.AddCell(CeldaTexto(item.P_id, fuente, colorFondo,false));
                tabla.AddCell(CeldaTexto(item.P_desc, fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Oc_compte, fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_unidad_pres.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_bulto_compte.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_unidad_suelta_compte.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_cantidad_compte.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_bulto_recibidos.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_unidad_suelta.ToString("N2"), fuente, colorFondo));
                tabla.AddCell(CeldaTexto(item.Rpd_Cantidad.ToString("N2"), fuente, colorFondo));
            }

            pdf.Add(tabla);
        }

    }
}
