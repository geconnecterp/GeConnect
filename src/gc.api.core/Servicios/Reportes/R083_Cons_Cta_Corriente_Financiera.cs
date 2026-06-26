using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace gc.api.core.Servicios.Reportes
{
	public class R083_Cons_Cta_Corriente_Financiera : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IFinancieroServicio _financieroServicio;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R083_Cons_Cta_Corriente_Financiera(IUnitOfWork uow, IFinancieroServicio financieroServicio,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_financieroServicio = financieroServicio;
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
				string cuenta;
				List<FinancieroBcoCtaCteDto> registros = ObtenerDatos(solicitud, out tit, out cuenta);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = "";

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
				#region Datos del Cierre
				CargarRepoDatosDeTitulo(pdf, registros, cuenta, chico, normal, normalBold, titulo, tituloBig);
				#endregion
				#region Reporte 
				CargarRepoTablaMovimientos(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
				#endregion
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
		public static void CargarRepoDatosDeTitulo(Document pdf, List<FinancieroBcoCtaCteDto> registros, string cuenta, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Total del saldo
			decimal totalSaldo = registros.Sum(x => x.cf_saldo);

			// Formato monetario
			string saldoFormateado = totalSaldo.ToString("#,##0.00", new CultureInfo("es-AR"));

			// Texto derecha
			string txtSaldo = "Saldo registrado: " + saldoFormateado;

			// Fuente subrayada + negrita SOLO para el valor de la cuenta
			Font fontCuentaValor = new Font(normalBold);
			fontCuentaValor.SetStyle(Font.BOLD | Font.UNDERLINE);

			// Construcción del texto izquierdo con estilos separados
			Phrase fraseCuenta = new Phrase();
			fraseCuenta.Add(new Chunk("Cuenta: ", normal));                 // sin negrita ni subrayado
			fraseCuenta.Add(new Chunk(cuenta.ToUpper(), fontCuentaValor));  // negrita + subrayado

			// Construcción del texto izquierdo con estilos separados
			Phrase fraseSaldo = new Phrase();
			fraseSaldo.Add(new Chunk("Saldo registrado: ", normal));                 // sin negrita ni subrayado
			fraseSaldo.Add(new Chunk(saldoFormateado, normalBold));  // negrita 

			// Tabla contenedora
			PdfPTable tbl = new PdfPTable(2);
			tbl.WidthPercentage = 100;
			tbl.SetWidths(new float[] { 60f, 40f });

			// Celda izquierda
			PdfPCell celCuenta = new PdfPCell(fraseCuenta);
			celCuenta.Border = Rectangle.NO_BORDER;
			celCuenta.HorizontalAlignment = Element.ALIGN_LEFT;
			celCuenta.VerticalAlignment = Element.ALIGN_MIDDLE;

			// Celda derecha
			PdfPCell celSaldo = new PdfPCell(fraseSaldo);
			celSaldo.Border = Rectangle.NO_BORDER;
			celSaldo.HorizontalAlignment = Element.ALIGN_RIGHT;
			celSaldo.VerticalAlignment = Element.ALIGN_MIDDLE;

			// Agregar celdas
			tbl.AddCell(celCuenta);
			tbl.AddCell(celSaldo);

			// Agregar al PDF
			pdf.Add(tbl);

			// Espacio inferior
			pdf.Add(new Paragraph(" ", chico));
		}


		public static void CargarRepoTablaMovimientos(Document pdf, List<FinancieroBcoCtaCteDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Tabla con 6 columnas
			PdfPTable tbl = new PdfPTable(6);
			tbl.WidthPercentage = 100;
			tbl.SetWidths(new float[] { 11f, 11f, 45f, 11f, 11f, 11f });

			// Encabezados
			AgregarHeader(tbl, "Fecha", normalBold, Element.ALIGN_CENTER);
			AgregarHeader(tbl, "Movimiento N°", normalBold, Element.ALIGN_CENTER);
			AgregarHeader(tbl, "Concepto", normalBold, Element.ALIGN_LEFT);
			AgregarHeader(tbl, "Debe", normalBold, Element.ALIGN_RIGHT);
			AgregarHeader(tbl, "Haber", normalBold, Element.ALIGN_RIGHT);
			AgregarHeader(tbl, "Saldo", normalBold, Element.ALIGN_RIGHT);

			// Filas
			foreach (var r in registros)
			{
				// Fecha
				AgregarCelda(tbl, r.cf_fecha.ToString("dd/MM/yyyy"), normal, Element.ALIGN_CENTER);

				// Movimiento N°
				AgregarCelda(tbl, r.dia_movi, normal, Element.ALIGN_CENTER);

				// Concepto
				AgregarCelda(tbl, r.cf_concepto, normal, Element.ALIGN_LEFT);

				// Debe
				AgregarCelda(tbl, FormatoMoneda(r.cf_debe), normal, Element.ALIGN_RIGHT);

				// Haber
				AgregarCelda(tbl, FormatoMoneda(r.cf_haber), normal, Element.ALIGN_RIGHT);

				// Saldo
				AgregarCelda(tbl, FormatoMoneda(r.cf_saldo), normal, Element.ALIGN_RIGHT);
			}

			pdf.Add(tbl);
			pdf.Add(new Paragraph(" ", chico)); // Espacio inferior
		}


		#endregion
		private static string FormatoMoneda(decimal valor)
		{
			return valor.ToString("#,##0.00", new CultureInfo("es-AR"));
		}

		private static void AgregarHeader(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell cel = new PdfPCell(new Phrase(texto, font));
			cel.HorizontalAlignment = align;
			cel.VerticalAlignment = Element.ALIGN_MIDDLE;
			cel.BackgroundColor = new BaseColor(230, 230, 230);
			cel.BorderWidth = 0.75f;
			tbl.AddCell(cel);
		}

		private static void AgregarCelda(PdfPTable tbl, string texto, Font font, int align)
		{
			PdfPCell cel = new PdfPCell(new Phrase(texto, font));
			cel.HorizontalAlignment = align;
			cel.VerticalAlignment = Element.ALIGN_MIDDLE;
			cel.BorderWidth = 0.5f;
			tbl.AddCell(cel);
		}

		private List<FinancieroBcoCtaCteDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string cuenta)
		{
			try
			{
				var ret = new List<FinancieroBcoCtaCteDto>();
				var ctaf_id = solicitud.Parametros.GetValueOrDefault("ctaf_id", "")?.ToString() ?? null;
				var ctaf_desc = solicitud.Parametros.GetValueOrDefault("ctaf_desc", "")?.ToString() ?? null;
				var tipo_filtro = solicitud.Parametros.GetValueOrDefault("tipo_filtro", "")?.ToString() ?? null;
				var ct_tipo = solicitud.Parametros.GetValueOrDefault("ct_tipo", "")?.ToString() ?? null;
				var desde = solicitud.Parametros.GetValueOrDefault("FechaDesde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("FechaHasta", "").ToDateTime();
				titulo = $"Consulta de Cuenta Corriente Financiera";
				cuenta = $"{ctaf_desc}";
				var request = new FinancieroBcoCtaCteRequest()
				{
					ctaf_id = ctaf_id,
					tipo_filtro = tipo_filtro,
					ct_tipo = ct_tipo,
					FechaDesde = desde,
					FechaHasta = hasta
				};
				ret = _financieroServicio.GetFinancieroBcoCtaCte(request);
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				cuenta = "";
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string cuenta;
			List<FinancieroBcoCtaCteDto> registros = ObtenerDatos(solicitud, out tit, out cuenta);

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
			string cuenta;
			List<FinancieroBcoCtaCteDto> registros = ObtenerDatos(solicitud, out tit, out cuenta);

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
