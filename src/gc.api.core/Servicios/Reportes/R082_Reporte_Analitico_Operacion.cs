using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
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

namespace gc.api.core.Servicios.Reportes
{
	public class R082_Reporte_Analitico_Operacion : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R082_Reporte_Analitico_Operacion(IUnitOfWork uow, IApiVentasServicio ventasSv,
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
				string caja_nro_proceso;
				int caja_nro_cierre;
				List<RepoVtaAnaliticoOperacionesDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out caja_nro_proceso, out caja_nro_cierre);

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
				CargarRepoVtaDatosDeCierre(pdf, registros, caja_nro_proceso, caja_nro_cierre, chico, normal, normalBold, titulo, tituloBig);
				#endregion
				#region Reporte 
				CargarRepoVta_Analitico_Operaciones(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
		public static void CargarRepoVtaDatosDeCierre(Document pdf, List<RepoVtaAnaliticoOperacionesDto> registros, string caja_nro_proceso, int caja_nro_cierre, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			PdfPCell Celda(Phrase frase, int align = Element.ALIGN_LEFT)
			{
				return new PdfPCell(frase)
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = align,
					PaddingTop = 2f,
					PaddingBottom = 2f
				};
			}

			var r = registros.First();

			// ============================
			// TABLA IZQUIERDA (Parte, Cierre)
			// ============================
			PdfPTable tablaIzq = new PdfPTable(1);
			tablaIzq.WidthPercentage = 45;
			tablaIzq.HorizontalAlignment = Element.ALIGN_LEFT;

			tablaIzq.AddCell(Celda(new Phrase()
			{
				new Chunk("Parte N° ", normal),
				new Chunk(caja_nro_proceso, normalBold)
			}));

			tablaIzq.AddCell(Celda(new Phrase()
			{
				new Chunk("Cierre N° ", normal),
				new Chunk(caja_nro_cierre.ToString(), normalBold)
			}));


			// ============================
			// TABLA DERECHA (Cajero, Apertura, Cierre)
			// ============================
			PdfPTable tablaDer = new PdfPTable(1);
			tablaDer.WidthPercentage = 45;
			tablaDer.HorizontalAlignment = Element.ALIGN_RIGHT;

			tablaDer.AddCell(Celda(new Phrase()
			{
				new Chunk("Cajero ", normal),
				new Chunk(r.usu_nombre, normalBold)
			}, Element.ALIGN_RIGHT));

			tablaDer.AddCell(Celda(new Phrase()
			{
				new Chunk("Apertura Caja ", normal),
				new Chunk(r.caja_apertura.ToString("dd/MM/yyyy HH:mm"), normalBold)
			}, Element.ALIGN_RIGHT));

			tablaDer.AddCell(Celda(new Phrase()
			{
				new Chunk("Cierre Caja ", normal),
				new Chunk(r.caja_cierre?.ToString("dd/MM/yyyy HH:mm"), normalBold)
			}, Element.ALIGN_RIGHT));

			// ============================
			// TABLA CONTENEDORA (2 columnas)
			// ============================
			PdfPTable tablaCont = new(2)
			{
				WidthPercentage = 100
			};
			tablaCont.SetWidths([50f, 50f]);

			PdfPCell celdaIzq = new(tablaIzq)
			{
				Border = Rectangle.NO_BORDER,
				Padding = 0
			};

			PdfPCell celdaDer = new(tablaDer)
			{
				Border = Rectangle.NO_BORDER,
				Padding = 0
			};

			tablaCont.AddCell(celdaIzq);
			tablaCont.AddCell(celdaDer);

			// Agregar al PDF
			pdf.Add(tablaCont);

			// ============================
			// LÍNEA HORIZONTAL NEGRA FULL WIDTH
			// ============================
			var linea = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);
			pdf.Add(new Chunk(linea));

