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

        protected string GeneraFileXLS<S>(List<S> registros, List<string> _titulos, List<string> _campos) where S : class
        {

            // Convertir los datos a Excel
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");

                // Obtener las propiedades públicas del tipo de elemento
                var reg = registros.First();
                var properties = reg.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                //carga cabecera
                int i = 0;
                foreach (var item in _titulos)
                {
                    worksheet.Cell(1, i + 1).Value = item;
                    i++;
                }

                // Agregar datos
                int fila = 2;
                foreach (var item in registros)
                {
                    i = 0;
                    foreach (PropertyInfo prop in properties)
                    {
                        if (!_campos.Contains(prop.Name)) { continue; }

                        var valor = prop.GetValue(item);
                        worksheet.Cell(fila, i + 1).Value = valor != null ? valor.ToString() : string.Empty;
                        i++;
                    }

                    fila++;
                }

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
                Contacto = $"Te: {cta.Cta_Te} - Cel: {cta.Cta_Celu}." ,

            };
            return datos;
        }

        //public class CustomPdfPageEventHelper : PdfPageEventHelper
        //{
        //    private readonly string _footerText;
        //    public CustomPdfPageEventHelper(string footerText)
        //    {
        //        _footerText = footerText;
        //    }

        //    public override void OnEndPage(PdfWriter writer, Document document)
        //    {
        //        PdfPTable footerTable = new PdfPTable(2);
        //        footerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
        //        footerTable.DefaultCell.Border = Rectangle.NO_BORDER;

        //        // Add footer text
        //        PdfPCell cell = new PdfPCell(new Phrase(_footerText, new Font(Font.HELVETICA, 8, Font.NORMAL)));
        //        cell.Border = Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = Element.ALIGN_LEFT;
        //        footerTable.AddCell(cell);

        //        // Add page number
        //        cell = new PdfPCell(new Phrase("Página " + writer.PageNumber, new Font(Font.HELVETICA, 8, Font.NORMAL)));
        //        cell.Border = Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
        //        footerTable.AddCell(cell);

        //        footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 20, writer.DirectContent);
        //    }
        //}
    }
}
