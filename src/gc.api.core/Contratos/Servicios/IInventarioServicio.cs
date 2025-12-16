using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Inventario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios
{
	public interface IInventarioServicio : IServicio<Inventario>
	{
		List<InventarioDto> GetInventarioLista(GetInventarioListaRequest request);
	}
}
