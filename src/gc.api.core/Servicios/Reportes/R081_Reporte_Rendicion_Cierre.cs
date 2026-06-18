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
				List<RepoVtaResumenDto> registrosResumen = ObtenerDatosResumen(solicitud); //Vamos descomentando a medida que vayamos armando el reporte
																						   //List<RepoVtaRendicionDto> registrosRendicion = ObtenerDatosRendicion(solicitud);
																						   //List<RepoVtaRendicionDetalleDto> registrosRendicionDetCheq = ObtenerDatosRendicionDetalleCheques(solicitud);
																						   //List<RepoVtaRendicionDetalleDto> registrosRendicionDetTran = ObtenerDatosRendicionDetalleTransferencias(solicitud);
																						   //List<RepoVtaRendicionDetalleDto> registrosRendicionDetTarj = ObtenerDatosRendicionDetalleTarjetas(solicitud);
																						   //List<RepoVtaRendicionDetalleDto> registrosRendicionDetOtros = ObtenerDatosRendicionDetalleOtros(solicitud);
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
				#region Sección Datos del cierre
				#region Datos del Cierre
				CargarRepoVtaDatosDeCierre(pdf, cierre, chico, normal, normalBold, titulo, tituloBig);
				#endregion

				#region #1 Resumen de Operaciones y Rendicion
				CargarRepoVta_SeccionNro1_A(pdf, registrosResumen, chico, normal, normalBold, titulo, tituloBig);
				#endregion
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
		/// <param name="pdf"></param>
		/// <param name="registros"></param>
		/// <param name="chico"></param>
		/// <param name="normal"></param>
		/// <param name="normalBold"></param>
		/// <param name="titulo"></param>
		/// <param name="tituloBig"></param>
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
			tablaOp.AddCell(TituloCelda($"Total a Rendir por Caja: {(r.total_caja >= 0 ? "+" : "-")} {Math.Abs(r.total_caja).ToString("N2")}"));

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

			tablaRen.AddCell(TituloCelda($"Rendición + Fondos de Caja: {(totalRendicionFondos >= 0 ? "+" : "-")} {Math.Abs(totalRendicionFondos).ToString("N2")}"));

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
			pdf.Add(new Paragraph(" ", chico)); ;
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
