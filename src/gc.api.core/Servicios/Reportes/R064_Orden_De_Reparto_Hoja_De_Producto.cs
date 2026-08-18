using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R064_Orden_De_Reparto_Hoja_De_Producto : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiOrdenDeRepartoServicio _apiOrdenReparto;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R064_Orden_De_Reparto_Hoja_De_Producto(IUnitOfWork uow, IApiOrdenDeRepartoServicio apiOrdenReparto,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiOrdenReparto = apiOrdenReparto;
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
				string subtit;
				List<OrdenDeRepartoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subtit;

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

				#region Lista 
				CargarRepoHojaDeProductoDeOrdenDeReparto(pdf, registros, chico, normal, normalBold);
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
				throw new NegocioException("Se produjo un error al intentar generar el Reporte de Extracto Bancario. Para mayores datos ver el log.");
			}
		}

		private List<OrdenDeRepartoDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var or_compte = solicitud.Parametros.GetValueOrDefault("orCompte", "")?.ToString() ?? null;

				var listaTemp = _apiOrdenReparto.ObtenerDetalleDeOrdenDeReparto(or_compte);
				var item = listaTemp.First();
				titulo = $"Orden de Reparto N° {or_compte} - {item.ore_desc}";
				subtit = $"Repartidor: {item.rp_nombre} Fecha: {item.or_fecha:dd/MM/yyyy}\nObs: {item.or_obs}";
				return listaTemp;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				return [];
			}
			
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<OrdenDeRepartoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			string subtit;
			List<OrdenDeRepartoDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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

		#region funciones
		public static void CargarRepoHojaDeProductoDeOrdenDeReparto(Document pdf, List<OrdenDeRepartoDetalleDto> registros, Font chico, Font normal, Font normalBold)
		{
			if (registros == null || registros.Count == 0)
				return;

			// Agrupar por rubro
			var grupos = registros
				.GroupBy(x => new { x.rub_id, x.rub_desc })
				.OrderBy(g => g.Key.rub_desc);

			foreach (var grupo in grupos)
			{
				// ============================================================
				// SEPARADOR DE ANCHO COMPLETO
				// ============================================================
				PdfPTable sep = new PdfPTable(1);
				sep.WidthPercentage = 100;

				sep.AddCell(new PdfPCell(new Phrase(" ", chico))
				{
					Border = Rectangle.BOTTOM_BORDER,
					BorderWidthBottom = 1f,
					PaddingBottom = 5
				});

				pdf.Add(sep);

				// ============================================================
				// TÍTULO DEL RUBRO
				// ============================================================
				Font fontRubro = new Font(normalBold.BaseFont, normalBold.Size + 2, Font.BOLD);

				PdfPTable tablaTitulo = new PdfPTable(1);
				tablaTitulo.WidthPercentage = 100;

				tablaTitulo.AddCell(new PdfPCell(new Phrase(
					$"Rubros: {grupo.Key.rub_desc}", fontRubro))
				{
					Border = Rectangle.NO_BORDER,
					PaddingTop = 4,
					PaddingBottom = 6
				});

				pdf.Add(tablaTitulo);

				// ============================================================
				// TABLA DE PRODUCTOS (VISIBLE, ENCABEZADO GRIS)
				// ============================================================
				PdfPTable tabla = new PdfPTable(new float[] { 15, 65, 20 });
				tabla.WidthPercentage = 100;

				BaseColor grisSuave = new BaseColor(230, 230, 230);

				// Encabezados visibles
				tabla.AddCell(CeldaHeaderVisible("Código", normalBold, grisSuave));
				tabla.AddCell(CeldaHeaderVisible("Descripción", normalBold, grisSuave));
				tabla.AddCell(CeldaHeaderVisible("Cant. Enviada", normalBold, grisSuave, Element.ALIGN_RIGHT));

				// Filas de productos
				foreach (var item in grupo)
				{
					tabla.AddCell(CeldaDatoVisible(item.p_id, normal));
					tabla.AddCell(CeldaDatoVisible(item.p_desc, normal));

					string cantidad = item.PermiteDecimales
						? item.pcd_enviada.ToString("N2")
						: ((int)item.pcd_enviada).ToString();

					tabla.AddCell(CeldaDatoVisible(cantidad, normal, Element.ALIGN_RIGHT));
				}

				pdf.Add(tabla);
			}


		}

		private static PdfPCell CeldaHeaderVisible(string texto, Font font, BaseColor fondo, int align = Element.ALIGN_LEFT)
		{
			return new PdfPCell(new Phrase(texto, font))
			{
				BackgroundColor = fondo,
				Border = Rectangle.BOX,
				PaddingTop = 4,
				PaddingBottom = 4,
				HorizontalAlignment = align
			};
		}

		private static PdfPCell CeldaDatoVisible(string texto, Font font, int align = Element.ALIGN_LEFT)
		{
			return new PdfPCell(new Phrase(texto, font))
			{
				Border = Rectangle.BOX,
				PaddingTop = 3,
				PaddingBottom = 3,
				HorizontalAlignment = align
			};
		}
		#endregion
	}
}
