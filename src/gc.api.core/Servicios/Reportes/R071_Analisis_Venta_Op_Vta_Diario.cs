using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R071_Analisis_Venta_Op_Vta_Diario : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R071_Analisis_Venta_Op_Vta_Diario(IUnitOfWork uow, IApiVentasServicio ventasSv,
											IOptions<EmpresaGeco> empresa, ICuentaServicio consultaSv, ILogger logger) : base(uow)
		{
			_empresaGeco = empresa.Value;
			_titulos = ["N° OP", "Tipo", "Fecha", "Proveedor", "Anulada", "Usuario", "Importe"];
			_campos = ["op_compte", "opt_desc", "op_fecha", "cta_denominacion", "op_anulada_desc", "usu_apellidoynombre", "op_importe"];
			_logger = logger;
			_ventasSv = ventasSv;
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
				List<AnaVtaMesDetalleHoraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subtit;

				//hago el modelo de dato aca ya que necesito los datos de la cuenta
				var regs = registros.Select(x => new
				{

				}).ToList();

				#endregion
				#region Scripts PDF
				#region instanciamos el pdf
				pdf = HelperPdf.GenerarInstanciaAndInit(ref writer, out ms, HojaSize.A4, false);

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
				CargarRepoAnalisisDeVentaHora(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<AnaVtaMesDetalleHoraDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var sucursales = solicitud.Parametros.GetValueOrDefault("Sucursales", "")?.ToString() ?? null;
				var sucursalesTextos = solicitud.Parametros.GetValueOrDefault("SucursalesTextos", "")?.ToString() ?? null;
				var mes = solicitud.Parametros.GetValueOrDefault("Mes", "")?.ToString() ?? null;
				var periodo = solicitud.Parametros.GetValueOrDefault("Periodo", "")?.ToString() ?? null;
				var (desde, hasta) = ObtenerRangoMes(Convert.ToInt32(periodo), Convert.ToInt32(mes));
				var request = new AnaVtaMesRequest()
				{
					adm_list = sucursales,
					desde = desde,
					hasta = hasta
				};
				var listaTemp = _ventasSv.ObtenerAnaVtaMesDetalleHoraLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis Operativo de Venta Diario del Mes {mes.PadLeft(2, '0')}-{periodo}";
				subtit = $"Sucursales: {sucursalesTextos}";
				return listaTemp;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				return [];
			}

		}

		public static (DateTime Desde, DateTime Hasta) ObtenerRangoMes(int periodo, int mes)
		{
			var desde = new DateTime(periodo, mes, 1);
			var hasta = desde.AddMonths(1).AddDays(-1);

			return (desde, hasta);
		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<AnaVtaMesDetalleHoraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<AnaVtaMesDetalleHoraDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoAnalisisDeVentaHora(Document pdf, List<AnaVtaMesDetalleHoraDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			// Página apaisada
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			// ============================
			// ENCABEZADO REPETIBLE
			// ============================
			PdfPTable header = new PdfPTable(1);
			header.WidthPercentage = 100;

			PdfPCell tituloCell = new PdfPCell(new Phrase("Análisis de Venta por Hora", tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 5
			};
			header.AddCell(tituloCell);

			PdfPCell subCell = new PdfPCell(new Phrase("Detalle por franja horaria", titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				PaddingBottom = 10
			};
			header.AddCell(subCell);

			header.HeaderRows = 2;
			pdf.Add(header);

			// ============================
			// TABLA PRINCIPAL
			// ============================
			PdfPTable tabla = new PdfPTable(16)
			{
				WidthPercentage = 100
			};

			tabla.SetWidths(new float[] {
				1.6f, // Día
				1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,
				1.4f,1.4f,1.4f,1.4f,1.4f,1.4f,1.4f // 6-8 ... 21-22
			});

			// ============================
			// ENCABEZADOS
			// ============================
			void AddHeader(string texto)
			{
				PdfPCell c = new PdfPCell(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Día");
			AddHeader("6 a 8");
			AddHeader("8 a 9");
			AddHeader("9 a 10");
			AddHeader("10 a 11");
			AddHeader("11 a 12");
			AddHeader("12 a 13");
			AddHeader("13 a 14");
			AddHeader("14 a 15");
			AddHeader("15 a 16");
			AddHeader("16 a 17");
			AddHeader("17 a 18");
			AddHeader("18 a 19");
			AddHeader("19 a 20");
			AddHeader("20 a 21");
			AddHeader("21 a 22");

			// ============================
			// FUNCIÓN MINI‑CELDA (FA / OP)
			// ============================
			PdfPCell CeldaDual(decimal fa, decimal op)
			{
				PdfPTable mini = new PdfPTable(1);
				mini.WidthPercentage = 100;

				PdfPCell c1 = new PdfPCell(new Phrase(fa.ToString("N2"), chico))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 2,
					Border = Rectangle.NO_BORDER
				};

				PdfPCell c2 = new PdfPCell(new Phrase(op.ToString("N0"), chico))
				{
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 2,
					Border = Rectangle.NO_BORDER
				};

				// Línea divisoria entre FA y OP
				c1.BorderWidthBottom = 0.5f;
				c1.BorderColorBottom = new BaseColor(200, 200, 200);

				mini.AddCell(c1);
				mini.AddCell(c2);

				return new PdfPCell(mini)
				{
					Padding = 0
				};
			}

			// ============================
			// FILAS
			// ============================
			foreach (var r in registros)
			{
				// Día
				tabla.AddCell(new PdfPCell(new Phrase(r.dia.ToString("dd/MM/yyyy"), normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// 6 a 8
				tabla.AddCell(CeldaDual(r.fa_6a8, r.op_6a8));
				tabla.AddCell(CeldaDual(r.fa_8a9, r.op_8a9));
				tabla.AddCell(CeldaDual(r.fa_9a10, r.op_9a10));
				tabla.AddCell(CeldaDual(r.fa_10a11, r.op_10a11));
				tabla.AddCell(CeldaDual(r.fa_11a12, r.op_11a12));
				tabla.AddCell(CeldaDual(r.fa_12a13, r.op_12a13));
				tabla.AddCell(CeldaDual(r.fa_13a14, r.op_13a14));
				tabla.AddCell(CeldaDual(r.fa_14a15, r.op_14a15));
				tabla.AddCell(CeldaDual(r.fa_15a16, r.op_15a16));
				tabla.AddCell(CeldaDual(r.fa_16a17, r.op_16a17));
				tabla.AddCell(CeldaDual(r.fa_17a18, r.op_17a18));
				tabla.AddCell(CeldaDual(r.fa_18a19, r.op_18a19));
				tabla.AddCell(CeldaDual(r.fa_19a20, r.op_19a20));
				tabla.AddCell(CeldaDual(r.fa_20a21, r.op_20a21));
				tabla.AddCell(CeldaDual(r.fa_21a22, r.op_21a22));
			}

			pdf.Add(tabla);
		}
		#endregion
	}
}
