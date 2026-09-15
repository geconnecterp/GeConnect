using DocumentFormat.OpenXml.Drawing.Diagrams;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R099_Reporte_Ordenes_De_Reparto : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiOrdenDeRepartoServicio _apiOdeR;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R099_Reporte_Ordenes_De_Reparto(IUnitOfWork uow, IApiOrdenDeRepartoServicio apiOdeR,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiOdeR = apiOdeR;
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
				string filtrosString;
				List<OrdenDeRepartoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
				CargarRepoDeOrdenesDeReparto(pdf, registros, filtrosString, chicoplus, normal, chicoBold, normalBold, titulo, tituloBig);
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
		public static void CargarRepoDeOrdenesDeReparto(Document pdf, List<OrdenDeRepartoListaDto> registros, string filtrosString, Font chico, Font normal, Font chicoBold, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new([12f, 15f, 15f, 25f, 25f, 10f])
			{
				WidthPercentage = 100,
				SpacingBefore = 5f,
				HeaderRows = 1
			};

			// Encabezados
			void Header(string texto)
			{
				tabla.AddCell(new PdfPCell(new Phrase(texto, chicoBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4f
				});
			}

			Header("ID");
			Header("Estado");
			Header("Generado");
			Header("Repartidor");
			Header("Observaciones");
			Header("Cant. Ped.");

			// ============================
			// FILAS
			// ============================
			bool alternado = false;

			foreach (var item in registros)
			{
				BaseColor fondo = alternado ? new BaseColor(245, 245, 245) : BaseColor.White;
				alternado = !alternado;

				// ID
				tabla.AddCell(new PdfPCell(new Phrase(item.or_compte, chico))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = fondo
				});

				// Estado
				tabla.AddCell(new PdfPCell(new Phrase(item.ore_desc, chico))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = fondo
				});

				// Fecha generada
				tabla.AddCell(new PdfPCell(new Phrase(item.or_fecha.ToString("dd/MM/yy"), chico))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = fondo
				});

				// Repartidor
				tabla.AddCell(new PdfPCell(new Phrase(item.rp_nombre, chico))
				{
					HorizontalAlignment = Element.ALIGN_LEFT,
					BackgroundColor = fondo
				});

				// Observaciones
				tabla.AddCell(new PdfPCell(new Phrase(item.or_obs, chico))
				{
					HorizontalAlignment = Element.ALIGN_LEFT,
					BackgroundColor = fondo
				});

				// Cantidad de pedidos
				tabla.AddCell(new PdfPCell(new Phrase(item.cantidad_de_pc.ToString(), chico))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = fondo
				});
			}

			pdf.Add(tabla);

			// ============================
			// TOTALES FINALES
			// ============================
			int totalOrdenes = registros.Count;
			int totalPedidos = registros.Sum(x => x.cantidad_de_pc);

			PdfPTable tablaTotales = new PdfPTable(new float[] { 70f, 30f });
			tablaTotales.WidthPercentage = 100;
			tablaTotales.SpacingBefore = 10f;

			PdfPCell celda1 = new PdfPCell(new Phrase($"Total Órdenes: {totalOrdenes}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = new BaseColor(230, 230, 230),
				Padding = 5f
			};

			PdfPCell celda2 = new PdfPCell(new Phrase($"Total Pedidos: {totalPedidos}", normalBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				BackgroundColor = new BaseColor(230, 230, 230),
				Padding = 5f
			};

			tablaTotales.AddCell(celda1);
			tablaTotales.AddCell(celda2);

			pdf.Add(tablaTotales);
		}


		#endregion

		private List<OrdenDeRepartoListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtitulo, out string filtrosString)
		{
			try
			{
				var ret = new List<OrdenDeRepartoListaDto>();
				var desde = solicitud.Parametros.GetValueOrDefault("desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("hasta", "").ToDateTime();
				var ore_list = solicitud.Parametros.GetValueOrDefault("ore_list", "")?.ToString() ?? null;
				var rp_list = solicitud.Parametros.GetValueOrDefault("rp_list", "")?.ToString() ?? null;
				filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;

				ret = _apiOdeR.ObtenerListaOrdenDeReparto(new OrdenDeRepartoRequest()
				{
					rp_list = rp_list,
					ore_list = ore_list,
					Desde = desde,
					Hasta = hasta,
					Registros = 999999999,
					Pagina = 1
				});
				titulo = $"Reporte de Ordenes de reparto";
				subtitulo = $"{filtrosString}";
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtitulo = "";
				filtrosString = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			string filtrosString;
			List<OrdenDeRepartoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
			string tipoReporte;
			string filtrosString;
			List<OrdenDeRepartoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out filtrosString);

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
