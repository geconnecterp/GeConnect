namespace gc.api.core.Servicios
{
	using ClosedXML.Excel;
	using gc.api.core.Contratos.Servicios;
	using gc.api.core.Entidades;
	using gc.api.core.Interfaces.Datos;
	using gc.infraestructura.Core.EntidadesComunes;
	using gc.infraestructura.Core.EntidadesComunes.Options;
	using gc.infraestructura.Core.Exceptions;
	using gc.infraestructura.Dtos;
	using gc.infraestructura.Dtos.Almacen;
	using gc.infraestructura.Dtos.Consultas;
	using gc.infraestructura.Dtos.DocManager;
	using gc.infraestructura.Dtos.Gen;
	using gc.infraestructura.EntidadesComunes.Options;
	using gc.infraestructura.Helpers;
	using iTextSharp.text;
	using iTextSharp.text.pdf;
	using Microsoft.Data.SqlClient;
	using Microsoft.Extensions.Options;
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Linq.Dynamic.Core;
	using System.Reflection;
	using System.Text;
	using System.Threading.Tasks;

	public class Servicio<T> : IServicio<T> where T : EntidadBase
	{
		protected readonly IUnitOfWork _uow;
		protected readonly IRepository<T> _repository;
		protected readonly PaginationOptions? _pagSet;
		protected readonly ConfigNegocioOption? _configTradeOption;

		public Servicio(IUnitOfWork uow, IOptions<PaginationOptions> options)
		{
			_uow = uow;
			_repository = _uow.GetRepository<T>();
			_pagSet = options.Value;
		}

		public Servicio(IUnitOfWork uow, IOptions<ConfigNegocioOption> options, IOptions<PaginationOptions> options2)
		{
			_uow = uow;
			_repository = _uow.GetRepository<T>();
			_configTradeOption = options.Value;
			_pagSet = options2.Value;
		}

		public Servicio(IUnitOfWork uow, IOptions<ConfigNegocioOption> options)
		{
			_uow = uow;
			_repository = _uow.GetRepository<T>();
			_configTradeOption = options.Value;
		}

		public Servicio(IUnitOfWork uow)
		{
			_uow = uow;
			_repository = _uow.GetRepository<T>();
		}

		public virtual T Find(object id)
		{
			if (id == null || id == default)
			{
				throw new NegocioException($"El Identificador de {typeof(T).Name} no es valido.");
			}

			object idd;

			if (id.GetType().ToString().Equals("object[]"))
			{
				idd = (id as object[])[0];
			}
			else
			{
				idd = id;
			}
			var entity = _repository.Find(idd);
			if (entity == null)
			{
				throw new NotFoundException($"No se encontró la información de {typeof(T).Name}.");
			}
			return entity;
		}

		public virtual async Task<T> FindAsync(object id)
		{
			if (id == null || id == default)
			{
				throw new NegocioException($"El Identificador de {typeof(T).Name} no es valido.");
			}

			object idd;
			if (id.GetType().ToString().Equals("object[]"))
			{
				idd = (id as object[])[0];
			}
			else
			{
				idd = id;
			}
			var entity = await _repository.FindAsync(idd);
			if (entity == null)
			{
				throw new NotFoundException($"No se encontró la información de {typeof(T).Name}.");
			}
			return entity;
		}

		public virtual PagedList<T> GetAll(QueryFilters filters)
		{
			//validando los parametros sensibles de filter
			if (_pagSet != null)
			{
				filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
				filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;
			}
			else
			{
				filters.Pagina = default;
				filters.Registros = default;
			}

			var entidades = GetAllIq();
			if (!string.IsNullOrWhiteSpace(filters.Sort) && !string.IsNullOrWhiteSpace(filters.SortDir))
			{
				entidades = entidades.OrderBy($"{filters.Sort} {filters.SortDir}");
			}

			var pagina = PagedList<T>.Create(entidades, filters.Pagina ?? 1, filters.Registros ?? 20);
			return pagina;
		}

		public virtual IQueryable<T> GetAllIq()
		{
			var entities = _repository.GetAll();
			return entities;
		}


		public virtual void Add(T item)
		{
			if (item == null)
			{
				throw new NegocioException($"No se recepcionaron los datos de {typeof(T).Name}.");
			}
			_repository.Add(item);
		}

		public virtual async Task<bool> AddAsync(T item)
		{
			if (item == null)
			{
				throw new NegocioException($"No se recepcionaron los datos de {typeof(T).Name}.");
			}
			await _repository.AddAsync(item);
			var res = await _uow.SaveChangesAsync();
			return res > 0;
		}

		public virtual async Task<bool> Update(T item)
		{
			if (item == null)
			{
				throw new NegocioException($"No se recepcionaron los datos de {typeof(T).Name}.");
			}
			_repository.Update(item);
			var res = await _uow.SaveChangesAsync();
			return res > 0;
		}

		public virtual async Task<bool> Delete(object id)
		{
			if (id == default)
			{
				throw new NegocioException($"El Identificador de {typeof(T).Name} no es valido.");
			}

			var item = _repository.Find(id);
			if (item == null)
			{
				throw new NotFoundException($"No se pudo encontrar la información de la entidad {typeof(T).Name}.");
			}
			_repository.Remove(item);
			var result = await _uow.SaveChangesAsync();
			return result > 0;
		}

		public List<T> EjecutarSP(string? sp, params object[] parametros)
		{
			return _repository.EjecutarSP(sp, parametros);
		}

		public int InvokarSpNQuery(string sp, List<SqlParameter> parametros, bool esTransacciona = false, bool elUltimo = true)
		{
			return _repository.InvokarSpNQuery(sp, parametros, esTransacciona, elUltimo);
		}

		public object InvokarSpScalar(string sp, List<SqlParameter> parametros, bool esTransacciona = false, bool elUltimo = true)
		{
			return _repository.InvokarSpScalar(sp, parametros, esTransacciona, elUltimo);
		}

		protected string ConvertirTXT2B64(StringBuilder sb)
		{
			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms))
				{
					writer.Write(sb.ToString());
					writer.Flush();
					var bytes = ms.ToArray();
					return Convert.ToBase64String(bytes);
				}
			}
		}

		protected string GeneraFileXLS<S>(List<S> registros, List<string> _titulos, List<string> _campos,
			 string nombreHoja = "Datos", string formatoFecha = "dd/MM/yy") where S : class
		{
			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add(nombreHoja);

				if (registros == null || !registros.Any()) return string.Empty;

				var reg = registros.First();
				var properties = reg.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
									 .Where(p => _campos.Contains(p.Name))
									 .ToList();

				// Estilo para bordes
				Action<IXLCell> aplicarBordes = cell =>
				{
					cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
					cell.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
				};

				// Cargar cabecera
				for (int col = 0; col < _titulos.Count; col++)
				{
					var cell = worksheet.Cell(1, col + 1);
					cell.Value = _titulos[col];
					cell.Style.Font.Bold = true;
					cell.Style.Fill.BackgroundColor = XLColor.LightGray;
					aplicarBordes(cell);
				}

				// Cargar datos
				int fila = 2;
				foreach (var item in registros)
				{
					int col = 0;
					foreach (var prop in properties)
					{
						var valor = prop.GetValue(item);
						var cell = worksheet.Cell(fila, col + 1);

						if (valor == null)
						{
							cell.Value = "-";
						}
						else if (valor is DateTime fecha)
						{
							cell.Value = fecha;
							cell.Style.DateFormat.Format = formatoFecha;
						}
						else if (valor is int or long or short or byte)
						{
							cell.Value = Convert.ToInt64(valor);
						}
						else if (valor is decimal or double or float)
						{
							cell.Value = Convert.ToDecimal(valor);
							cell.Style.NumberFormat.Format = "#,##0.00";
						}
						else if (valor is bool booleano)
						{
							cell.Value = booleano ? "Sí" : "No";
						}
						else
						{
							cell.Value = valor.ToString();
						}

						aplicarBordes(cell);
						col++;
					}

					// Alternar color de fondo
					if ((fila % 2) == 0)
					{
						worksheet.Row(fila).Style.Fill.BackgroundColor = XLColor.AliceBlue; //XLColor.FromHtml("#F5F5F5");
					}

					fila++;
				}

				worksheet.Columns().AdjustToContents();

				return ConvertirWorkBook2B64(workbook);
			}
		}

		protected string GeneraTXT<S>(List<S> registros, List<string> _campos,
			string? culturaNombre = null, string? formatoFecha = null)
		{
			// Convertir los datos a TXT
			var sb = new StringBuilder();
			var cultura = culturaNombre == null ? CultureInfo.CurrentCulture : CultureInfo.GetCultureInfo(culturaNombre);

			foreach (var item in registros)
			{
				var valores = item.GetType().GetProperties()
					.Where(x => _campos.Contains(x.Name))
					.Select(p =>
					{
						var valor = p.GetValue(item, null);
						if (valor == null) return string.Empty;
						if (formatoFecha != null && valor is DateTime fecha)
							return fecha.ToString(formatoFecha, cultura);
						if (culturaNombre != null && valor is decimal or double or float)
							return Convert.ToDecimal(valor).ToString("N2", cultura);
						return valor.ToString() ?? string.Empty;
					});

				sb.AppendLine(string.Join("\t|", valores));
			}

			return ConvertirTXT2B64(sb);
		}

		protected string ConvertirWorkBook2B64(XLWorkbook workbook)
		{
			using (var stream = new MemoryStream())
			{
				workbook.SaveAs(stream);
				var bytes = stream.ToArray();
				return Convert.ToBase64String(bytes);
			}
		}

		protected DatosCuenta CargaDatosCliente(CuentaDto cta)
		{
			var datos = new DatosCuenta
			{
				CtaId = cta.Cta_Id,
				RazonSocial = cta.Cta_Denominacion,
				Domicilio = cta.Cta_Domicilio,
				CUIT = cta.Cta_Documento,
				Contacto = $"Te: {cta.Cta_Te} - Cel: {cta.Cta_Celu}.",

			};
			return datos;
		}

		protected PdfPTable GeneraCabeceraPdf(ReporteSolicitudDto solicitud, Image logo, Font chico, Font titulo, EmpresaGeco _empresaGeco)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(4, [7f, 20f, 58f, 15f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo = HelperPdf.GeneraCelda(logo, false);
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} s:{solicitud.Administracion}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"IIBB: {_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"Dirección: {_empresaGeco.Direccion}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe
			subTabla = new PdfPTable(1);

			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};
			subTabla.AddCell(celdaTitulo);
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				celdaTitulo = new PdfPCell(new Phrase(solicitud.SubTitulo, titulo))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 5f
				};
				subTabla.AddCell(celdaTitulo);
			}


			tabla.AddCell(subTabla);

			// Columna 4: Fecha
			string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaFechaHora);
			return tabla;
		}

		protected PdfPTable GeneraCabeceraPdf3C(ReporteSolicitudDto solicitud, Font chico, Font titulo, Image? logo, EmpresaGeco _empresaGeco)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 20f, 70f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", titulo));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} s:{solicitud.Administracion}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"IIBB: {_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"Dirección: {_empresaGeco.Direccion}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe y Fecha
			PdfPTable subTablaC3 = new(1);
			subTablaC3.WidthPercentage = 100;

			// Título del informe
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};

			PdfPCell celdaSubTitulo = new();
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				// Título del informe
				celdaSubTitulo = new PdfPCell(new Phrase(solicitud.SubTitulo, titulo))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 10f
				};
			}

			// Fecha
			string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};

			// Datos apilados
			subTablaC3.AddCell(celdaFechaHora);
			subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
			if (string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				subTablaC3.AddCell(celdaTitulo);
				subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
			}
			else
			{
				subTablaC3.AddCell(celdaTitulo);
				subTablaC3.AddCell(celdaSubTitulo);
			}

			PdfPCell celdaSubTablaC3 = new PdfPCell(subTablaC3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTablaC3);

			return tabla;
		}

		protected PdfPTable GeneraCabeceraPDF2(ReporteSolicitudDto solicitud, Font chico, Font titulo, Image? logo, EmpresaGeco _empresaGeco)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(4, [10f, 20f, 50f, 20f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", titulo));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Responsabilidad} Ini.Act:{_empresaGeco.InicioActividades.ToShortDateString()}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} IB:{_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Direccion}, {_empresaGeco.Localidad}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};
			PdfPCell celdaSubTitulo = new();
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				// Título del informe
				celdaSubTitulo = new PdfPCell(new Phrase(solicitud.SubTitulo, titulo))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = Element.ALIGN_CENTER,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 10f
				};
			}
			PdfPTable subTablaC3 = new(1);
			subTablaC3.WidthPercentage = 100;
			subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
			subTablaC3.AddCell(celdaTitulo);
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
				subTablaC3.AddCell(celdaSubTitulo);
			}

			PdfPCell celdaSubTablaC3 = new PdfPCell(subTablaC3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTablaC3);

			// Columna 4: Fecha
			string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaFechaHora);
			return tabla;
		}

		protected PdfPTable GeneraCabeceraPDF2_ParaRemito(ReporteSolicitudDto solicitud, Font chico, Font chicoBold, Font normal, Font normalBold, Font titulo, Font tituloBig, Image? logo, EmpresaGeco empresa, RemitoNoFiscalDto remito, int titHorAlign = Element.ALIGN_LEFT, int subtitHorAlign = Element.ALIGN_LEFT)
		{
			// Tabla principal: 3 columnas
			PdfPTable tabla = HelperPdf.GeneraTabla(3, new float[] { 45f, 10f, 45f }, 100, 0, 0);

			// ============================================================
			// COLUMNA 1 — Datos de la empresa
			// ============================================================

			PdfPTable col1 = new PdfPTable(1);
			col1.WidthPercentage = 100;
			col1.DefaultCell.Padding = 0f;
			col1.SpacingBefore = 0f;
			col1.SpacingAfter = 0f;

			col1.AddCell(HelperPdf.CrearCeldaTexto(remito.emisor_nombre, titulo));
			col1.AddCell(HelperPdf.CrearCeldaTexto($"{remito.emisor_domicilio.Trim()}", normalBold));
			col1.AddCell(HelperPdf.CrearCeldaTexto($"{remito.emisor_afip_desc}", normalBold));
			col1.AddCell(HelperPdf.CrearCeldaTexto("", normal));
			col1.AddCell(HelperPdf.CrearCeldaTexto("", normal));

			PdfPCell celdaCol1 = new PdfPCell(col1)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_LEFT,
				VerticalAlignment = Element.ALIGN_TOP,
				Padding = 0f
			};

			tabla.AddCell(celdaCol1);

			// ============================================================
			// COLUMNA 2 — R grande + Cod. 91 (alineado arriba y superpuesto)
			// ============================================================

			PdfPTable col2 = new PdfPTable(1);
			col2.WidthPercentage = 100;
			col2.SpacingBefore = 0f;
			col2.SpacingAfter = 0f;

			var fuenteR = HelperPdf.DefineFontWithStyle("Arial", 18, Font.BOLD, 0, 0, 0);

			// R grande dentro de recuadro
			PdfPCell celdaR = new PdfPCell(new Phrase("R", fuenteR))
			{
				Border = Rectangle.BOX,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f,
				PaddingBottom = 10f
			};
			col2.AddCell(celdaR);

			// Cod. 91 superpuesto sobre el borde inferior del recuadro
			PdfPCell celdaCod = new PdfPCell(new Phrase("Cod. 91", chicoBold))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,

				// 🔥 SUPERPOSICIÓN REAL
				PaddingTop = -6f,     // sube el texto hacia el borde inferior del recuadro
				PaddingBottom = 0f,
				
				FixedHeight = 12f     // evita que la celda empuje hacia abajo
			};
			col2.AddCell(celdaCod);

			// Celda contenedora de la columna 2
			PdfPCell celdaCol2 = new PdfPCell(col2)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,

				// 🔥 ALINEADO ARRIBA
				VerticalAlignment = Element.ALIGN_TOP
			};

			tabla.AddCell(celdaCol2);

			// ============================================================
			// COLUMNA 3 — Datos del remito
			// ============================================================

			PdfPTable col3 = new PdfPTable(1);
			col3.WidthPercentage = 100;

			col3.AddCell(HelperPdf.CrearCeldaTexto($"N° Remito: {remito.cm_compte}", tituloBig, Element.ALIGN_RIGHT));
			col3.AddCell(HelperPdf.CrearCeldaTexto("Fecha: ", normal, DateTime.Now.ToString("dd/MM/yyyy"), normalBold, " DOCUMENTO NO VÁLIDO COMO FACTURA", chico, Element.ALIGN_RIGHT));
			col3.AddCell(HelperPdf.CrearCeldaTexto("CUIT: ", normal, remito.emisor_cuit, normalBold, Element.ALIGN_RIGHT, 118f));
			col3.AddCell(HelperPdf.CrearCeldaTexto("IB: ", normal, remito.emisor_ib_nro, normalBold, Element.ALIGN_RIGHT, 118f));
			col3.AddCell(HelperPdf.CrearCeldaTexto("Inicio Act.: ", normal, remito.emisor_fecha_ini.ToString("dd/MM/yyyy"), normalBold, Element.ALIGN_RIGHT, 125f));

			PdfPCell celdaCol3 = new PdfPCell(col3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_TOP
			};

			tabla.AddCell(celdaCol3);

			PdfPTable tablaContenedora = new PdfPTable(1);
			tablaContenedora.WidthPercentage = 100;

			// borde alrededor de toda la cabecera
			PdfPCell celdaContenedora = new PdfPCell(tabla)
			{
				Border = Rectangle.BOX,
				Padding = 1f,
				BorderWidth = 2f
			};

			tablaContenedora.AddCell(celdaContenedora);

			return tablaContenedora;

		}


		protected PdfPTable GeneraCabeceraPDF2_NoFecha(ReporteSolicitudDto solicitud, Font chico, Font titulo, Font tituloBig, Image? logo, EmpresaGeco _empresaGeco, int titHorAlign = 1, int subtitHorAlign = 1)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 20f, 70f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", tituloBig));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Responsabilidad} Ini.Act:{_empresaGeco.InicioActividades.ToShortDateString()}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} IB:{_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Direccion}, {_empresaGeco.Localidad}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, tituloBig))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = titHorAlign,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};
			PdfPCell celdaSubTitulo = new();
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				// Título del informe
				celdaSubTitulo = new PdfPCell(new Paragraph(solicitud.SubTitulo, titulo))
				{
					Border = Rectangle.NO_BORDER,
					HorizontalAlignment = subtitHorAlign,
					VerticalAlignment = Element.ALIGN_MIDDLE,
					PaddingTop = 10f
				};
			}

			// Datos apilados
			// Columna 3: Título del informe y Fecha
			PdfPTable subTablaC3 = new(1);
			subTablaC3.WidthPercentage = 100;
			subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
			subTablaC3.AddCell(celdaTitulo);
			if (!string.IsNullOrEmpty(solicitud.SubTitulo))
			{
				subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));
				subTablaC3.AddCell(celdaSubTitulo);
			}

			PdfPCell celdaSubTablaC3 = new PdfPCell(subTablaC3)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTablaC3);

			return tabla;
		}

		protected static PdfPCell CeldaTexto(object valor, Font fuente, BaseColor fondo, bool aplicarFormatoNumerico = true)
		{
			string texto = valor?.ToString() ?? "-";
			int alineacion = Element.ALIGN_LEFT;
			var cultura = new CultureInfo("es-ES");

			if (aplicarFormatoNumerico && decimal.TryParse(texto, NumberStyles.Any, cultura, out decimal valorDecimal))
			{
				texto = valorDecimal.ToString("N2", cultura);
				alineacion = Element.ALIGN_RIGHT;
			}

			var parrafo = new Paragraph(texto, fuente)
			{
				Alignment = alineacion
			};

			return new PdfPCell(parrafo)
			{
				BackgroundColor = fondo,
				Border = Rectangle.BOX,
				HorizontalAlignment = alineacion,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				Padding = 4
			};
		}

		protected static PdfPCell HeaderCell(string texto, int colspan, Font fuente, BaseColor? fondo = null)
		{
			var celda = new PdfPCell(new Phrase(texto, fuente))
			{
				Colspan = colspan,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				BackgroundColor = fondo ?? BaseColor.LightGray,
				Border = Rectangle.BOX,
				Padding = 5
			};
			return celda;
		}

		protected static PdfPCell BlankCell(int colspan)
		{
			return new PdfPCell(new Phrase(""))
			{
				Colspan = colspan,
				Border = Rectangle.NO_BORDER
			};
		}

		protected void CargarCabecera(ReporteSolicitudDto solicitud, Font chico, Font titulo, Image? logo, EmpresaGeco _empresaGeco, out PdfPTable tabla, out Phrase phrase)
		{
			tabla = HelperPdf.GeneraTabla(4, [10f, 20f, 50f, 20f], 100, 10, 20);

			// Columna 1: Logo
			PdfPCell celdaLogo;
			if (logo == null)
			{
				celdaLogo = new PdfPCell(new Paragraph("CA", titulo));
			}
			else
			{
				celdaLogo = HelperPdf.GeneraCelda(logo, false);
			}
			tabla.AddCell(celdaLogo);

			// Columna 2: Datos apilados y título
			PdfPTable subTabla = new PdfPTable(1);
			subTabla.WidthPercentage = 100;

			// Datos apilados
			subTabla.AddCell(HelperPdf.CrearCeldaTexto(_empresaGeco.Nombre, chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Responsabilidad} Ini.Act:{_empresaGeco.InicioActividades.ToShortDateString()}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"CUIT: {_empresaGeco.CUIT} IB:{_empresaGeco.IngresosBrutos}", chico));
			subTabla.AddCell(HelperPdf.CrearCeldaTexto($"{_empresaGeco.Direccion}, {_empresaGeco.Localidad}", chico));

			PdfPCell celdaSubTabla = new PdfPCell(subTabla)
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaSubTabla);

			// Columna 3: Título del informe
			PdfPCell celdaTitulo = new PdfPCell(new Phrase(solicitud.Titulo, titulo))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_CENTER,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};
			tabla.AddCell(celdaTitulo);

			// Columna 4: Fecha
			string fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
			PdfPCell celdaFechaHora = new PdfPCell(new Phrase(fechaHora, chico))
			{
				Border = Rectangle.NO_BORDER,
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE
			};
			tabla.AddCell(celdaFechaHora);

			// Convertir la tabla en un Phrase
			phrase = new Phrase();
			phrase.Add(tabla);
		}

	}
}
