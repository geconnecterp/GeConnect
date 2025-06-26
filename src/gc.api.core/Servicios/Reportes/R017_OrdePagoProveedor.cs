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

namespace gc.api.core.Servicios.Reportes
{
    public class R017_OrdePagoProveedor : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R017_OrdePagoProveedor(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Descripción", "Importe",  };
            _campos = new List<string> { "Descripcion", "Importe", };
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
                List<ConsOrdPagoDetExtendDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }

                var importe = registros.Sum(x => x.Cc_importe);
                
                //buscando datos del cliente
                var cta = _cuentaSv.GetCuentaComercialLista(ctaId, 'T');

                if (cta == null || cta.Count == 0)
                {
                    throw new NegocioException($"No se encontraron datos del cliente {ctaId}.");
                }
                var cliente = cta[0];
                cliente.Monto = 0m;
                cliente.MontoEtiqueta = "";
                //COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
                tit += cliente.Cta_Denominacion;
                solicitud.Titulo = tit;
                solicitud.Cuenta = cliente;


                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
                    x.Grupo,
                    GrDesc = x.Grupo_des,
                    Descripcion = x.Concepto,
                    Importe = x.Cc_importe,                    
                }).ToList();

                #endregion
                #region Scripts PDF
                #region instanciamos el pdf
                pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, true);

                // Agregar el evento de pie de página
                writer.PageEvent = new CustomPdfPageEventHelper(solicitud.Observacion);

                var logo = HelperPdf.CargaLogo(solicitud.LogoPath, 20, pdf.PageSize.Height - 10, 20);

                #endregion
                //****=============================****/
                //****  CAMBIAR ANCHOS DE COLUMNAS ****
                //****=============================****/
                anchos = [70f, 30f];

                var chico = HelperPdf.FontChicoPredeterminado();
                var normal = HelperPdf.FontNormalPredeterminado();
                var normalBold = HelperPdf.FontNormalPredeterminado(true);
                var titulo = HelperPdf.FontTituloPredeterminado();
                var subtitulo = HelperPdf.FontSubtituloPredeterminado();

                #region Generación de Cabecera               

                PdfPTable tabla = GeneraCabeceraPDF2(solicitud, chico, titulo, logo, _empresaGeco);

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

                //#region Datos del Cliente o Proveedor
                //tabla = HelperPdf.GeneraTabla(4, [20f, 70f, 5f, 5f], 100, 10, 10);
                ////hay que ir a buscar los datos del cliente para presentarlos en pantalla.
                //HelperPdf.CargarTablaClienteProveedor(pdf, c[0], normal, normalBold);
                //#endregion

                #region Carga del Listado

                HelperPdf.GeneraCabeceraLista(pdf, _titulos, anchos, normalBold);
                //utilizo cliente.Monto para el total previamente cargado, pero ya dejo preparado el helper para definir
                //multiples campos con totales. Ejemplo: Debe, Haber y Saldo.
                //var totales = new Dictionary<string, decimal>
                //        {
                //            { "Debe", 15420.50m },
                //            { "Haber", 10325.30m },
                //            { "Saldo", 5095.20m }
                //        };
                var totales = new Dictionary<string, decimal>{
                    { "Importe", importe} };
                    

                //HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico, false, true, totales);
                var aTotalizar = new List<string> { "Importe" };
                HelperPdf.GenerarListadoAgrupado(pdf, regs, _campos, _titulos, anchos, "Grupo", "GrDesc", chico, HelperPdf.FontSubtituloPredeterminado(),null,false,null);
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



        private List<ConsOrdPagoDetExtendDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            string cmptId = solicitud.Parametros.GetValueOrDefault("op_compte", "").ToString();          
            titulo = $"Orden de Pago Proveedor {cmptId}";
            return _consultaServicio.ConsultaOrdenDePagoProveedor(cmptId);

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsOrdPagoDetExtendDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                x.Grupo,
                GrDesc = x.Grupo_des,
                Descripcion = x.Concepto,
                Importe = x.Cc_importe,
            }).ToList();


            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsOrdPagoDetExtendDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                x.Grupo,
                GrDesc = x.Grupo_des,
                Descripcion = x.Concepto,
                Importe = x.Cc_importe,
            }).ToList();

            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
