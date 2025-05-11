using gc.api.core.Contratos.Servicios;
using gc.api.core.Contratos.Servicios.Reportes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Reportes
{
    public class R005_InformeOPago : Servicio<EntidadBase>, IGeneradorReporte
    {
        private readonly IConsultaServicio _consultaServicio;

        private readonly EmpresaGeco _empresaGeco;
        private readonly List<string> _titulos;
        private readonly List<string> _campos;

        public R005_InformeOPago(IUnitOfWork uow, IConsultaServicio consulta,
           IOptions<EmpresaGeco> empresa) : base(uow)
        {
            _consultaServicio = consulta;

            _empresaGeco = empresa.Value;
            _titulos = new List<string> { "Fecha", "N° Mov", "Tipo", "Comprobante", "Concepto", "Debe", "Haber", "Saldo" };
            _campos = new List<string> { "Cc_fecha", "Dia_movi", "Tco_desc", "Cm_compte", "Cc_concepto", "Cc_debe", "Cc_haber", "Cc_saldo" };
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
