using gc.infraestructura.EntidadesComunes.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface ICajaInitServicio
    {
        (bool, string) ValidarDatosIniciales(CajaSettings caja);
        
    }
}
