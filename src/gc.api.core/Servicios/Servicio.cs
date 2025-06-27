namespace gc.api.core.Servicios
{
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Threading.Tasks;
    using gc.api.core.Entidades;
    using gc.api.core.Interfaces.Datos;
    using gc.api.core.Contratos.Servicios;
    using gc.infraestructura.Core.EntidadesComunes.Options;
    using gc.infraestructura.Core.Exceptions;
    using gc.infraestructura.Core.EntidadesComunes;
    using Microsoft.Data.SqlClient;
    using iTextSharp.text.pdf;
    using iTextSharp.text;
    using System.Text;
    using ClosedXML.Excel;
    using gc.infraestructura.Dtos.Consultas;
    using System.Reflection;
    using gc.infraestructura.Dtos.DocManager;
    using gc.infraestructura.Dtos.Almacen;
    using gc.infraestructura.Dtos.Gen;
    using gc.infraestructura.EntidadesComunes.Options;
    using gc.infraestructura.Helpers;
    using System.Globalization;

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
             string nombreHoja = "Datos") where S : class
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
                            cell.Style.DateFormat.Format = "dd/MM/yy";
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

        protected string GeneraTXT<S>(List<S> registros, List<string> _campos)
        {
            // Convertir los datos a TXT
            var sb = new StringBuilder();

            foreach (var item in registros)
            {
                sb.AppendLine(string.Join("\t|", item.GetType().GetProperties().Where(x => _campos.Contains(x.Name)).Select(p => p.GetValue(item, null))));
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
            return tabla;
        }

		protected PdfPTable GeneraCabeceraPdf3C(ReporteSolicitudDto solicitud, Font chico, Font titulo, Image? logo, EmpresaGeco _empresaGeco)
		{
			PdfPTable tabla = HelperPdf.GeneraTabla(3, [10f, 25f, 65f], 100, 10, 20);
            
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
				HorizontalAlignment = Element.ALIGN_RIGHT,
				VerticalAlignment = Element.ALIGN_MIDDLE,
				PaddingTop = 10f
			};

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
			subTablaC3.AddCell(celdaTitulo);
			subTablaC3.AddCell(HelperPdf.CrearCeldaTexto(string.Empty, chico));

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
