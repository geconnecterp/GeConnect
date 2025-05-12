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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R005_InformeOPago : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R005_InformeOPago(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "N° OP", "Tipo", "Fecha", "Importe" };
            _campos = new List<string> { "NroOp", "Tipo", "Fecha", "Importe" };
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
                List<ConsOrdPagosDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
                }
                var importe = registros.Sum(x => x.Op_importe);


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
                    NroOp = x.Op_compte,
                    Tipo = x.Opt_desc,
                    Fecha = x.Op_fecha,
                    Importe = x.Op_importe,
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
                anchos = [25f,25f,25f,25f];

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
                    { "Importe", importe}
                    };

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



        private List<ConsOrdPagosDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            var fechaStr = solicitud.Parametros.GetValueOrDefault("fechaD", "").ToString();
            if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out DateTime fechaD))
            {
                throw new NegocioException("La fecha desde es inválida.");
            }
            fechaD = fechaStr.ToDateTime();

            fechaStr = solicitud.Parametros.GetValueOrDefault("fechaH", "").ToString();
            if (string.IsNullOrEmpty(fechaStr) || !DateTime.TryParse(fechaStr, out DateTime fechaH))
            {
                throw new NegocioException("La fecha hasta es inválida.");
            }
            fechaH = fechaStr.ToDateTime();

            titulo = $"Informe de Ordenes de Pago\r\nPeriodo: desde {fechaD.ToString("dd/MM/yy")} al {fechaH.ToString("dd/MM/yy")} ";

            return _consultaServicio.ConsultaOrdenesDePagoProveedor(ctaId, fechaD, fechaH, "%", "%");

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsOrdPagosDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }
            
            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                NroOp = x.Op_compte,
                Tipo = x.Opt_desc,
                Fecha = x.Op_fecha,
                Importe = x.Op_importe,
            }).ToList();


            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<ConsOrdPagosDto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }
           
            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                NroOp = x.Op_compte,
                Tipo = x.Opt_desc,
                Fecha = x.Op_fecha,
                Importe = x.Op_importe,
            }).ToList();


            #endregion

            return GeneraFileXLS(regs, _titulos, _campos,"O.Pago");
        }
    }
}
