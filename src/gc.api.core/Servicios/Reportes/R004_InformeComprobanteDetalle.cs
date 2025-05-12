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
using DocumentFormat.OpenXml.Vml;
using Microsoft.Extensions.Logging;

namespace gc.api.core.Servicios.Reportes
{
    public class R004_InformeComprobanteDetalle : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;


        public R004_InformeComprobanteDetalle(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Fecha", "N° Cmpte", "Neto", "IVA", "TOTAL", "N° Doc", "Cargado", "Usuario" };
            _campos = new List<string> { "Fecha", "NCmpte", "Neto", "IVA", "Total", "NDoc", "Cargado", "Usuario" };
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
                List<ConsCompDetDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }
                var neto = registros.Sum(x => x.Cm_neto);
                var iva = registros.Sum(x => x.Cm_iva);
                var tot = registros.Sum(x => x.Cm_total);



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
                    Fecha = x.Cm_fecha,
                    NCmpte = $"({x.Tco_id}) {x.Cm_compte}",
                    Neto = x.Cm_neto,
                    IVA = x.Cm_iva,
                    Total = x.Cm_total,
                    Cargado = x.Cm_fecha_carga,
                    Usuario = x.Usu_id,


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
                anchos = [10f, 20f, 13f, 13f, 14f, 10f, 10f, 10f];

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
                    { "Neto", neto},
                    { "IVA",iva} ,
                    { "Total",tot} };

                HelperPdf.GenerarListadoDesdeLista(pdf, regs, _campos, anchos, chico, false, true, totales);

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

        private List<ConsCompDetDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            string mes = solicitud.Parametros.GetValueOrDefault("mes", "").ToString();
            bool relCuil = solicitud.Parametros.GetValueOrDefault("relCuil", "").ToBoolean();
            string userId = solicitud.Parametros.GetValueOrDefault("userId", "").ToString() ?? "";
            titulo = $"Ventas Registradas {mes} \r\nCuenta: ({ctaId}) ";
            return _consultaServicio.ConsultaComprobantesMesDetalle(ctaId, mes, relCuil, userId);

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsCompDetDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }
            var neto = registros.Sum(x => x.Cm_neto);
            var iva = registros.Sum(x => x.Cm_iva);
            var tot = registros.Sum(x => x.Cm_total);


            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                Fecha = x.Cm_fecha,
                NCmpte = $"({x.Tco_id}) {x.Cm_compte}",
                Neto = x.Cm_neto,
                IVA = x.Cm_iva,
                Total = x.Cm_total,
                Cargado = x.Cm_fecha_carga,
                Usuario = x.Usu_id,


            }).ToList();


            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsCompDetDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }
            var neto = registros.Sum(x => x.Cm_neto);
            var iva = registros.Sum(x => x.Cm_iva);
            var tot = registros.Sum(x => x.Cm_total);


            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                Fecha = x.Cm_fecha,
                NCmpte = $"({x.Tco_id}) {x.Cm_compte}",
                Neto = x.Cm_neto,
                IVA = x.Cm_iva,
                Total = x.Cm_total,
                Cargado = x.Cm_fecha_carga,
                Usuario = x.Usu_id,


            }).ToList();


            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
