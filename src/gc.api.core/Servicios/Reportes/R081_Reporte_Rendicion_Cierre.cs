using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
	public class R081_Reporte_Rendicion_Cierre : Servicio<EntidadBase>, IGeneradorReporte
	{
		private readonly IApiVentasServicio _ventasSv;
		private readonly EmpresaGeco _empresaGeco;
		private readonly List<string> _titulos;
		private readonly List<string> _campos;
		private readonly ILogger _logger;

		public R081_Reporte_Rendicion_Cierre(IUnitOfWork uow, IApiVentasServicio ventasSv,
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

				#region Obtención de Datos
				List<CajaProcesoCierresListaDto> cierre = ObtenerDatos(solicitud, out tit, out subtit);
				List<RepoVtaResumenDto> registrosResumen = ObtenerDatosResumen(solicitud);
				List<RepoVtaRendicionDto> registrosRendicion = ObtenerDatosRendicion(solicitud);
				List<RepoVtaRendicionDetalleDto> registrosRendicionDetTarj = ObtenerDatosRendicionDetalleTarjetas(solicitud);
				List<RepoVtaRendicionDetalleDto> registrosRendicionDetCheq = ObtenerDatosRendicionDetalleCheques(solicitud);//Vamos descomentando a medida que vayamos armando el reporte
				List<RepoVtaRendicionDetalleDto> registrosRendicionDetTran = ObtenerDatosRendicionDetalleTransferencias(solicitud);
				List<RepoVtaRendicionDetalleDto> registrosRendicionDetOtros = ObtenerDatosRendicionDetalleOtros(solicitud);
				List<RepoVtaCtaCteDto> registrosCtaCte = ObtenerDatosCtaCte(solicitud);
				List<RepoVtaCobranzaDto> registrosCobranzas = ObtenerDatosCobranzas(solicitud);
				List<RepoVtaNCDto> registrosNC = ObtenerDatosNC(solicitud);
				List<RepoVtaCreditoUsadoDto> registrosCreditosUsados = ObtenerDatosCreditosUsados(solicitud);
				List<RepoVtaNDDto> registrosND = ObtenerDatosND(solicitud);
				List<RepoVtaZDto> registrosZ = ObtenerDatosZ(solicitud);
				//List<RepoVtaCambioValoresDto> registrosCambioValores = ObtenerDatosCambioValores(solicitud);
				//List<RepoVtaAnticipoDto> registrosAnticipos = ObtenerDatosAnticipos(solicitud);
				#endregion

				solicitud.Titulo = tit;
				solicitud.SubTitulo = subtit;

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

				PdfPTable tabla = GeneraCabeceraPDF2_NoFecha(solicitud, chico, titulo, tituloBig, logo, _empresaGeco, Element.ALIGN_LEFT, Element.ALIGN_LEFT);

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

				#region Armado de Repoorte 
				#region Datos del Cierre
				CargarRepoVtaDatosDeCierre(pdf, cierre, chico, normal, normalBold, titulo, tituloBig);
				#endregion

				#region #1 Resumen de Operaciones y Rendicion
				CargarRepoVta_SeccionNro1_A(pdf, registrosResumen, chico, normal, normalBold, titulo, tituloBig);
				CargarRepoVta_SeccionNro1_B(pdf, registrosResumen, chico, normal, normalBold, titulo, tituloBig);
				//TODO Marce: falta definir de donde saco Problemas de Secuencia:
				#endregion

				#region #2 Valores Rendidos por tipo de Medio de Pago
				CargarRepoVta_SeccionNro2_A(pdf, registrosRendicion, chico, normal, normalBold, titulo, tituloBig);
				if (registrosRendicionDetTarj != null && registrosRendicionDetTarj.Count > 0)
					CargarRepoVta_SeccionNro2_B(pdf, registrosRendicionDetTarj, chico, normal, normalBold, titulo, tituloBig);
				if (registrosRendicionDetCheq != null && registrosRendicionDetCheq.Count > 0)
					CargarRepoVta_SeccionNro2_C(pdf, registrosRendicionDetCheq, chico, normal, normalBold, titulo, tituloBig);
				if (registrosRendicionDetTran != null && registrosRendicionDetTran.Count > 0)
					CargarRepoVta_SeccionNro2_D(pdf, registrosRendicionDetTran, chico, normal, normalBold, titulo, tituloBig);
				if (registrosRendicionDetOtros != null && registrosRendicionDetOtros.Count > 0)
					CargarRepoVta_SeccionNro2_E(pdf, registrosRendicionDetOtros, chico, normal, normalBold, titulo, tituloBig);
				#endregion

				#region #3  Auditoria de Valores - Diferencias Rendiciones con Registros de Sistema (Pendiente de definicion)
				#endregion

				#region #4 Detalle de operaciones especiales
				if (registrosCtaCte != null && registrosCtaCte.Count > 0)
					CargarRepoVta_SeccionNro4_A(pdf, registrosCtaCte, chico, normal, normalBold, titulo, tituloBig);
				if (registrosCobranzas != null && registrosCobranzas.Count > 0)
					CargarRepoVta_SeccionNro4_B(pdf, registrosCobranzas, chico, normal, normalBold, titulo, tituloBig);
				if (registrosNC != null && registrosNC.Count > 0)
					CargarRepoVta_SeccionNro4_C(pdf, registrosNC, chico, chicoBold, normal, normalBold, titulo, tituloBig);
				if (registrosCreditosUsados != null && registrosCreditosUsados.Count > 0)
					CargarRepoVta_SeccionNro4_E(pdf, registrosCreditosUsados, chico, chicoBold, normal, normalBold, titulo, tituloBig);
				if (registrosND != null && registrosND.Count > 0)
					CargarRepoVta_SeccionNro4_D(pdf, registrosND, chico, normal, normalBold, titulo, tituloBig);
				#endregion
				//HelperPdf.CargarRepoSorteoAnalisisProdLista(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
				#endregion

				#region #5 Detalle de Valores Ingresados o Cambio de Efectivos

				#endregion

				#region #6  Información Fiscal
				if (registrosZ != null && registrosZ.Count > 0)
					CargarRepoVta_SeccionNro4_F(pdf, registrosZ, chico, chicoBold, normal, normalBold, titulo, tituloBig);
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
				throw new NegocioException("Se produjo un error al intentar generar el Reporte de Rendición de Cierre de Caja. Para mayores datos ver el log.");
			}
		}

		#region Funciones de generacion de secciones de reportes
		public static void CargarRepoVtaDatosDeCierre(Document pdf, List<CajaProcesoCierresListaDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
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
				new Chunk(r.caja_nro_proceso, normalBold)
			}));

			tablaIzq.AddCell(Celda(new Phrase()
			{
				new Chunk("Cierre N° ", normal),
				new Chunk(r.caja_nro_cierre.ToString(), normalBold)
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
				new Chunk(r.usu_apellidoynombre, normalBold)
			}, Element.ALIGN_RIGHT));

			tablaDer.AddCell(Celda(new Phrase()
			{
				new Chunk("Apertura Caja ", normal),
				new Chunk(r.caja_apertura.ToString("dd/MM/yyyy HH:mm"), normalBold)
			}, Element.ALIGN_RIGHT));

			tablaDer.AddCell(Celda(new Phrase()
			{
				new Chunk("Cierre Caja ", normal),
				new Chunk(r.caja_cierre.ToString("dd/MM/yyyy HH:mm"), normalBold)
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

		/// <summary>
		/// Sección #1 Resumen de Operaciones y Rendición
		/// Tabla: Ingresos / Egresos en Operaciones de Caja //  Rendiciones y  Fondos de Caja
		/// </summary>
		/// <param name="pdf">Componente de generación de reporte</param>
		/// <param name="registros">Lista para generar datos</param>
		/// <param name="chico">Tipo de fuente</param>
		/// <param name="normal">Tipo de fuente</param>
		/// <param name="normalBold">Tipo de fuente</param>
		/// <param name="titulo">Tipo de fuente</param>
		/// <param name="tituloBig">Tipo de fuente</param>
		public static void CargarRepoVta_SeccionNro1_A(Document pdf, List<RepoVtaResumenDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var r = registros.First();

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(Phrase frase, int align = Element.ALIGN_LEFT, bool lineaInferior = true)
			{
				return new PdfPCell(frase)
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					PaddingTop = 5f,
					PaddingBottom = 5f,
					PaddingLeft = 4f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string label, decimal valor)
			{
				string signo = valor >= 0 ? "+" : "-";
				decimal absValor = Math.Abs(valor);

				PdfPTable t = new PdfPTable(2);
				t.WidthPercentage = 100;
				t.SetWidths(new float[] { 20f, 80f });

				t.AddCell(new PdfPCell(new Phrase(signo, normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					Padding = 2f
				});

				t.AddCell(new PdfPCell(new Phrase(absValor.ToString("N2"), normalBold))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 2f
				});

				return new PdfPCell(new Phrase(label, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					Colspan = 1
				};
			}

			PdfPTable Fila(string label, decimal valor)
			{
				PdfPTable fila = new PdfPTable(2);
				fila.WidthPercentage = 100;
				fila.SetWidths(new float[] { 60f, 40f });

				fila.AddCell(Celda(new Phrase(label, normal), Element.ALIGN_RIGHT));

				PdfPTable valorTabla = new PdfPTable(2);
				valorTabla.WidthPercentage = 100;
				valorTabla.SetWidths(new float[] { 20f, 80f });

				string signo = valor >= 0 ? "+" : "-";
				decimal absValor = Math.Abs(valor);

				valorTabla.AddCell(Celda(new Phrase(signo, normalBold), Element.ALIGN_LEFT));
				valorTabla.AddCell(Celda(new Phrase(absValor.ToString("N2"), normalBold), Element.ALIGN_RIGHT));

				PdfPCell celdaValor = new PdfPCell(valorTabla)
				{
					Border = Rectangle.BOTTOM_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					Padding = 0
				};

				fila.AddCell(celdaValor);

				return fila;
			}

			PdfPTable TotalFila(string label, decimal valor)
			{
				PdfPTable fila = new PdfPTable(2);
				fila.WidthPercentage = 100;
				fila.SetWidths(new float[] { 60f, 40f });

				// Label alineado a la derecha
				fila.AddCell(new PdfPCell(new Phrase(label, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200), // gris más oscuro
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 5f
				});

				// Tabla interna para signo + valor
				PdfPTable valorTabla = new PdfPTable(2);
				valorTabla.WidthPercentage = 100;
				valorTabla.SetWidths(new float[] { 20f, 80f });

				string signo = valor >= 0 ? "+" : "-";
				decimal absValor = Math.Abs(valor);

				valorTabla.AddCell(new PdfPCell(new Phrase(signo, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_LEFT,
					Padding = 5f
				});

				valorTabla.AddCell(new PdfPCell(new Phrase(absValor.ToString("N2"), normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Padding = 5f
				});

				PdfPCell celdaValor = new PdfPCell(valorTabla)
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 0
				};

				fila.AddCell(celdaValor);

				return fila;
			}


			// ============================================================
			// ETIQUETA SUPERIOR
			// ============================================================

			pdf.Add(new Paragraph("1) Resumen de Operaciones y Rendición", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA IZQUIERDA
			// ============================================================

			PdfPTable tablaOp = new PdfPTable(1);
			tablaOp.WidthPercentage = 100;

			tablaOp.AddCell(TituloCelda("Ingresos / Egresos en Operaciones de Caja"));

			tablaOp.AddCell(Fila("Facturación:", r.co_facturacion));
			tablaOp.AddCell(Fila("Facturación Diferida a otros PV:", r.co_facturacion_dif));
			tablaOp.AddCell(Fila("Cobranzas:", r.co_cobranza));
			tablaOp.AddCell(Fila("Cobranza de Ventas de otros PV:", r.co_cobranza_dif));
			tablaOp.AddCell(Fila("Anticipos de Clientes:", r.co_creditos_gen));
			tablaOp.AddCell(Fila("Devolución de Dinero:", r.co_devolucion_dinero));
			tablaOp.AddCell(Fila("Créditos Usados en Pago:", r.co_creditos_usados));
			tablaOp.AddCell(Fila("Ingresos de Valores Externos:", r.co_ingresos));

			// Fila en blanco antes del total
			tablaOp.AddCell(Celda(new Phrase(" "), Element.ALIGN_LEFT, false));

			// Total con fondo gris
			tablaOp.AddCell(TotalFila("Total a Rendir por Caja:", r.total_caja));

			// ============================================================
			// TABLA DERECHA
			// ============================================================

			PdfPTable tablaRen = new PdfPTable(1);
			tablaRen.WidthPercentage = 100;

			tablaRen.AddCell(TituloCelda("Rendiciones y Fondos de Caja"));

			tablaRen.AddCell(Fila("Efectivos:", r.efectivos));
			tablaRen.AddCell(Fila("Cheques:", r.cheques));
			tablaRen.AddCell(Fila("Tickets:", r.tickets));
			tablaRen.AddCell(Fila("Tarjetas (D-C):", r.tarjetas));
			tablaRen.AddCell(Fila("Mutuales y Vales de Compra:", r.mutuales + r.vales));
			tablaRen.AddCell(Fila("Otros:", r.otros));
			tablaRen.AddCell(Fila("Documentos a Cuenta Corriente:", r.co_ctacte));
			tablaRen.AddCell(Fila("Fondos Existentes en Apertura de Caja:", r.fondo_inicial));
			tablaRen.AddCell(Fila("Fondos Depositados en Cierre de Caja:", r.fondo_final));

			decimal totalRendicionFondos =
				r.efectivos + r.cheques + r.tickets + r.tarjetas +
				r.mutuales + r.vales + r.otros + r.co_ctacte +
				r.fondo_inicial + r.fondo_final;

			tablaRen.AddCell(TotalFila("Rendición + Fondos de Caja:", totalRendicionFondos));


			// ============================================================
			// TABLA CONTENEDORA
			// ============================================================

			PdfPTable tablaCont = new PdfPTable(2);
			tablaCont.WidthPercentage = 100;
			tablaCont.SetWidths(new float[] { 50f, 50f });

			tablaCont.AddCell(new PdfPCell(tablaOp)
			{
				Border = Rectangle.BOX,
				BorderColor = new BaseColor(180, 180, 180),
				Padding = 5f
			});

			tablaCont.AddCell(new PdfPCell(tablaRen)
			{
				Border = Rectangle.BOX,
				BorderColor = new BaseColor(180, 180, 180),
				Padding = 5f
			});

			pdf.Add(tablaCont);

			// ============================================================
			// TOTALIZADOR FINAL: DIFERENCIA DE CAJA
			// ============================================================

			decimal diferencia = r.total_caja - totalRendicionFondos;
			string signoDif = diferencia >= 0 ? "+" : "-";
			decimal absDif = Math.Abs(diferencia);

			PdfPTable tablaDif = new PdfPTable(1);
			tablaDif.WidthPercentage = 49; // alineado con la tabla izquierda
			tablaDif.HorizontalAlignment = Element.ALIGN_LEFT;
			tablaDif.AddCell(TotalFila("Diferencia de Caja (Faltante):", diferencia));

			pdf.Add(tablaDif);

			// ============================
			// LÍNEA HORIZONTAL NEGRA FULL WIDTH
			// ============================
			var linea = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.Black, Element.ALIGN_CENTER, -2);
			pdf.Add(new Chunk(linea));

			pdf.Add(new Paragraph(" ", chico)); ;
		}

		public static void CargarRepoVta_SeccionNro1_B(Document pdf, List<RepoVtaResumenDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			var r = registros.First();

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(Phrase frase, int align = Element.ALIGN_LEFT, bool lineaInferior = true)
			{
				return new PdfPCell(frase)
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					PaddingTop = 5f,
					PaddingBottom = 5f,
					PaddingLeft = 4f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPTable Fila(string label, string valor)
			{
				PdfPTable fila = new PdfPTable(2);
				fila.WidthPercentage = 100;
				fila.SetWidths(new float[] { 60f, 40f });

				fila.AddCell(Celda(new Phrase(label, normal), Element.ALIGN_RIGHT));
				fila.AddCell(Celda(new Phrase(valor, normalBold), Element.ALIGN_RIGHT));

				return fila;
			}

			// ============================================================
			// ETIQUETA SUPERIOR
			// ============================================================

			pdf.Add(new Paragraph("2) Cantidad de Operaciones y Otras Operaciones", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA IZQUIERDA — Cantidad de Operaciones Realizadas
			// ============================================================

			PdfPTable tablaCant = new PdfPTable(1);
			tablaCant.WidthPercentage = 100;

			tablaCant.AddCell(TituloCelda("Cantidad de Operaciones Realizadas"));

			tablaCant.AddCell(Fila("Facturación:", r.cant_facturacion.ToString()));
			tablaCant.AddCell(Fila("Nota de Créditos CtaCte:", r.cant_nota_credito.ToString()));
			tablaCant.AddCell(Fila("Cobranzas:", r.cant_cobranza.ToString()));
			tablaCant.AddCell(Fila("Cobranzas Anuladas:", r.cant_cobranza_anu.ToString()));
			tablaCant.AddCell(Fila("Nota de Crédito por Devolución de Dinero:", r.cant_devolucion_dinero.ToString()));
			tablaCant.AddCell(Fila("Servicios (FS, ND, NC):", r.cant_nota_debito_prov.ToString()));
			tablaCant.AddCell(Fila("Cobranzas de Facturas Diferidas:", r.cant_cobranza_dif.ToString()));
			tablaCant.AddCell(Fila("Cambio de Valores / Ing. Externos:", r.cant_cambio_ing.ToString()));

			int totalOperaciones =
				r.cant_facturacion +
				r.cant_nota_credito +
				r.cant_cobranza +
				r.cant_cobranza_anu +
				r.cant_devolucion_dinero +
				r.cant_nota_debito_prov +
				r.cant_cobranza_dif +
				r.cant_cambio_ing;

			PdfPTable totalCantTabla = new PdfPTable(2);
			totalCantTabla.WidthPercentage = 100;
			totalCantTabla.SetWidths(new float[] { 60f, 40f });

			// Label
			totalCantTabla.AddCell(new PdfPCell(new Phrase("Total de Ope. Realizadas:", normalBold))
			{
				BackgroundColor = new BaseColor(200, 200, 200),
				Border = Rectangle.BOX,
				BorderColor = new BaseColor(120, 120, 120),
				HorizontalAlignment = Element.ALIGN_RIGHT,
				Padding = 5f
			});

			// Valor
			totalCantTabla.AddCell(new PdfPCell(new Phrase(totalOperaciones.ToString(), normalBold))
			{
				BackgroundColor = new BaseColor(200, 200, 200),
				Border = Rectangle.BOX,
				BorderColor = new BaseColor(120, 120, 120),
				HorizontalAlignment = Element.ALIGN_RIGHT,
				Padding = 5f
			});

			tablaCant.AddCell(new PdfPCell(totalCantTabla)
			{
				Border = Rectangle.NO_BORDER,
				Padding = 0
			});

			// ============================================================
			// TABLA DERECHA — Importe de Otras Operaciones
			// ============================================================

			PdfPTable tablaImp = new PdfPTable(1);
			tablaImp.WidthPercentage = 100;

			tablaImp.AddCell(TituloCelda("Importe de Otras Operaciones"));

			tablaImp.AddCell(Fila("NC Generadas en CtaCte.:", r.co_nota_credito.ToString("N2")));
			tablaImp.AddCell(Fila("Notas de Débito + FS:", r.co_nota_debito_prov.ToString("N2")));

			// ============================================================
			// TABLA CONTENEDORA (dos columnas)
			// ============================================================

			PdfPTable tablaCont = new PdfPTable(2);
			tablaCont.WidthPercentage = 100;
			tablaCont.SetWidths(new float[] { 50f, 50f });

			tablaCont.AddCell(new PdfPCell(tablaCant)
			{
				Border = Rectangle.BOX,
				BorderColor = new BaseColor(180, 180, 180),
				Padding = 5f
			});

			PdfPCell celdaImp = new PdfPCell();
			celdaImp.Border = Rectangle.BOX;
			celdaImp.BorderColor = new BaseColor(180, 180, 180);
			celdaImp.Padding = 5f;
			celdaImp.MinimumHeight = 0;     // evita estiramiento
			celdaImp.FixedHeight = -1;      // permite altura natural
			celdaImp.AddElement(tablaImp);  // agrega la tabla sin expandir

			tablaCont.AddCell(celdaImp);

			pdf.Add(tablaCont);
			pdf.Add(new Paragraph(" ", chico));
		}


		/// <summary>
		/// Sección #2 Valores Rendidos por tipo de Medio de Pago
		/// </summary>
		/// <param name="pdf">Componente de generación de reporte</param>
		/// <param name="registros">Lista para generar datos</param>
		/// <param name="chico">Tipo de fuente</param>
		/// <param name="normal">Tipo de fuente</param>
		/// <param name="normalBold">Tipo de fuente</param>
		/// <param name="titulo">Tipo de fuente</param>
		/// <param name="tituloBig">Tipo de fuente</param>
		public static void CargarRepoVta_SeccionNro2_A(Document pdf, List<RepoVtaRendicionDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// NUEVA HOJA
			// ============================================================
			pdf.NewPage();

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell CeldaDato(string texto, Font font, int align = Element.ALIGN_LEFT, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell CeldaTitulo(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					PaddingTop = 5f,
					PaddingBottom = 5f,
					PaddingLeft = 4f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell CeldaSubtitulo(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					Colspan = 4,
					HorizontalAlignment = Element.ALIGN_LEFT
				};
			}

			PdfPCell CeldaTotal(string texto, Font font, bool esLabel)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = esLabel ? Element.ALIGN_LEFT : Element.ALIGN_RIGHT
				};
			}

			// ============================================================
			// AGRUPAR POR NÚMERO DE RENDICIÓN
			// ============================================================

			var grupos = registros
				.GroupBy(x => x.caja_nro_rend)
				.OrderBy(g => g.Key);

			foreach (var grupo in grupos)
			{
				var lista = grupo.OrderBy(x => x.orden).ToList();
				var r0 = lista.First();

				// ============================================================
				// TÍTULO DE LA RENDICIÓN
				// ============================================================

				string tipo = r0.rend_tipo switch
				{
					"P" => "Rendición Parcial Nº ",
					"F" => "Rendición Final Nº ",
					"T" => "Total de Rendiciones Nº ",
					_ => "Rendición Nº "
				};

				string fecha = r0.rend_fecha.HasValue
					? r0.rend_fecha.Value.ToString("dd/MM/yyyy HH:mm")
					: "";

				pdf.Add(new Paragraph($"{tipo}{r0.caja_nro_rend}  {fecha}", titulo));
				pdf.Add(new Paragraph(" ", chico));

				// ============================================================
				// TABLA PRINCIPAL
				// ============================================================

				PdfPTable tabla = new PdfPTable(4);
				tabla.WidthPercentage = 100;
				tabla.SetWidths(new float[] { 40f, 20f, 20f, 20f });

				tabla.AddCell(CeldaTitulo("Medio de Pago"));
				tabla.AddCell(CeldaTitulo("Arqueo Caja"));
				tabla.AddCell(CeldaTitulo("Fondo"));
				tabla.AddCell(CeldaTitulo("Rendición"));

				// ============================================================
				// SUBTOTALES POR TIPO DE INSTRUMENTO (tcf_id / tcf_desc)
				// ============================================================

				var subgrupos = lista
					.GroupBy(x => x.tcf_id)
					.OrderBy(g => g.Key);

				decimal totalArqueo = 0m;
				decimal totalFondo = 0m;

				foreach (var sg in subgrupos)
				{
					string instrumentoDesc = sg.First().tcf_desc;

					// Subtítulo del grupo (Efectivo, Tarjetas de Crédito, Cuentas Bancarias, etc.)
					tabla.AddCell(CeldaSubtitulo(instrumentoDesc));

					decimal subArqueo = 0m;
					decimal subFondo = 0m;

					foreach (var item in sg)
					{
						decimal arqueo = item.rendicion;
						decimal fondo = item.fondo;
						decimal rend = arqueo - fondo;

						// Fila de datos (SIN tablas anidadas)
						tabla.AddCell(CeldaDato(item.ins_desc, normal, Element.ALIGN_LEFT));              // Medio de Pago
						tabla.AddCell(CeldaDato(arqueo.ToString("N2"), normalBold, Element.ALIGN_RIGHT)); // Arqueo Caja
						tabla.AddCell(CeldaDato(fondo.ToString("N2"), normalBold, Element.ALIGN_RIGHT));  // Fondo
						tabla.AddCell(CeldaDato(rend.ToString("N2"), normalBold, Element.ALIGN_RIGHT));   // Rendición

						subArqueo += arqueo;
						subFondo += fondo;
					}

					decimal subRend = subArqueo - subFondo;

					// Fila total del grupo
					tabla.AddCell(CeldaTotal($"Total {instrumentoDesc}:", normalBold, true));
					tabla.AddCell(CeldaTotal(subArqueo.ToString("N2"), normalBold, false));
					tabla.AddCell(CeldaTotal(subFondo.ToString("N2"), normalBold, false));
					tabla.AddCell(CeldaTotal(subRend.ToString("N2"), normalBold, false));

					totalArqueo += subArqueo;
					totalFondo += subFondo;
				}

				// ============================================================
				// TOTAL GENERAL DE LA RENDICIÓN
				// ============================================================

				decimal totalRend = totalArqueo - totalFondo;

				tabla.AddCell(CeldaTotal("Total Rendición:", normalBold, true));
				tabla.AddCell(CeldaTotal(totalArqueo.ToString("N2"), normalBold, false));
				tabla.AddCell(CeldaTotal(totalFondo.ToString("N2"), normalBold, false));
				tabla.AddCell(CeldaTotal(totalRend.ToString("N2"), normalBold, false));

				pdf.Add(tabla);
				pdf.Add(new Paragraph(" ", chico));
			}
		}

		public static void CargarRepoVta_SeccionNro2_B(Document pdf, List<RepoVtaRendicionDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell SubtotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Tarjetas", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(8);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 20f, 15f, 10f, 10f, 10f, 10f, 12f, 13f });

			tabla.AddCell(TituloCelda("Tarjeta"));
			tabla.AddCell(TituloCelda("N° Tarjeta"));
			tabla.AddCell(TituloCelda("Cuotas"));
			tabla.AddCell(TituloCelda("N° Lote"));
			tabla.AddCell(TituloCelda("N° Cupón"));
			tabla.AddCell(TituloCelda("CashBack"));
			tabla.AddCell(TituloCelda("Venta"));
			tabla.AddCell(TituloCelda("Importe"));

			// ============================================================
			// AGRUPAR POR TARJETA (ins_desc)
			// ============================================================

			var grupos = registros
				.GroupBy(x => x.ins_desc)
				.OrderBy(g => g.Key);

			decimal totalVenta = 0m;
			decimal totalImporte = 0m;

			foreach (var grupo in grupos)
			{
				string tarjeta = grupo.Key;
				int cantidadCupones = grupo.Count();

				decimal subtotalVenta = 0m;
				decimal subtotalImporte = 0m;

				// ------------------------------------------------------------
				// FILAS DE DETALLE
				// ------------------------------------------------------------
				foreach (var item in grupo.OrderBy(x => x.rend_item))
				{
					tabla.AddCell(Celda(tarjeta, normal, Element.ALIGN_LEFT)); // Tarjeta
					tabla.AddCell(Celda("", normal, Element.ALIGN_LEFT));      // N° Tarjeta vacío
					tabla.AddCell(Celda("1", normalBold, Element.ALIGN_RIGHT)); // Cuotas
					tabla.AddCell(Celda(item.rend_dato2_valor, normal, Element.ALIGN_CENTER)); // Lote
					tabla.AddCell(Celda(item.rend_dato3_valor, normal, Element.ALIGN_CENTER)); // Cupón
					tabla.AddCell(Celda("0.00", normalBold, Element.ALIGN_RIGHT)); // Cashback

					tabla.AddCell(Celda(item.rend_importe_arq.ToString("N2"), normalBold, Element.ALIGN_RIGHT)); // Venta
					tabla.AddCell(Celda(item.rend_importe_ok.ToString("N2"), normalBold, Element.ALIGN_RIGHT));  // Importe

					subtotalVenta += item.rend_importe_arq;
					subtotalImporte += item.rend_importe_ok;
				}

				// ------------------------------------------------------------
				// SUBTOTAL DEL GRUPO
				// ------------------------------------------------------------

				tabla.AddCell(SubtotalCelda($"{tarjeta} - ({cantidadCupones} C):", 6));
				tabla.AddCell(SubtotalCelda(subtotalVenta.ToString("N2"), 1));
				tabla.AddCell(SubtotalCelda(subtotalImporte.ToString("N2"), 1));

				totalVenta += subtotalVenta;
				totalImporte += subtotalImporte;
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(SubtotalCelda("Total - (Todas las Tarjetas):", 6));
			tabla.AddCell(SubtotalCelda(totalVenta.ToString("N2"), 1));
			tabla.AddCell(SubtotalCelda(totalImporte.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro2_C(Document pdf, List<RepoVtaRendicionDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Cheques Rendidos por Caja - DIFERIDOS", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(7);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 8f, 22f, 15f, 12f, 12f, 23f, 15f });

			tabla.AddCell(TituloCelda("Rend."));
			tabla.AddCell(TituloCelda("Banco"));
			tabla.AddCell(TituloCelda("N° Cheque"));
			tabla.AddCell(TituloCelda("Plaza"));
			tabla.AddCell(TituloCelda("Fecha Vto."));
			tabla.AddCell(TituloCelda("Cliente"));
			tabla.AddCell(TituloCelda("Importe"));

			// ============================================================
			// FILAS
			// ============================================================

			decimal totalImporte = 0m;

			foreach (var item in registros.OrderBy(x => x.caja_nro_rend))
			{
				string fechaVto = item.rend_fecha_valor.HasValue
					? item.rend_fecha_valor.Value.ToString("dd/MM/yyyy")
					: "";

				string cliente = "";
				if (!string.IsNullOrWhiteSpace(item.cta_id))
					cliente = $"({item.cta_id}) {item.cta_denominacion}";
				else
					cliente = item.cta_denominacion ?? "";

				tabla.AddCell(Celda(item.caja_nro_rend.ToString(), normal, Element.ALIGN_RIGHT));
				tabla.AddCell(Celda(item.rend_dato1_valor, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.rend_dato2_valor, normal, Element.ALIGN_RIGHT));
				tabla.AddCell(Celda(item.rend_dato3_valor, normal, Element.ALIGN_CENTER));
				tabla.AddCell(Celda(fechaVto, normal, Element.ALIGN_CENTER));
				tabla.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.rend_importe_ok.ToString("N2"), normalBold, Element.ALIGN_RIGHT));

				totalImporte += item.rend_importe_ok;
			}

			// ============================================================
			// TOTAL
			// ============================================================

			tabla.AddCell(TotalCelda("Total Cheques:", 6));
			tabla.AddCell(TotalCelda(totalImporte.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro2_D(Document pdf, List<RepoVtaRendicionDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell SubtituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					Colspan = 4,
					HorizontalAlignment = Element.ALIGN_LEFT
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Transferencias / Depósitos", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(4);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 20f, 20f, 40f, 20f });

			tabla.AddCell(TituloCelda("N° Transf."));
			tabla.AddCell(TituloCelda("Fecha Transf."));
			tabla.AddCell(TituloCelda("Cliente"));
			tabla.AddCell(TituloCelda("Importe"));

			// ============================================================
			// AGRUPAR POR ins_desc
			// ============================================================

			var grupos = registros
				.GroupBy(x => x.ins_desc)
				.OrderBy(g => g.Key);

			decimal totalGeneral = 0m;

			foreach (var grupo in grupos)
			{
				string etiquetaGrupo = grupo.Key;
				decimal subtotal = 0m;

				// Subtítulo del grupo
				tabla.AddCell(SubtituloCelda(etiquetaGrupo));

				foreach (var item in grupo.OrderBy(x => x.rend_item))
				{
					string fecha = item.rend_fecha_valor.HasValue
						? item.rend_fecha_valor.Value.ToString("dd/MM/yyyy")
						: "";

					string cliente = "";
					if (!string.IsNullOrWhiteSpace(item.cta_id))
						cliente = $"({item.cta_id}) {item.cta_denominacion}";
					else
						cliente = item.cta_denominacion ?? "";

					tabla.AddCell(Celda(item.rend_dato3_valor, normal, Element.ALIGN_RIGHT));   // N° Transf.
					tabla.AddCell(Celda(fecha, normal, Element.ALIGN_CENTER));                 // Fecha
					tabla.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));                 // Cliente
					tabla.AddCell(Celda(item.rend_importe_ok.ToString("N2"), normalBold, Element.ALIGN_RIGHT)); // Importe

					subtotal += item.rend_importe_ok;
				}

				// Subtotal del grupo
				tabla.AddCell(TotalCelda($"Total {etiquetaGrupo}:", 3));
				tabla.AddCell(TotalCelda(subtotal.ToString("N2"), 1));

				totalGeneral += subtotal;
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(TotalCelda("Total General:", 3));
			tabla.AddCell(TotalCelda(totalGeneral.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro2_E(Document pdf, List<RepoVtaRendicionDetalleDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Otros Instrumentos", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(3);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 30f, 50f, 20f });

			tabla.AddCell(TituloCelda("Instrumento"));
			tabla.AddCell(TituloCelda("Concepto"));
			tabla.AddCell(TituloCelda("Importe"));

			// ============================================================
			// FILAS
			// ============================================================

			decimal totalImporte = 0m;

			foreach (var item in registros.OrderBy(x => x.ins_desc))
			{
				tabla.AddCell(Celda(item.ins_desc, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.concepto_valor, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.rend_importe_ok.ToString("N2"), normalBold, Element.ALIGN_RIGHT));

				totalImporte += item.rend_importe_ok;
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(TotalCelda("Total:", 2));
			tabla.AddCell(TotalCelda(totalImporte.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_A(Document pdf, List<RepoVtaCtaCteDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO
			// ============================================================

			pdf.Add(new Paragraph("4) Detalle de Operaciones Especiales", tituloBig));
			pdf.Add(new Paragraph("Detalle de Documentos Generados", titulo));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL (3 columnas)
			// ============================================================

			PdfPTable tabla = new PdfPTable(3);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 25f, 55f, 20f });

			// Encabezados con el MISMO estilo
			tabla.AddCell(TituloCelda("Documento"));
			tabla.AddCell(TituloCelda("Cliente"));
			tabla.AddCell(TituloCelda("Importe"));

			// ============================================================
			// FILAS
			// ============================================================

			decimal totalImporte = 0m;

			foreach (var item in registros)
			{
				string cliente = $"({item.cta_id}) {item.cta_denominacion}";
				totalImporte += item.co_ctacte;

				tabla.AddCell(Celda(item.doc_compte, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.co_ctacte.ToString("N2"), normalBold, Element.ALIGN_RIGHT));
			}

			// ============================================================
			// TOTAL (idéntico estilo a SeccionNro2_C)
			// ============================================================

			tabla.AddCell(TotalCelda("TOTAL:", 2));
			tabla.AddCell(TotalCelda(totalImporte.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_B(Document pdf, List<RepoVtaCobranzaDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers (idénticos a SeccionNro2_C)
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// Separación de registros
			// ============================================================

			var docs = registros.Where(x => x.tco_id == "DOC").ToList();
			var otros = registros.Where(x => x.tco_id != "DOC").ToList();

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Cobranzas", tituloBig));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA 1 – Documentos a Cobrar (tco_id = DOC)
			// ============================================================

			if (docs.Count > 0)
			{
				pdf.Add(new Paragraph("Documentos a Cobrar", titulo));
				pdf.Add(new Paragraph(" ", chico));

				PdfPTable tabla1 = new PdfPTable(4);
				tabla1.WidthPercentage = 100;
				tabla1.SetWidths(new float[] { 25f, 25f, 35f, 15f });

				tabla1.AddCell(TituloCelda("Recibo"));
				tabla1.AddCell(TituloCelda("Compte. Cance."));
				tabla1.AddCell(TituloCelda("Cliente"));
				tabla1.AddCell(TituloCelda("Importe"));

				decimal total1 = 0m;

				foreach (var item in docs)
				{
					string cliente = $"({item.cta_id}) {item.cta_denominacion}";
					total1 += item.cc_importe;

					tabla1.AddCell(Celda(item.rb_compte, normal, Element.ALIGN_LEFT));
					tabla1.AddCell(Celda(item.cm_compte, normal, Element.ALIGN_LEFT));
					tabla1.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));
					tabla1.AddCell(Celda(item.cc_importe.ToString("N2"), normalBold, Element.ALIGN_RIGHT));
				}

				tabla1.AddCell(TotalCelda("Total:", 3));
				tabla1.AddCell(TotalCelda(total1.ToString("N2"), 1));

				pdf.Add(tabla1);
				pdf.Add(new Paragraph(" ", chico));
			}

			// ============================================================
			// TABLA 2 – Valores Rechazados / Otros Comprobantes
			// ============================================================

			if (otros.Count > 0)
			{
				pdf.Add(new Paragraph("Valores Rechazados", titulo));
				pdf.Add(new Paragraph(" ", chico));

				PdfPTable tabla2 = new PdfPTable(4);
				tabla2.WidthPercentage = 100;
				tabla2.SetWidths(new float[] { 25f, 25f, 35f, 15f });

				tabla2.AddCell(TituloCelda("Recibo"));
				tabla2.AddCell(TituloCelda("Compte. Cance."));
				tabla2.AddCell(TituloCelda("Cliente"));
				tabla2.AddCell(TituloCelda("Importe"));

				decimal total2 = 0m;

				foreach (var item in otros)
				{
					string cliente = $"({item.cta_id}) {item.cta_denominacion}";
					total2 += item.cc_importe;

					tabla2.AddCell(Celda(item.rb_compte, normal, Element.ALIGN_LEFT));
					tabla2.AddCell(Celda(item.cm_compte, normal, Element.ALIGN_LEFT));
					tabla2.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));
					tabla2.AddCell(Celda(item.cc_importe.ToString("N2"), normalBold, Element.ALIGN_RIGHT));
				}

				tabla2.AddCell(TotalCelda("Total:", 3));
				tabla2.AddCell(TotalCelda(total2.ToString("N2"), 1));

				pdf.Add(tabla2);
				pdf.Add(new Paragraph(" ", chico));
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			decimal totalGeneral = registros.Sum(x => x.cc_importe);

			PdfPTable tablaTotal = new PdfPTable(1);
			tablaTotal.WidthPercentage = 30;
			tablaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;

			tablaTotal.AddCell(TotalCelda("Total Cobranza: " + totalGeneral.ToString("N2"), 1));

			pdf.Add(tablaTotal);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_C(Document pdf, List<RepoVtaNCDto> registros, Font chico, Font chicoBold, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers (idénticos a SeccionNro2_C)
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Notas de Créditos Generadas", tituloBig));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// AGRUPAR POR co_tipo
			// ============================================================

			var grupos = registros
				.GroupBy(x => new { x.co_tipo, x.co_tipo_desc })
				.OrderBy(g => g.Key.co_tipo)
				.ToList();

			decimal totalGeneral = 0m;

			// ============================================================
			// UNA TABLA POR GRUPO
			// ============================================================

			foreach (var grupo in grupos)
			{
				pdf.Add(new Paragraph(grupo.Key.co_tipo_desc, titulo));
				pdf.Add(new Paragraph(" ", chico));

				PdfPTable tabla = new PdfPTable(6);
				tabla.WidthPercentage = 100;
				tabla.SetWidths(new float[] { 12f, 18f, 30f, 18f, 12f, 20f });

				// Encabezados
				tabla.AddCell(TituloCelda("Tipo"));
				tabla.AddCell(TituloCelda("Comprobante"));
				tabla.AddCell(TituloCelda("Cliente"));
				tabla.AddCell(TituloCelda("Compr. Ori."));
				tabla.AddCell(TituloCelda("Importe"));
				tabla.AddCell(TituloCelda("Autorizado por"));

				decimal subtotal = 0m;

				foreach (var item in grupo)
				{
					string cliente = $"({item.cta_id}) {item.cta_denominacion}";
					subtotal += item.co_nota_credito;

					tabla.AddCell(Celda(item.tco_desc, chico, Element.ALIGN_LEFT));
					tabla.AddCell(Celda(item.cm_compte, chico, Element.ALIGN_CENTER));
					tabla.AddCell(Celda(cliente, chico, Element.ALIGN_LEFT));
					tabla.AddCell(Celda(item.cm_compte_ori, chico, Element.ALIGN_CENTER));
					tabla.AddCell(Celda(item.co_nota_credito.ToString("N2"), chicoBold, Element.ALIGN_RIGHT));
					tabla.AddCell(Celda(item.usu_apellidoynombre_autoriza, chico, Element.ALIGN_LEFT));
				}

				// Subtotal
				tabla.AddCell(TotalCelda("Subtotal:", 5));
				tabla.AddCell(TotalCelda(subtotal.ToString("N2"), 1));

				totalGeneral += subtotal;

				pdf.Add(tabla);
				pdf.Add(new Paragraph(" ", chico));
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			PdfPTable tablaTotal = new PdfPTable(1);
			tablaTotal.WidthPercentage = 30;
			tablaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;

			tablaTotal.AddCell(TotalCelda("TOTAL GENERAL: " + totalGeneral.ToString("N2"), 1));

			pdf.Add(tablaTotal);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_D(Document pdf, List<RepoVtaNDDto> registros, Font chico, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers (idénticos a SeccionNro2_C)
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, normalBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Notas de Débitos a Sujetos Comerciales", tituloBig));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(4);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 20f, 20f, 40f, 20f });

			// Encabezados
			tabla.AddCell(TituloCelda("Tipo Compte."));
			tabla.AddCell(TituloCelda("Comprobante"));
			tabla.AddCell(TituloCelda("Cliente / Proveedor"));
			tabla.AddCell(TituloCelda("Importe"));

			decimal total = 0m;

			foreach (var item in registros)
			{
				string cliente = $"({item.cta_id}) {item.cta_denominacion}";
				total += item.co_nota_debito_prov;

				tabla.AddCell(Celda(item.tco_desc, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.cm_compte, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(cliente, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.co_nota_debito_prov.ToString("N2"), normalBold, Element.ALIGN_RIGHT));
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(TotalCelda("TOTAL:", 3));
			tabla.AddCell(TotalCelda(total.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_E(Document pdf, List<RepoVtaCreditoUsadoDto> registros, Font chico, Font chicoBold, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers (idénticos a SeccionNro2_C)
			// ============================================================

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, chicoBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, chicoBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Detalle de Créditos Usados como Medio de Pago", tituloBig));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(5);
			tabla.WidthPercentage = 100;
			tabla.SetWidths(new float[] { 18f, 18f, 12f, 32f, 20f });

			// Encabezados
			tabla.AddCell(TituloCelda("Tipo Compte."));
			tabla.AddCell(TituloCelda("Comprobante"));
			tabla.AddCell(TituloCelda("Ope N°"));
			tabla.AddCell(TituloCelda("Cliente"));
			tabla.AddCell(TituloCelda("Importe"));

			decimal total = 0m;

			foreach (var item in registros)
			{
				string cliente = $"({item.cta_id}) {item.cta_denominacion}";
				total += item.cc_importe;

				tabla.AddCell(Celda(item.tco_desc, chico, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.cm_compte, chico, Element.ALIGN_LEFT));
				tabla.AddCell(Celda("", chico, Element.ALIGN_CENTER)); // Ope N° vacío
				tabla.AddCell(Celda(cliente, chico, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.cc_importe.ToString("N2"), normalBold, Element.ALIGN_RIGHT));
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(TotalCelda("TOTAL:", 4));
			tabla.AddCell(TotalCelda(total.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		public static void CargarRepoVta_SeccionNro4_F(Document pdf, List<RepoVtaZDto> registros, Font chico, Font chicoBold, Font normal, Font normalBold, Font titulo, Font tituloBig)
		{
			if (registros == null || registros.Count == 0)
				return;

			// ============================================================
			// Helpers
			// ============================================================
			PdfPCell CeldaTotalParcial(string texto, Font font, int align)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					BackgroundColor = new BaseColor(230, 230, 230), // gris encabezado
					Border = Rectangle.BOTTOM_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell Celda(string texto, Font font, int align, bool lineaInferior = true)
			{
				return new PdfPCell(new Phrase(texto, font))
				{
					Border = lineaInferior ? Rectangle.BOTTOM_BORDER : Rectangle.NO_BORDER,
					BorderWidthBottom = 0.5f,
					BorderColorBottom = new BaseColor(180, 180, 180),
					HorizontalAlignment = align,
					PaddingTop = 4f,
					PaddingBottom = 4f,
					PaddingLeft = 4f,
					PaddingRight = 4f
				};
			}

			PdfPCell CeldaEspacio()
			{
				return new PdfPCell(new Phrase(""))
				{
					Border = Rectangle.NO_BORDER,
					Padding = 0,
					FixedHeight = 1f
				};
			}

			PdfPCell TituloCelda(string texto)
			{
				return new PdfPCell(new Phrase(texto, chicoBold))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(180, 180, 180),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_CENTER
				};
			}

			PdfPCell TituloEspacio()
			{
				return new PdfPCell(new Phrase(""))
				{
					BackgroundColor = new BaseColor(230, 230, 230),
					Border = Rectangle.NO_BORDER,
					Padding = 0
				};
			}

			PdfPCell TotalCelda(string texto, int colspan)
			{
				return new PdfPCell(new Phrase(texto, chicoBold))
				{
					BackgroundColor = new BaseColor(200, 200, 200),
					Border = Rectangle.BOX,
					BorderColor = new BaseColor(120, 120, 120),
					Padding = 5f,
					HorizontalAlignment = Element.ALIGN_RIGHT,
					Colspan = colspan
				};
			}

			// ============================================================
			// TÍTULO GENERAL
			// ============================================================

			pdf.Add(new Paragraph("Información Fiscal", tituloBig));
			pdf.Add(new Paragraph(" ", chico));

			// ============================================================
			// TABLA PRINCIPAL
			// ============================================================

			PdfPTable tabla = new PdfPTable(12);
			tabla.WidthPercentage = 100;

			tabla.SetWidths(new float[]
			{
				16f,   // Concepto
				11f,   // Fact A
				11f,   // Fact B
				12f,   // Tot Fact
				1f,    // espacio
				11f,   // NC A
				11f,   // NC B
				12f,   // Tot NC
				1f,    // espacio
				10f,   // ND
				1f,    // espacio
				14f    // Total Gral
			});

			// ============================================================
			// ENCABEZADOS
			// ============================================================

			tabla.AddCell(TituloCelda("Concepto"));
			tabla.AddCell(TituloCelda("Facturas A"));
			tabla.AddCell(TituloCelda("Facturas B"));
			tabla.AddCell(TituloCelda("Tot. Facturas"));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TituloCelda("N. Crédito A"));
			tabla.AddCell(TituloCelda("N. Crédito B"));
			tabla.AddCell(TituloCelda("Tot. N. Créd."));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TituloCelda("N. Débito"));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TituloCelda("Tot. General"));

			// ============================================================
			// FILAS
			// ============================================================

			decimal totalFactA = 0, totalFactB = 0, totalFact = 0;
			decimal totalNcA = 0, totalNcB = 0, totalNc = 0;
			decimal totalNd = 0, totalGral = 0;

			foreach (var item in registros.OrderBy(x => x.orden))
			{
				decimal totFact = item.ft_a_imp + item.ft_b_imp;
				decimal totNc = item.nc_a_imp + item.nc_b_imp;
				decimal totGeneral = totFact + totNc + item.nd_a_imp;

				totalFactA += item.ft_a_imp;
				totalFactB += item.ft_b_imp;
				totalFact += totFact;

				totalNcA += item.nc_a_imp;
				totalNcB += item.nc_b_imp;
				totalNc += totNc;

				totalNd += item.nd_a_imp;
				totalGral += totGeneral;

				tabla.AddCell(Celda(item.tipo_desc, normal, Element.ALIGN_LEFT));
				tabla.AddCell(Celda(item.ft_a_imp.ToString("N2"), chico, Element.ALIGN_RIGHT));
				tabla.AddCell(Celda(item.ft_b_imp.ToString("N2"), chico, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaTotalParcial(totFact.ToString("N2"), chicoBold, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaEspacio());
				tabla.AddCell(Celda(item.nc_a_imp.ToString("N2"), chico, Element.ALIGN_RIGHT));
				tabla.AddCell(Celda(item.nc_b_imp.ToString("N2"), chico, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaTotalParcial(totNc.ToString("N2"), chicoBold, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaEspacio());
				tabla.AddCell(Celda(item.nd_a_imp.ToString("N2"), chico, Element.ALIGN_RIGHT));
				tabla.AddCell(CeldaEspacio());
				tabla.AddCell(CeldaTotalParcial(totGeneral.ToString("N2"), chicoBold, Element.ALIGN_RIGHT));
			}

			// ============================================================
			// TOTAL GENERAL
			// ============================================================

			tabla.AddCell(TotalCelda("Totales", 1));
			tabla.AddCell(TotalCelda(totalFactA.ToString("N2"), 1));
			tabla.AddCell(TotalCelda(totalFactB.ToString("N2"), 1));
			tabla.AddCell(TotalCelda(totalFact.ToString("N2"), 1));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TotalCelda(totalNcA.ToString("N2"), 1));
			tabla.AddCell(TotalCelda(totalNcB.ToString("N2"), 1));
			tabla.AddCell(TotalCelda(totalNc.ToString("N2"), 1));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TotalCelda(totalNd.ToString("N2"), 1));
			tabla.AddCell(TituloEspacio());
			tabla.AddCell(TotalCelda(totalGral.ToString("N2"), 1));

			pdf.Add(tabla);
			pdf.Add(new Paragraph(" ", chico));
		}

		#endregion

		#region Funciones Auxiliares

		#endregion

		#region Funciones de obtencion de datos
		private List<CajaProcesoCierresListaDto> ObtenerDatos(ReporteSolicitudDto solicitud, out string titulo, out string subtit)
		{
			try
			{
				var ret = new List<CajaProcesoCierresListaDto>();
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var sucursales = solicitud.Parametros.GetValueOrDefault("suc", "")?.ToString() ?? null;
				titulo = $"Resumen de Operaciones y Detalle de Rendición de Caja";

				if (!string.IsNullOrEmpty(caja_nro_proceso))
					ret = _ventasSv.ObtenerCajaProcesoCierresLista(caja_nro_proceso);
				subtit = $"Sucursal {sucursales}\nPunto de Venta (Caja): {ret.First().caja_id}";
				return ret;
			}
			catch (Exception)
			{
				titulo = $"";
				subtit = $"";
				return [];
			}

		}

		private List<RepoVtaResumenDto> ObtenerDatosResumen(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaResumen(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaRendicionDto> ObtenerDatosRendicion(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaRendicion(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaRendicionDetalleDto> ObtenerDatosRendicionDetalleCheques(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaDetRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					tcf_id = "CH"
				};
				return _ventasSv.ObtenerRepoVtaRendicionDetalle(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaRendicionDetalleDto> ObtenerDatosRendicionDetalleTransferencias(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaDetRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					tcf_id = "BA"
				};
				return _ventasSv.ObtenerRepoVtaRendicionDetalle(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaRendicionDetalleDto> ObtenerDatosRendicionDetalleTarjetas(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaDetRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					tcf_id = "TC"
				};
				return _ventasSv.ObtenerRepoVtaRendicionDetalle(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaRendicionDetalleDto> ObtenerDatosRendicionDetalleOtros(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaDetRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					tcf_id = "OT"
				};
				return _ventasSv.ObtenerRepoVtaRendicionDetalle(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaCtaCteDto> ObtenerDatosCtaCte(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaCtaCte(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaCobranzaDto> ObtenerDatosCobranzas(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaCobranza(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaAnticipoDto> ObtenerDatosAnticipos(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaAnticipo(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaCreditoUsadoDto> ObtenerDatosCreditosUsados(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaCreditoUsado(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaNCDto> ObtenerDatosNC(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaNC(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaNDDto> ObtenerDatosND(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaND(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaCambioValoresDto> ObtenerDatosCambioValores(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaCambioValores(request);
			}
			catch (Exception)
			{
				return [];
			}

		}

		private List<RepoVtaZDto> ObtenerDatosZ(ReporteSolicitudDto solicitud)
		{
			try
			{
				var caja_nro_proceso = solicitud.Parametros.GetValueOrDefault("caja_nro_proceso", "")?.ToString() ?? null;
				var caja_nro_cierre = solicitud.Parametros.GetValueOrDefault("caja_nro_cierre", "0")?.ToInt() ?? 0;
				var request = new RepoVtaRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre
				};
				return _ventasSv.ObtenerRepoVtaZ(request);
			}
			catch (Exception)
			{
				return [];
			}

		}
		#endregion

		public string GenerarTxt(ReporteSolicitudDto solicitud)
		{
			#region Obteniendo registros desde la base de datos
			string tit;
			string subtit;
			List<CajaProcesoCierresListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
			List<CajaProcesoCierresListaDto> registros = ObtenerDatos(solicitud, out tit, out subtit);

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
