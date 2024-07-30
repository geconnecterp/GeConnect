using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios
{
    public interface IRubroServicio : IServicio<Rubro>
    {
        List<RubroListaDto> GetRubroLista();
    }
}
