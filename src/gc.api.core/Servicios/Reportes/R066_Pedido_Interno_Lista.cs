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
	public class R066_Pedido_Interno_Lista : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiProductoServicio _apiProdSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R066_Pedido_Interno_Lista(IUnitOfWork uow, IApiProductoServicio apiProdSv,
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
				List<PedidoInternoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoPedidosInternos(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<PedidoInternoListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var fecha_d = solicitud.Parametros.GetValueOrDefault("FechaD", "").ToDateTime();
				var fecha_h = solicitud.Parametros.GetValueOrDefault("FechaH", "").ToDateTime();
				var adm_list = solicitud.Parametros.GetValueOrDefault("Rel01", "").ToString() ?? null;
				var estado_list = solicitud.Parametros.GetValueOrDefault("Rel02", "").ToString() ?? null;
				var adm_id = solicitud.Parametros.GetValueOrDefault("AdmId", "")?.ToString() ?? "0000";
				var usu_id = solicitud.Parametros.GetValueOrDefault("UsuId", "")?.ToString() ?? "";
				var filtrosString = solicitud.Parametros.GetValueOrDefault("filtrosString", "")?.ToString() ?? null;

				var request = new PedidoInternoRequest()
				{
					fecha_d= fecha_d,
					fecha_h= fecha_h,
					adm_list = GetString(adm_list),
					estado_list = GetString(estado_list),
					adm_id = adm_id,
					usu_id = usu_id,
					Registros = 999999999,
					Pagina = 1
				};
				var listaTemp = _apiProdSv.PedidosInternosLista(request);
				var item = listaTemp.First();
				titulo = $"Pedidos Internos";
				subtit = $"Desde: {fecha_d:dd/MM/yyyy} Hasta: {fecha_h:dd/MM/yyyy}\n{filtrosString}";
				return listaTemp;
			}
			catch (Exception)
			{
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
			List<PedidoInternoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<PedidoInternoListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoPedidosInternos(Document pdf, List<PedidoInternoListaDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
			{
				pdf.Add(new Paragraph("No hay pedidos internos para mostrar.", normal));
				return;
			}

			// ============================
			// TABLA
			// ============================
			PdfPTable tabla = new PdfPTable(6);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 15f, 12f, 20f, 20f, 15f, 18f });

			// Encabezados
			AgregarCeldaHeader(tabla, "PI N°", normalBold);
			AgregarCeldaHeader(tabla, "Fecha", normalBold);
			AgregarCeldaHeader(tabla, "Sucursal Generó", normalBold);
			AgregarCeldaHeader(tabla, "Sucursal Entrega", normalBold);
			AgregarCeldaHeader(tabla, "Estado", normalBold);
			AgregarCeldaHeader(tabla, "Usuario", normalBold);

			// Filas
			foreach (var item in registros)
			{
				AgregarCelda(tabla, item.pi_compte, normal, Element.ALIGN_CENTER);
				AgregarCelda(tabla, item.pi_fecha.ToString("dd/MM/yy"), normal, Element.ALIGN_CENTER);
				AgregarCelda(tabla, item.adm_id_gen_nombre, normal, Element.ALIGN_LEFT);
				AgregarCelda(tabla, item.adm_id_des_nombre, normal, Element.ALIGN_LEFT);
				AgregarCelda(tabla, item.pie_desc, normal, Element.ALIGN_LEFT);
				AgregarCelda(tabla, item.usu_apellidoynombre, normal, Element.ALIGN_LEFT);
			}

			pdf.Add(tabla);
		}

		private static void AgregarCelda(PdfPTable tabla, string texto, Font fuente, int Align = 0, bool esEncabezado = false, BaseColor? fondo = null)
		{
			var celda = new PdfPCell(new Phrase(texto, fuente))
			{
				HorizontalAlignment = Align,
				Padding = 4f
			};
			if (esEncabezado && fondo != null)
				celda.BackgroundColor = fondo;

			tabla.AddCell(celda);
		}

		private static void AgregarCeldaHeader(PdfPTable tabla, string texto, Font font)
		{
			PdfPCell celda = new PdfPCell(new Phrase(texto, font));
			celda.HorizontalAlignment = Element.ALIGN_CENTER;
			celda.VerticalAlignment = Element.ALIGN_MIDDLE;
			celda.BackgroundColor = new BaseColor(230, 230, 230);
			celda.Padding = 4f;
			tabla.AddCell(celda);
		}

		#endregion
	}
}
