using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

using System.Linq.Dynamic.Core;

namespace gc.api.core.Servicios
{
    public class CuentaServicio : Servicio<Cuenta>, ICuentaServicio
    {
        public CuentaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
        {

        }

        public override PagedList<Cuenta> GetAll(QueryFilters filters)
        {
            filters.Pagina = filters.Pagina == default ? _pagSet.DefaultPageNumber : filters.Pagina;
            filters.Registros = filters.Registros == default ? _pagSet.DefaultPageSize : filters.Registros;

            var cuentass = GetAllIq();
            cuentass = cuentass.OrderBy($"{filters.Sort} {filters.SortDir}");

            if (!filters.Todo)
            {
                if (filters.Id != null && filters.Id != default)
                {
                    cuentass = cuentass.Where(r => r.Cta_Id == (string)filters.Id);
                }
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Denominacion.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Tdoc_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Documento.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Domicilio.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Localidad.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Cpostal.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Dep_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Te.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Celu.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Email.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Www.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Afip_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Nj_Id.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Ib_Nro.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Bco_Cuenta_Nro.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Bco_Cuenta_Cbu.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Obs.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Emp_Legajo.Contains(filters.Buscar));
            }

            if (!string.IsNullOrEmpty(filters.Buscar))
            {
                cuentass = cuentass.Where(r => r.Cta_Emp_Ctaf.Contains(filters.Buscar));
            }

            var paginas = PagedList<Cuenta>.Create(cuentass, filters.Pagina ?? 1, filters.Registros ?? 20);

            return paginas;
        }

        public List<ProveedorFamiliaListaDto> GetProveedorFamiliaLista(string ctaId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PROVEEDOR_FAMILIA_LISTA;
            var ps = new List<SqlParameter>()
            {
                new("@cta_id",ctaId)
            };
            var listaTemp = _repository.EjecutarLstSpExt<ProveedorFamiliaListaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<ProveedorGrupoDto> GetABMProveedorFamiliaLista(string ctaId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_PROV_FAMILIA_LISTA;
            var ps = new List<SqlParameter>()
            {
                new("@cta_id",ctaId)
            };
            var listaTemp = _repository.EjecutarLstSpExt<ProveedorGrupoDto>(sp, ps, true);
            return listaTemp;
        }

        public List<ProveedorGrupoDto> GetABMProveedorFamiliaDatos(string ctaId, string pgId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_PROV_FAMILIA_DATOS;
            var ps = new List<SqlParameter>()
            {
                new("@cta_id",ctaId),
                new("@pg_id",pgId)
            };
            var listaTemp = _repository.EjecutarLstSpExt<ProveedorGrupoDto>(sp, ps, true);
            return listaTemp;
        }

        /// <summary>
        /// sE OBTIENEN LOS CLIENTES QUE SON BUSCADOS A PARTIR DE 3 CARACTERES O DIGITOS.
        /// </summary>
        /// <param name="search">CARACTERES O DIGITOS A BUSCAR</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ClienteListaDto> GetClienteLista(string search)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CLIENTE_LISTA;

            //SE EVALUA SI EL DATO DE BUSQUEDA ES NUMERICO O ALFANUMERICO
            var esNum = search.ToIntOrNull();


            var ps = new List<SqlParameter>() {
                new SqlParameter("@search",search),
                new SqlParameter("@esNum",esNum!=null),
            };
            
            var res = _repository.EjecutarLstSpExt<ClienteListaDto>(sp, ps, true);

            if (res.Count == 0)
            {
                return new List<ClienteListaDto>();
            }
            else
            {
                return res.ToList();
            }
        }

        /// <summary>
        /// Se obtienen los proveedores. Tener en cuenta que se invocan con parametro ope_iva = 'BI'
        /// </summary>
        /// <returns>Se obtiene la lista de proveedores</returns>
        public List<ProveedorLista> GetProveedorLista(string ope_iva)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_PROVEEDOR_LISTA;
            var ps = new List<SqlParameter>() {
                new SqlParameter("@ope_iva",ope_iva)
            };
            var _repProdLista = _uow.GetRepository<ProveedorLista>();
            var res = _repProdLista.InvokarSp2Lst(sp, ps, true);

