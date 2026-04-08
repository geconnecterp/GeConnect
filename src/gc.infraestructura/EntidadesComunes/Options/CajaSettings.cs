using gc.infraestructura.Dtos.Cajas;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Diagnostics;

namespace gc.infraestructura.EntidadesComunes.Options
{
    public class CajaSettings
    {
        public string CajaId { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public TipoFact Facturacion { get; set; }
        public TipoCnnCF TipoCnnCF { get; set; }
        public string AdmId { get; set; } = string.Empty; //Sucursal

        public CajaDatosDto Caja { get; set; } = new();
        //public string TipoCanal { get; set; } = string.Empty; //Tipo de Canal Comercial
        //public string LP { get; set; } = string.Empty; //Lista de Precios
        //public bool FactCF { get; set; } //Facturación Consumidor Final
        //public bool FactCR { get; set; } //Factiración Cliente Registrado
        //public bool Cobranza { get; set; }
        //public bool FactOR { get; set; } // Facturacion Ordenes de Reparto
        //public bool CobranzaOR { get; set; }
        //public bool Prefacturas { get; set; }
        //public TipoAPI APIExterna { get; set; }
        //public string NroProceso { get; set; } = string.Empty;  //caja_nro_proceso
        //public string NroCierre { get; set; } = string.Empty; //caja_nro_cierre
        //public string NroOperacion { get; set; } = string.Empty; //caja_nro_operacion
        //public string POSRelac { get; set; } = string.Empty;
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
