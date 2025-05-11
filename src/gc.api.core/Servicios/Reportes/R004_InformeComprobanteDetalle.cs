using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R004_InformeComprobanteDetalle : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;

        public R004_InformeComprobanteDetalle(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Fecha","Tipo", "N° Cmpte", "Neto", "IVA", "TOTAL", "N° Or.Pago",  "CARGADO", "USUARIO" };
            _campos = new List<string> { "Cm_fecha", "Tco_id", "Cm_compte", "Cm_neto", "Cm_iva", "Cm_total", "Op_compte", "Cm_fecha_carga", "Usu_id" };
        }

        public string Generar(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }

        public string GenerarTxt(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }

        public string GenerarXls(ReporteSolicitudDto solicitud)
        {
            throw new NotImplementedException();
        }
    }
}
