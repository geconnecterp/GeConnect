using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.EntidadesComunes.ControlComun.CuentaComercial;
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

		/// <summary>
		/// Se obtienen los proveedores. Tener en cuenta que se invocan con parametro ope_iva = 'BI'
		/// </summary>
		/// <returns>Se obtiene la lista de proveedores</returns>
		public List<ProveedorLista> GetProveedorLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PROVEEDOR_LISTA;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@ope_iva","BI")
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
	}
}