			// Espacio inferior
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_Analitico_Operaciones(Document pdf, List<RepoVtaAnaliticoOperacionesDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var grupos = registros
				.GroupBy(x => x.caja_nro_ope)
				.OrderBy(g => g.Key);

			float[] widths = new float[] {
				40f, 50f, 180f, 120f, 110f, 70f, 70f,
				15f,
				180f, 70f
			};

			PdfPTable tabla = new(widths)
			{
				WidthPercentage = 100,
				SpacingBefore = 10f
			};

			// === AGRUPADORES ===

			PdfPCell grpOp = new(new Phrase("N° Op", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray,
				Padding = 4
			};
			tabla.AddCell(grpOp);

			PdfPCell grpHora = new(new Phrase("Hora", normalBold))
			{
				Rowspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = BaseColor.LightGray,
				Padding = 4
			};
			tabla.AddCell(grpHora);

			PdfPCell grp1 = new(new Phrase("Detalle de Operaciones", normalBold))
			{
				Colspan = 5,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = BaseColor.LightGray,
				Padding = 4
			};
			tabla.AddCell(grp1);

			PdfPCell separador = new(new Phrase(""))
			{
				Colspan = 1,
				BackgroundColor = BaseColor.White,
				Border = Rectangle.NO_BORDER
			};
			tabla.AddCell(separador);

			PdfPCell grp2 = new(new Phrase("Detalle de Valores", normalBold))
			{
				Colspan = 2,
				HorizontalAlignment = Element.ALIGN_CENTER,
				BackgroundColor = BaseColor.LightGray,
				Padding = 4
			};
			tabla.AddCell(grp2);

			// === CABECERAS ===
			void AddHeader(string texto, int align = Element.ALIGN_LEFT)
			{
				PdfPCell c = new(new Phrase(texto, normalBold))
				{
					HorizontalAlignment = align,
					BackgroundColor = BaseColor.LightGray,
					Padding = 3
				};
				tabla.AddCell(c);
			}

			AddHeader("Cliente", Element.ALIGN_CENTER);
			AddHeader("Descripción", Element.ALIGN_CENTER);
			AddHeader("Comprobante", Element.ALIGN_CENTER);
			AddHeader("Importe", Element.ALIGN_CENTER);
			AddHeader("A rendir", Element.ALIGN_CENTER);

			PdfPCell sep2 = new(new Phrase(""))
			{
				BackgroundColor = BaseColor.White,
				Border = Rectangle.NO_BORDER
			};
			tabla.AddCell(sep2);

			AddHeader("Descripción");
			AddHeader("Importe", Element.ALIGN_RIGHT);

			// === AUXILIARES ===
			string FormatMonto(decimal? v)
			{
				if (v == null) return "";
				decimal val = v.Value;
				if (val < 0)
					return $"({Math.Abs(val):N2})";
				return $"{val:N2}";
			}

			string Hora(DateTime? f)
			{
				if (f == null) return "";
				return f.Value.ToString("HH:mm");
			}

			// === CUERPO ===
			int? ultimoOp = null;

			foreach (var grupo in grupos)
			{
				var filas = grupo.OrderBy(x => x.registro).ToList();
				ultimoOp = null;

				for (int i = 0; i < filas.Count; i++)
				{
					var r = filas[i];
					bool esPrimeraFilaGrupo = (ultimoOp != r.caja_nro_ope);
					bool esUltimaFilaGrupo = (i == filas.Count - 1);
					ultimoOp = r.caja_nro_ope;

					string opText = esPrimeraFilaGrupo ? r.caja_nro_ope?.ToString() ?? "" : "";
					string horaText = esPrimeraFilaGrupo ? Hora(r.co_fecha) : "";
					string clienteText = esPrimeraFilaGrupo ? $"{r.nombre} ({r.cta_id})" : "";
					string descText = esPrimeraFilaGrupo ? r.co_tipo_desc : "";

					int bordeComun = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER;

					if (esUltimaFilaGrupo)
						bordeComun |= Rectangle.BOTTOM_BORDER;

					if (esPrimeraFilaGrupo)
						bordeComun |= Rectangle.TOP_BORDER;

					PdfPCell c1 = new(new Phrase(opText, normal))
					{
						HorizontalAlignment = Element.ALIGN_CENTER,
						Border = bordeComun
					};
					tabla.AddCell(c1);

					PdfPCell c2 = new(new Phrase(horaText, normal))
					{
						HorizontalAlignment = Element.ALIGN_CENTER,
						Border = bordeComun
					};
					tabla.AddCell(c2);

					PdfPCell c3 = new(new Phrase(clienteText, normal))
					{
						HorizontalAlignment = Element.ALIGN_LEFT,
						Border = bordeComun
					};
					tabla.AddCell(c3);

					PdfPCell c4 = new(new Phrase(descText, normal))
					{
						HorizontalAlignment = Element.ALIGN_LEFT,
						Border = bordeComun
					};
					tabla.AddCell(c4);

					PdfPCell c5 = new(new Phrase(r.cm_compte, normal))
					{
						HorizontalAlignment = Element.ALIGN_CENTER,
						Border = bordeComun
					};
					tabla.AddCell(c5);

					PdfPCell c6 = new(new Phrase(FormatMonto(r.importe), normal))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Border = bordeComun
					};
					tabla.AddCell(c6);

					PdfPCell c7 = new(new Phrase(FormatMonto(r.a_rendir), normal))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Border = bordeComun
					};
					tabla.AddCell(c7);

					PdfPCell csep = new(new Phrase(""))
					{
						Border = Rectangle.NO_BORDER
					};
					tabla.AddCell(csep);

					PdfPCell c8 = new(new Phrase(r.concepto, normal))
					{
						HorizontalAlignment = Element.ALIGN_LEFT,
						Border = bordeComun
					};
					tabla.AddCell(c8);

					PdfPCell c9 = new(new Phrase(FormatMonto(r.valor), normal))
					{
						HorizontalAlignment = Element.ALIGN_RIGHT,
						Border = bordeComun
					};
					tabla.AddCell(c9);
				}
			}

			pdf.Add(tabla);
		}

		#endregion

		private List<RepoVtaAnaliticoOperacionesDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit, out string cajaNroProceso, out int cajaNroCierre)
		{
			try
			{
				var ret = new List<RepoVtaAnaliticoOperacionesDto>();
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var sucursales = solicitud.Parametros.GetValueOrDefault("suc", "")?.ToString() ?? null;
				titulo = $"Resumen Analítico de Operaciones";
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				ret = _ventasSv.ObtenerRepoVtaAnaliticoOperaciones(request);
				subtit = $"Sucursal {sucursales}\nPunto de Venta (Caja): {ret.First().caja_id}";
				cajaNroProceso = caja_nro_proceso;
				cajaNroCierre = caja_nro_cierre;
				return ret;
			}
			catch (Exception)
			{
				titulo = "";
				subtit = "";
				cajaNroProceso = "";
				cajaNroCierre = 0;
				return [];
			}

		}

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			string cajaNroProceso;
			int cajaNroCierre;
			List<RepoVtaAnaliticoOperacionesDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out cajaNroProceso, out cajaNroCierre);

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
			string cajaNroProceso;
			int cajaNroCierre;
			List<RepoVtaAnaliticoOperacionesDto> registros = ObtenerDatos(solicitud, out tit, out subtit, out cajaNroProceso, out cajaNroCierre);

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