            if (res.Count == 0)
            {
                return new List<ProveedorLista>();
            }
            else
            {
                return res.ToList();
            }
        }

        public List<CuentaDto> GetCuentaComercialLista(string texto, char tipo)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_CUENTA_BUSQUEDA;
            var ps = new List<SqlParameter>()
            {
                    new("@busqueda",texto),
                    new("@busqueda_tipo",tipo)
            };
            var res = _repository.InvokarSp2Lst(sp, ps, true);
            if (res.Count == 0)
                return [];
            else
                return res.Select(x => new CuentaDto()
                {
                    #region Campos
                    Cta_Id = x.Cta_Id,
                    Cta_Denominacion = x.Cta_Denominacion,
                    Tdoc_Id = x.Tdoc_Id,
                    Cta_Documento = x.Cta_Documento,
                    Cta_Domicilio = x.Cta_Domicilio,
                    Cta_Localidad = x.Cta_Localidad,
                    Cta_Cpostal = x.Cta_Cpostal,
                    Prov_Id = x.Prov_Id,
                    Dep_Id = x.Dep_Id,
                    Cta_Te = x.Cta_Te,
                    Cta_Email = x.Cta_Email,
                    Cta_Www = x.Cta_Www,
                    Afip_Id = x.Afip_Id,
                    Nj_Id = x.Nj_Id,
                    Cta_Ib_Nro = x.Cta_Ib_Nro,
                    Cta_Ib_Regimen = x.Cta_Ib_Regimen,
                    Tcb_Id = x.Tcb_Id,
                    Cta_Bco_Cuenta_Nro = x.Cta_Bco_Cuenta_Nro,
                    Cta_Bco_Cuenta_Cbu = x.Cta_Bco_Cuenta_Cbu,
                    Cta_Alta = x.Cta_Alta,
                    Cta_Obs = x.Cta_Obs,
                    Cta_Emp = x.Cta_Emp,
                    Cta_Emp_Legajo = x.Cta_Emp_Legajo,
                    Tipo = x.Tipo,
                    Habilitada = x.Habilitada,
                    Tdoc_Desc = x.Tdoc_Desc,
                    Ib_id = x.Ib_id,
                    #endregion
                }).ToList();
        }

        public List<RPROrdenDeCompraDto> GetOCporCuenta(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_OC;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id",cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<RPROrdenDeCompraDto>(sp, ps, true);
            listaTemp.ForEach(x => x.oc_fecha = Convert.ToDateTime(x.oc_fecha).ToString("dd/MM/yyyy"));
            return listaTemp;
        }

        public List<RPROrdenDeCompraDetalleDto> GetDetalleDeOC(string oc_compte)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_OC_D;
            var ps = new List<SqlParameter>()
            {
                    new("@oc_compte",oc_compte)
            };
            var listaTemp = _repository.EjecutarLstSpExt<RPROrdenDeCompraDetalleDto>(sp, ps, true);
            listaTemp.ForEach(x => x.oc_fecha = Convert.ToDateTime(x.oc_fecha).ToString("dd/MM/yyyy"));
            return listaTemp;
        }

        public List<CuentaABMDto> GetCuentaParaABM(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_Datos;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaABMDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaFPDto> GetCuentaFormaDePago(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_FP_Lista;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaFPDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaContactoDto> GetCuentContactos(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_CONTACTOS_Lista;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaContactoDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaContactoDto> GetCuentContactosporCuentaYTC(string cta_id, string tc_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_CONTACTOS_Datos;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id),
                    new("@tc_id", tc_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaContactoDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaObsDto> GetCuentaObs(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_OBS_Lista;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaObsDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaObsDto> GetCuentaObsDatos(string cta_id, string to_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_OBS_Datos;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id),
                    new("@to_id", to_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaObsDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaNotaDto> GetCuentaNota(string cta_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_NOTA_Lista;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaNotaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaNotaDto> GetCuentaNotaDatos(string cta_id, string usu_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_NOTA_Datos;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id),
                    new("@usu_id", usu_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaNotaDto>(sp, ps, true);
            return listaTemp;
        }

        public List<CuentaFPDto> GetFormaDePagoPorCuentaYFP(string cta_id, string fp_id)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CLI_FP_Datos;
            var ps = new List<SqlParameter>()
            {
                    new("@cta_id", cta_id),
                    new("@fp_id", fp_id)
            };
            var listaTemp = _repository.EjecutarLstSpExt<CuentaFPDto>(sp, ps, true);
            return listaTemp;
        }

		public List<ComprobanteDeCompraDto> GetCompteDatosProv(string ctaId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_COMPTE_DATOS_PROV;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id",ctaId),
			};
			var listaTemp = _repository.EjecutarLstSpExt<ComprobanteDeCompraDto>(sp, ps, true);
			return listaTemp;
		}
		public List<RprAsociadosDto> GetCompteCargaRprAsoc(string ctaId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_COMPTE_CARGA_RPR;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id",ctaId),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RprAsociadosDto>(sp, ps, true);
			return listaTemp;
		}
		public List<NotasACuenta> GetCompteCargaCtaAsoc(string ctaId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_COMPTE_CARGA_A_CTA;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id",ctaId),
			};
			var listaTemp = _repository.EjecutarLstSpExt<NotasACuenta>(sp, ps, true);
			return listaTemp;
		}
		public List<RespuestaDto> CompteCargaConfirma(CompteCargaConfirmaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_COMPTE_CARGA_CONFIRMA;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id",request.cta_id),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
				new("@json_encabezado",request.json_encabezado),
				new("@json_concepto",request.json_concepto),
				new("@json_otro",request.json_otro),
				new("@json_relacion",request.json_relacion),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
