using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.api.core.Servicios.Reportes
{
	public class R098_Reporte_Prod_Sin_Stock_En_Transferencias : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R098_Reporte_Prod_Sin_Stock_En_Transferencias(IUnitOfWork uow, IApiProductoServicio apiProdSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_apiProdSv = apiProdSv;
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
				List<TRNuevaAutDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoStockDeProdSinStockEnTransf(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private static bool GetBoolParam(IDictionary<string, string> parametros, string clave, bool valorPorDefecto = false)
		{
			if (parametros == null || !parametros.TryGetValue(clave, out var valor) || string.IsNullOrWhiteSpace(valor))
				return valorPorDefecto;

			return bool.TryParse(valor, out var resultado) ? resultado : valorPorDefecto;
		}

		private List<TRNuevaAutDetalleDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var tabla = solicitud.Parametros.GetValueOrDefault("tabla", "").ToString() ?? null;

				titulo = $"Reporte de Productos Sin Stock en Transferencias";
				subtit = "";

				if (string.IsNullOrWhiteSpace(tabla))
					return [];

				// Deserializar el JSON a la lista de DTO
				var lista = JsonConvert.DeserializeObject<List<TRNuevaAutDetalleDto>>(tabla);

				return lista ?? [];
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al deserializar datos de productos sin stock.");
				titulo = "";
				subtit = "";
				return [];
			}

		}

		private string GetString(string json)
		{
			if (string.IsNullOrEmpty(json))
				return string.Empty;
			List<string> lista = JsonConvert.DeserializeObject<List<string>>(json);
			return string.Join(",", lista);
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<TRNuevaAutDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<TRNuevaAutDetalleDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoStockDeProdSinStockEnTransf(Document pdf, List<TRNuevaAutDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Agrupar por adm_id
			var grupos = registros
				.GroupBy(r => new { r.adm_id, r.adm_nombre })
				.OrderBy(g => g.Key.adm_id);

			foreach (var grupo in grupos)
			{
				// ================================
				// TABLA
				// ================================
				PdfPTable tabla = new PdfPTable(6)
				{
					WidthPercentage = 100
				};

				tabla.SetWidths(new float[] { 10f, 35f, 15f, 15f, 12f, 13f });

				// ================================
				// FILA AGRUPADORA (fila 0)
				// ================================
				PdfPCell celdaGrupo = new PdfPCell(
					new Phrase($"({grupo.Key.adm_id}) {grupo.Key.adm_nombre}", normalBold))
				{
					Colspan = 6,
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 5
				};
				tabla.AddCell(celdaGrupo);

				// ================================
				// FILA HEADER (fila 1)
				// ================================
				string[] headers = { "ID", "Descripción", "ID Prov.", "Comprobante", "Stock", "Pedido" };

				foreach (var h in headers)
				{
					PdfPCell cell = new PdfPCell(new Phrase(h, normalBold))
					{
						HorizontalAlignment = Element.ALIGN_CENTER,
						BackgroundColor = BaseColor.LightGray,
						Padding = 4
					};
					tabla.AddCell(cell);
				}

				// Indicar que las primeras 2 filas se repiten en cada página
				tabla.HeaderRows = 2;

				// ================================
				// FILAS DE PRODUCTOS
				// ================================
				foreach (var item in grupo)
				{
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id, chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_desc, chico)) { HorizontalAlignment = Element.ALIGN_LEFT });
					tabla.AddCell(new PdfPCell(new Phrase(item.p_id_prov, chico)) { HorizontalAlignment = Element.ALIGN_CENTER });
					tabla.AddCell(new PdfPCell(new Phrase(item.pi_compte, chico)) { HorizontalAlignment = Element.ALIGN_CENTER });

					tabla.AddCell(new PdfPCell(new Phrase(@GridHelper.FormatearDato(item.stk, GridHelper.FormatDato.Monto, item.PermiteDecimales), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
					tabla.AddCell(new PdfPCell(new Phrase(@GridHelper.FormatearDato(item.pedido, GridHelper.FormatDato.Monto, item.PermiteDecimales), chico)) { HorizontalAlignment = Element.ALIGN_RIGHT });
				}

				pdf.Add(tabla);

				// Espacio entre grupos
				pdf.Add(new Paragraph(" ", normal));
			}
		}
		
		#endregion
	}
}
