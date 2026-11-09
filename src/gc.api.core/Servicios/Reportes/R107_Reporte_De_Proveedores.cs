using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.LineaCaja;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R107_Reporte_De_Proveedores : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiCajaServicio _iCajaSrv;
		private readonly IABMClienteServicio _iCliSrv;
		private readonly IABMProveedorServicio _iProvSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R107_Reporte_De_Proveedores(IUnitOfWork uow, IConsultaServicio consSrv,
													IABMClienteServicio iCliSrv, IABMProveedorServicio iProvSrv,
											  IOptions<EmpresaGeco> empresa, IApiCajaServicio iCajaSrv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_iCliSrv = iCliSrv;
			_iProvSrv = iProvSrv;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_iCajaSrv = iCajaSrv;
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
				List<ABMProveedorSearchDto> registros = ObtenerDatos(solicitud, out tit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = string.Empty;

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

				var chicoplus = HelperPdf.FontSuperChicoPredeterminado();
				var chico = HelperPdf.FontChicoPredeterminado();
				var chicoBold = HelperPdf.FontChicoPredeterminado(true);
				var normal = HelperPdf.FontNormalPredeterminado();
				var normalBold = HelperPdf.FontNormalPredeterminado(true);
				var titulo = HelperPdf.FontTituloPredeterminado();
				var tituloBig = HelperPdf.FontTituloBigBoldPredeterminado();
				var subtitulo = HelperPdf.FontSubtituloPredeterminado();

				#region Generación de Cabecera               

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, tituloBig, logo, _empresaGeco);

				// Convertir la tabla en un Phrase
				Phrase phrase = [tabla];

				// Crear el HeaderFooter con el Phrase que contiene la tabla
				HeaderFooter header = new(phrase, false)
				{
					Alignment = Element.ALIGN_TOP,
					BorderWidth = 0,
				};

				pdf.Header = header;
				#endregion

				pdf.Open();

				#region Armado de Reporte
				CargarRepoListadoDeProveedores(pdf, registros, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
				_logger.LogError(ex, "Error en R031");
				throw new NegocioException("Se produjo un error al intentar generar el Reporte Analítico de Operaciones. Para mayores datos ver el log.");
			}
		}

		#region Funciones de generacion de secciones de reportes
		public static void CargarRepoListadoDeProveedores(Document pdf, List<ABMProveedorSearchDto> registros, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron registros.", normal));
				return;
			}

			
		}


		#endregion

		private List<ABMProveedorSearchDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo)
		{
			try
			{
				var ret = new List<ABMProveedorSearchDto>();
				var id = solicitud.Parametros.GetValueOrDefault("id", "")?.ToString() ?? null;
				var id2 = solicitud.Parametros.GetValueOrDefault("id2", "")?.ToString() ?? null;
				var buscar = solicitud.Parametros.GetValueOrDefault("buscar", "")?.ToString() ?? null;
				var rel01 = solicitud.Parametros.GetValueOrDefault("rel01", "")?.ToString() ?? null;
				var listaRel01 = (solicitud.Parametros.GetValueOrDefault("rel01", "")?.ToString() ?? "")
				.Split(',', StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.ToList();


				ret = _iProvSrv.Buscar(new QueryFilters()
				{
					Id = id,
					Id2 = id2,
					Buscar = buscar,
					Rel01 = listaRel01,
					Registros = int.MaxValue,
					Pagina = 1,
					Sort = "cta_id"
				});
				titulo = $"Listado de Proveedores";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			List<ABMProveedorSearchDto> registros = ObtenerDatos(solicitud, out tit);

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
			List<ABMProveedorSearchDto> registros = ObtenerDatos(solicitud, out tit);

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
