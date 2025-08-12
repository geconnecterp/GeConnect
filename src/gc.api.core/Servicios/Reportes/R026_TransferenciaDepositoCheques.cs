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
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;

namespace gc.api.core.Servicios.Reportes
{
	public class R026_TransferenciaDepositoCheques : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IConsultaServicio _consultaServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;

		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ICuentaServicio _cuentaSv;
		private readonly ILogger _logger;

		public R026_TransferenciaDepositoCheques(IUnitOfWork uow, IConsultaServicio consulta, IOrdenDePagoServicio ordenDePagoServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_consultaServicio = consulta;
			_ordenDePagoServicio = ordenDePagoServicio;

			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
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
				string tit;
				string fDesde;
				string fHasta;
				List<OrdenDePagoConsultaDto> registros = ObtenerDatos(solicitud, out tit, out fDesde, out fHasta);

				var importe = registros.Sum(x => x.op_importe);

				//COMPLETAMOS EL TITULO DEL REPORTE AGREGANDO LA DENOMINACIÓN DE LA CUENTA
				//tit += cliente.Cta_Denominacion;
				solicitud.Titulo = tit;
				solicitud.SubTitulo = $"Fecha desde {fDesde} hasta {fHasta}";

				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{
					
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

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, logo, _empresaGeco);

				// Convertir la tabla en un Phrase
				Phrase phrase = new Phrase();
				phrase.Add(tabla);

				// Crear el HeaderFooter con el Phrase que contiene la tabla
				HeaderFooter header = new(phrase, false)
				{
					Alignment = Element.ALIGN_TOP,
					BorderWidth = 0,
				};

				pdf.Header = header;
				#endregion

				pdf.Open();

				#region Lista de Ordenes de Pago
				HelperPdf.CargarTablaConceptosOrdenesDePago(pdf, registros, chico, normalBold);
				#endregion

				//Chunk linebreak = new Chunk(new LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, 5));
				//pdf.Add(linebreak);

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
				_logger.LogError(ex, "Error en R023");
				throw new NegocioException("Se produjo un error al intentar generar el Reporte de Orden de Pago Directa. Para mayores datos ver el log.");
			}
		}



		private List<OrdenDePagoConsultaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string fDesdePrint, out string fHastaPrint)
		{
			fDesdePrint = solicitud.Parametros.GetValueOrDefault("Date1Print", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			fHastaPrint = solicitud.Parametros.GetValueOrDefault("Date2Print", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fDesde = solicitud.Parametros.GetValueOrDefault("Date1", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var fHasta = solicitud.Parametros.GetValueOrDefault("Date2", "").ToString() ?? DateTime.Now.ToString("dd-MM-yyyy");
			var rel01 = solicitud.Parametros.GetValueOrDefault("rel01", "") == null ? [] : solicitud.Parametros.GetValueOrDefault("rel01", "").ToString().Split(",").ToList();
			var rel02 = solicitud.Parametros.GetValueOrDefault("rel02", "") == null ? [] : solicitud.Parametros.GetValueOrDefault("rel02", "").ToString().Split(",").ToList();
			var rel03_ = solicitud.Parametros.GetValueOrDefault("rel03", "") == null ? [] : solicitud.Parametros.GetValueOrDefault("rel03", "").ToString().Split(",").ToList();
			var rel03 = rel03_.Select(x => new ComboGenDto() { Id = x, Descripcion = x }).ToList();
			titulo = $"Consulta de Ordenes de Pago";
			return _ordenDePagoServicio.CargarOrdenDePagoConsultaListaReporte(new infraestructura.Dtos.Almacen.Request.BuscarOrdenesDePagoRequest() { Buscar = "", Date1 = fDesde, Date2 = fHasta, Id = "", Id2 = "", Rel01 = rel01, Rel02 = rel02, Rel03 = rel03 });

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string fDesde;
			string fHasta;
			List<OrdenDePagoConsultaDto> registros = ObtenerDatos(solicitud, out tit, out fDesde, out fHasta);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{

			}).ToList();


			#endregion

			return GeneraTXT(regs, _campos);
		}

		public string GenerarXls(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string fDesde;
			string fHasta;
			List<OrdenDePagoConsultaDto> registros = ObtenerDatos(solicitud, out tit, out fDesde, out fHasta);

			if (registros == null || registros.Count == 0)
			{
				throw new NegocioException($"No se encontraron registros.");
			}

			//hago el modelo de dato aca ya que necesito los datos de la cuenta
			var regs = registros.Select(x => new
			{

			}).ToList();

			#endregion

			return GeneraFileXLS(regs, _titulos, _campos);
		}
	}
}
