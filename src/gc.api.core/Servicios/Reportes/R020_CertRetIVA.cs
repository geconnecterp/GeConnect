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
using iTextSharp.text.pdf.draw;
using System.Drawing;

namespace gc.api.core.Servicios.Reportes
{
    public class R020_CertRetIVA : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;
        private readonly ICuentaServicio _cuentaSv;
        private readonly ILogger _logger;

        public R020_CertRetIVA(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = ["IVA Nro", "ID", "IVA CUIT", "IVA Razón Social", "IVA Domicilio", "IVA Fecha", "Base", "Reten", "Estado", "OP_Compte", "IVA Actu", "IVA Impreso", "Razón Social", "CUIT", "IIBB", "Domicilio",];
            _campos = ["civaNro", "ctaId", "civaCuit", "civaRazSoc", "civaDomicilio", "civaFecha", "civaBase", "civaReten", "civaEstado", "opCompte", "civaActu", "civaImpreso", "empRazonSocial", "empCuit", "empIbNro", "empDomicilio",];
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
                List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

                if (registros == null || registros.Count == 0)
                {
                    throw new NegocioException($"No se encontraron registros para poder generar el certificado correspondiente.");
                }

                //var importe = registros.Sum(x => x.Cc_importe);
                
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
                //tit += cliente.Cta_Denominacion;
                solicitud.Titulo = tit;
                solicitud.Cuenta = cliente;


                //hago el modelo de dato aca ya que necesito los datos de la cuenta
                var regs = registros.Select(x => new
                {
					#region Campos
					civaNro = x.civa_nro,
					ctaId = x.cta_id,
					civaCuit = x.civa_cuit,
					civaRazSoc = x.civa_raz_soc,
					civaDomicilio = x.civa_domicilio,
					civaFecha = x.civa_fecha,
					civaBase = x.civa_base,
					civaReten = x.civa_reten,
					civaEstado = x.civa_estado,
					opCompte = x.op_compte,
					civaActu = x.civa_actu,
					civaImpreso = x.civa_impreso,
					empRazonSocial = x.emp_razon_social,
					empCuit = x.emp_cuit,
					empIbNro = x.emp_ib_nro,
					empDomicilio = x.emp_domicilio,
					#endregion
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

                PdfPTable tabla = GeneraCabeceraPdf3C(solicitud, chico, titulo, logo, _empresaGeco);

                // Convertir la tabla en un Phrase
                Phrase phrase = new();
                phrase.Add(tabla);

                // Crear el HeaderFooter con el Phrase que contiene la tabla
                HeaderFooter header = new(phrase, false)
                {
                    Alignment = Element.ALIGN_TOP,
                    BorderWidth = 0,
                    //Border=1
                    //BorderWidthBottom = 1,   

                };

                pdf.Header = header;
                #endregion

                pdf.Open();


				//TODO MARCE: Completar el cuerpo del reporte

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



        private List<CertRetenIVADto> ObtenerDatos(ReporteSolicitudDto solicitud, out string ctaId, out string titulo)
        {
            //Se obtienen los parámetros del reporte
            ctaId = solicitud.Parametros.GetValueOrDefault("ctaId", "").ToString() ?? "";
            string cmptId = solicitud.Parametros.GetValueOrDefault("op_compte", "").ToString();          
            titulo = $"Certificado de Retención de IVA";
            return _consultaServicio.ConsultaCertRetenIVA(cmptId);

        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
                civaNro = x.civa_nro,
                ctaId = x.cta_id,
                civaCuit = x.civa_cuit,
                civaRazSoc = x.civa_raz_soc,
                civaDomicilio=x.civa_domicilio,
                civaFecha = x.civa_fecha,
                civaBase = x.civa_base,
                civaReten = x.civa_reten,
                civaEstado= x.civa_estado,
                opCompte=x.op_compte,
                civaActu=x.civa_actu,
                civaImpreso=x.civa_impreso,
                empRazonSocial=x.emp_razon_social,
                empCuit=x.emp_cuit,
                empIbNro = x.emp_ib_nro,
				empDomicilio = x.emp_domicilio,
			}).ToList();


            #endregion

            return GeneraTXT(regs, _campos);
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            #region Obteniendo registros desde la base de datos
            string ctaId;
            string tit;
            List<CertRetenIVADto> registros = ObtenerDatos(solicitud, out ctaId, out tit);

            if (registros == null || registros.Count == 0)
            {
                throw new NegocioException($"No se encontraron registros de la cuenta corriente {ctaId}.");
            }

            //hago el modelo de dato aca ya que necesito los datos de la cuenta
            var regs = registros.Select(x => new
            {
				civaNro = x.civa_nro,
				ctaId = x.cta_id,
				civaCuit = x.civa_cuit,
				civaRazSoc = x.civa_raz_soc,
				civaDomicilio = x.civa_domicilio,
				civaFecha = x.civa_fecha,
				civaBase = x.civa_base,
				civaReten = x.civa_reten,
				civaEstado = x.civa_estado,
				opCompte = x.op_compte,
				civaActu = x.civa_actu,
				civaImpreso = x.civa_impreso,
				empRazonSocial = x.emp_razon_social,
				empCuit = x.emp_cuit,
				empIbNro = x.emp_ib_nro,
				empDomicilio = x.emp_domicilio,
			}).ToList();

            #endregion

            return GeneraFileXLS(regs, _titulos, _campos);
        }
    }
}
