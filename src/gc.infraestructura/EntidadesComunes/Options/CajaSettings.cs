using gc.infraestructura.Dtos.Cajas;

namespace gc.infraestructura.EntidadesComunes.Options
{
    public class CajaSettings
    {
        public string CajaId { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public TipoFact Facturacion { get; set; }
        public TipoCnnCF TipoCnnCF { get; set; }
        public string AdmId { get; set; } = string.Empty; //Sucursal
        public bool acumula { get; set; } = false;
        public CajaDatosDto Caja { get; set; } = new();       
    }

    public enum TipoAPI
    {
        AFIP = 1,
        Clover = 2,
        PayWey=3,
        MePa=4
    }
    public enum TipoFact
    {
        FE = 1,
        CF = 2
    }
    public enum TipoCnnCF
    {
        NONE = 0,
        IP = 1,
        USB = 2
    }
}
