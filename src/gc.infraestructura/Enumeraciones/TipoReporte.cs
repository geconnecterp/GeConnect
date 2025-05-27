using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.infraestructura.Enumeraciones
{

    /// <summary>
    /// Enumeración para los tipos de reportes disponibles en el sistema.
    /// Consultarnos antes de ingresar para evitar solapar numeración
    /// </summary>
    public enum InfoReporte
    {
        R001_InfoCtaCte = 1,
        R002_InfoVenc = 2,
        R003_InfoCmpte = 3,
        R004_InfoCmpteDet=4,
        R005_InfoOPago=5,
        R006_InfoOPagoDet=6,
        R007_InfoRecProv=7,
        R008_InfoRecProvDet=8,
        R009_InfoErrAsTemp = 9,
        R010_InfoAsTemp = 10,
    }
}
