using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using iTextSharp.text;
using log4net.Core;

namespace gc.api.core.Servicios.Reportes
{
    public class R002_InformeVencimiento : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        public R002_InformeVencimiento(IUnitOfWork uow, IConsultaServicio consulta) : base(uow)
        {
            _consultaServicio = consulta;

        }
        public string Generar(ReporteSolicitudDto solicitud)
        {
           throw new NotImplementedException();
        }
    }
}
