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
using System.Text.Json;

namespace gc.api.core.Servicios.Reportes
{
	public class R108_Reporte_De_Clientes : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiCajaServicio _iCajaSrv;
		private readonly IABMClienteServicio _iCliSrv;
		private readonly IABMProveedorServicio _iProvSrv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R108_Reporte_De_Clientes(IUnitOfWork uow, IConsultaServicio consSrv,
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
				string subTit;
				List<ABMClienteSearchDto> registros = ObtenerDatos(solicitud, out tit, out subTit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subTit;

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

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, tituloBig, logo, _empresaGeco, 1, 1, true);

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

				// Espacio
				pdf.Add(new Paragraph(" ", normal));
				pdf.Add(new Paragraph(" ", normal));

				#region Armado de Reporte
				CargarRepoListadoDeClientes(pdf, registros, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoListadoDeClientes(Document pdf, List<ABMClienteSearchDto> registros, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No se encontraron registros.", normal));
				return;
			}

			var tabla = new PdfPTable(8)
			{
				WidthPercentage = 100,
				HeaderRows = 1
			};

			tabla.SetWidths(
			[
				10f, // Cuenta
				35f, // Denominación
				8f,  // Legajo
				10f, // Tipo Doc
				15f, // Documento
				40f, // Domicilio
				15f, // Teléfono
				25f  // Email
			]);

			AgregarCabecera(tabla, "Cuenta", chicoBold);
			AgregarCabecera(tabla, "Denominación", chicoBold);
			AgregarCabecera(tabla, "Legajo", chicoBold);
			AgregarCabecera(tabla, "Tipo Doc", chicoBold);
			AgregarCabecera(tabla, "Documento", chicoBold);
			AgregarCabecera(tabla, "Domicilio", chicoBold);
			AgregarCabecera(tabla, "Teléfono", chicoBold);
			AgregarCabecera(tabla, "Email", chicoBold);

			foreach (var item in registros)
			{
				tabla.AddCell(CrearCelda(item.cta_id, chico, Element.ALIGN_CENTER));
				tabla.AddCell(CrearCelda(item.cta_denominacion, chico, Element.ALIGN_LEFT));
				tabla.AddCell(CrearCelda(string.Empty, chico, Element.ALIGN_CENTER));
				tabla.AddCell(CrearCelda(item.tdoc_desc, chico, Element.ALIGN_CENTER));
				tabla.AddCell(CrearCelda(item.cta_documento, chico, Element.ALIGN_CENTER));

				var tablaDomicilio = new PdfPTable(2)
				{
					WidthPercentage = 100
				};

				tablaDomicilio.SetWidths(new float[] { 95f, 5f });

				// Domicilio
				var domicilioCell = new PdfPCell(
					new Phrase(item.cta_domicilio ?? string.Empty, chico))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					Padding = 0
				};

				tablaDomicilio.AddCell(domicilioCell);

				// ProvId
				var provCell = new PdfPCell(
					new Phrase(item.prov_id ?? string.Empty, chico))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					BackgroundColor = BaseColor.LightGray,
					Padding = 2
				};

				tablaDomicilio.AddCell(provCell);

				var celdaDomicilio = new PdfPCell(tablaDomicilio)
				{
					Padding = 2,
					VerticalAlignment = Element.ALIGN_MIDDLE
				};

				tabla.AddCell(celdaDomicilio);

				tabla.AddCell(CrearCelda(item.cta_celu, chico, Element.ALIGN_CENTER));
				tabla.AddCell(CrearCelda(item.cta_email, chico, Element.ALIGN_CENTER));
			}

			pdf.Add(tabla);
		}

		private static void AgregarCabecera(PdfPTable tabla, string texto, Font fuente)
		{
			var celda = new PdfPCell(new Phrase(texto, fuente))
			{
				BackgroundColor = BaseColor.LightGray,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Padding = 4
			};

			tabla.AddCell(celda);
		}

		private static PdfPCell CrearCelda(string texto, Font fuente, int align = 0)
		{
			return new PdfPCell(new Phrase(texto ?? string.Empty, fuente))
			{
				Padding = 3,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				HorizontalAlignment = align
			};
		}
		#endregion

		private List<ABMClienteSearchDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subTit)
		{
			try
			{
				var ret = new List<ABMClienteSearchDto>();
				var id = solicitud.Parametros.GetValueOrDefault("id", "")?.ToString() ?? null;
				var id2 = solicitud.Parametros.GetValueOrDefault("id2", "")?.ToString() ?? null;
				var buscar = solicitud.Parametros.GetValueOrDefault("buscar", "")?.ToString() ?? null;
				var filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;
				var listaRel01 = JsonSerializer.Deserialize<List<string>>(
					solicitud.Parametros.GetValueOrDefault("rel01", "[]")?.ToString() ?? "[]"
				) ?? [];

				var listaRel02 = JsonSerializer.Deserialize<List<string>>(
					solicitud.Parametros.GetValueOrDefault("rel02", "[]")?.ToString() ?? "[]"
				) ?? [];

				ret = _iCliSrv.Buscar(new QueryFilters()
				{
					Id = id,
					Id2 = id2,
					Buscar = buscar,
					Rel01 = listaRel01,
					Rel02 = listaRel02,
					Registros = int.MaxValue,
					Pagina = 1,
					Sort = "cta_id"
				});
				titulo = $"Listado de Cuentas Comerciales";
				subTit = filtrosString;
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subTit = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subTit;
			List<ABMClienteSearchDto> registros = ObtenerDatos(solicitud, out tit, out subTit);

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
			string subTit;
			List<ABMClienteSearchDto> registros = ObtenerDatos(solicitud, out tit, out subTit);

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
