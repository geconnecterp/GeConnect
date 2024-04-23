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

    public class Servicio<T> : IServicio<T> where T : EntidadBase
    {
        protected readonly IUnitOfWork _uow;
        protected readonly IRepository<T> _repository;
        protected readonly PaginationOptions? _paginationOptions;
        protected readonly ConfigNegocioOption? _configTradeOption;

        public Servicio(IUnitOfWork uow, IOptions<PaginationOptions> options)
        {
            _uow = uow;
            _repository = _uow.GetRepository<T>();
            _paginationOptions = options.Value;
        }

        public Servicio(IUnitOfWork uow, IOptions<ConfigNegocioOption> options, IOptions<PaginationOptions> options2)
        {
            _uow = uow;
            _repository = _uow.GetRepository<T>();
            _configTradeOption = options.Value;
            _paginationOptions = options2.Value;
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
            if (_paginationOptions != null)
            {
                filters.PageNumber = filters.PageNumber == default ? _paginationOptions.DefaultPageNumber : filters.PageNumber;
                filters.PageSize = filters.PageSize == default ? _paginationOptions.DefaultPageSize : filters.PageSize;
            }
            else
            {
                filters.PageNumber = default;
                filters.PageSize = default;
            }

            var entidades = GetAllIq();
            if (!string.IsNullOrWhiteSpace(filters.Sort) && !string.IsNullOrWhiteSpace(filters.SortDir))
            {
                entidades = entidades.OrderBy($"{filters.Sort} {filters.SortDir}");
            }

            var pagina = PagedList<T>.Create(entidades, filters.PageNumber, filters.PageSize);
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
    }
}
