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
				//List<RepoVtaCtaCteDto> registrosCtaCte = ObtenerDatosCtaCte(solicitud);
				//List<RepoVtaCobranzaDto> registrosCobranzas = ObtenerDatosCobranzas(solicitud);
				//List<RepoVtaAnticipoDto> registrosAnticipos = ObtenerDatosAnticipos(solicitud);
				//List<RepoVtaCreditoUsadoDto> registrosCreditosUsados = ObtenerDatosCreditosUsados(solicitud);
				//List<RepoVtaNCDto> registrosNC = ObtenerDatosNC(solicitud);
				//List<RepoVtaNDDto> registrosND = ObtenerDatosND(solicitud);
				//List<RepoVtaCambioValoresDto> registrosCambioValores = ObtenerDatosCambioValores(solicitud);
				//List<RepoVtaZDto> registrosZ = ObtenerDatosZ(solicitud);
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
				//HelperPdf.CargarRepoSorteoAnalisisProdLista(pdf, registros, chico, normal, normalBold, titulo, tituloBig);
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
