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
	public class R069_Analisis_Venta_Mensual : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R069_Analisis_Venta_Mensual(IUnitOfWork uow, IApiVentasServicio ventasSv,
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
				List<AnaVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
				CargarRepoAnalisisDeVentaMensual(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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

		private List<AnaVtaMesDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var sucursales = solicitud.Parametros.GetValueOrDefault("Sucursales", "")?.ToString() ?? null;
				var sucursalesTextos = solicitud.Parametros.GetValueOrDefault("SucursalesTextos", "")?.ToString() ?? null;
				var desde = solicitud.Parametros.GetValueOrDefault("Desde", "").ToDateTime();
				var hasta = solicitud.Parametros.GetValueOrDefault("Hasta", "").ToDateTime();
				var request = new AnaVtaMesRequest()
				{ 
					adm_list = sucursales,
					desde = desde,
					hasta = hasta
				};
				var listaTemp = _ventasSv.ObtenerAnaVtaMesLista(request);
				var item = listaTemp.First();
				titulo = $"Análisis de Venta Mensual desde el {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}";
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

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<AnaVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<AnaVtaMesDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
		public static void CargarRepoAnalisisDeVentaMensual(Document pdf, List<AnaVtaMesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			pdf.SetPageSize(PageSize.A4.Rotate());
			pdf.SetMargins(20f, 20f, 20f, 20f);

			PdfPTable tabla = new(9)
			{
				WidthPercentage = 100
			};
			tabla.SetWidths([
				1.2f, // Mes
				2.0f, // Facturación
				2.0f, // Fact. Ac.
				2.2f, // Dif. Mes Ant.
				2.2f, // Dif. Mes/Año Ant.
				2.0f, // Costo
				2.0f, // Rentabilidad
				2.0f, // Rent. Ac.
				2.2f  // Vta. CtaCte.
			]);

			void AddHeader(string texto)
			{
				PdfPCell c = new(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = Element.ALIGN_CENTER,
					BackgroundColor = new BaseColor(230, 230, 230),
					Padding = 4
				};
				tabla.AddCell(c);
			}

			AddHeader("Mes");
			AddHeader("Facturación");
			AddHeader("Fact. Ac.");
			AddHeader("Dif. Mes Ant.");
			AddHeader("Dif. Mes/Año Ant.");
			AddHeader("Costo");
			AddHeader("Rentabilidad");
			AddHeader("Rent. Ac.");
			AddHeader("Vta. CtaCte.");


			BaseColor ColorPorcentaje(decimal valor)
			{
				if (valor > 0) return new BaseColor(201, 228, 255); // celeste
				if (valor < 0) return new BaseColor(255, 224, 224); // rojo suave
				return BaseColor.White;
			}

			foreach (var r in registros)
			{
				// Mes
				tabla.AddCell(new PdfPCell(new Phrase($"{r.periodo}-{r.mes:00}", normal))
				{ HorizontalAlignment = Element.ALIGN_CENTER });

				// Facturación
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Facturación acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.co_facturacion_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Dif. Mes Ant. (porcentaje + valor)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.mes_ant_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.mes_ant_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.mes_ant_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}

				// Dif. Mes/Año Ant. (igual que arriba)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.per_ant_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.per_ant_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.per_ant_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}

				// Costo
				tabla.AddCell(new PdfPCell(new Phrase(r.co_costo.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Rentabilidad acumulada
				tabla.AddCell(new PdfPCell(new Phrase(r.rentabilidad_acu.ToString("N2"), normal))
				{ HorizontalAlignment = Element.ALIGN_RIGHT });

				// Vta. CtaCte. (porcentaje + valor)
				{
					PdfPTable mini = new PdfPTable(2);
					mini.WidthPercentage = 100;
					mini.SetWidths(new float[] { 1f, 1f });

					PdfPCell porc = new PdfPCell(new Phrase($"{r.ctacte_dif_porc}%", chico))
					{
						BackgroundColor = ColorPorcentaje(r.ctacte_dif_porc),
						HorizontalAlignment = Element.ALIGN_LEFT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					PdfPCell val = new PdfPCell(new Phrase(r.ctacte_dif.ToString("N2"), chico))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Padding = 2,
						Border = Rectangle.NO_BORDER
					};

					mini.AddCell(porc);
					mini.AddCell(val);

					PdfPCell cont = new PdfPCell(mini);
					tabla.AddCell(cont);
				}
			}

			pdf.Add(tabla);

		}
		#endregion
	}
}
