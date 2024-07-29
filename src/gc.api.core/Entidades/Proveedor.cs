using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class Proveedor : EntidadBase
    {
        public Proveedor()
        {
            Cta_Id = string.Empty;
            Ctap_Ean = string.Empty;
            Ctap_Id_Externo = string.Empty;
            Ctap_Viajante = string.Empty;
            Ctap_Viajante_Ce = string.Empty;
            Ctap_Viajante_Email = string.Empty;
            Ctap_Valores_A_Nombre = string.Empty;
            Rgan_Id = string.Empty;
            Ope_Iva = string.Empty;
            Ctag_Id = string.Empty;
            Ctap_Obs_Op = string.Empty;
            Ctap_Obs_Precios = string.Empty;
            Id_Old = string.Empty;
        }

        public string Cta_Id { get; set; }
        public char? Tp_Id { get; set; }
        public char? Fp_Id { get; set; }
        public short? Fp_Dias { get; set; }
        public string? Ctap_Ean { get; set; }
        public string? Ctap_Id_Externo { get; set; }
        public string? Ctap_Viajante { get; set; }
        public string? Ctap_Viajante_Ce { get; set; }
        public string? Ctap_Viajante_Email { get; set; }
        public string? Ctap_Valores_A_Nombre { get; set; }
        public short? Ctap_Rp_Plazo_Compra { get; set; }
        public short? Ctap_Rp_Plazo_Entrega { get; set; }
        public char Ctap_Rgan { get; set; }
        public string? Rgan_Id { get; set; }
        public char Rgan_Cert { get; set; }
        public DateTime? Rgan_Cert_Vto { get; set; }
        public decimal Rgan_Porc { get; set; }
        public char Ctap_Rib { get; set; }
        public char? Rib_Id { get; set; }
        public char Rib_Cert { get; set; }
        public DateTime? Rib_Cert_Vto { get; set; }
        public decimal Rib_Porc { get; set; }
        public char? Ctap_Ret_Iva { get; set; }
        public decimal? Ctap_Ret_Iva_Porc { get; set; }
        public char Ctap_Per_Iva { get; set; }
        public decimal Ctap_Per_Iva_Ali { get; set; }
        public char Ctap_Per_Ib { get; set; }
        public decimal Ctap_Per_Ib_Ali { get; set; }
        public char? Ctap_Pago_Susp { get; set; }
        public char Ctap_Devolucion { get; set; }
        public char? Ctap_Devolucion_Flete { get; set; }
        public char? Ctap_Acuenta_Dev { get; set; }
        public decimal? Ctap_D1 { get; set; }
        public decimal? Ctap_D2 { get; set; }
        public decimal? Ctap_D3 { get; set; }
        public decimal? Ctap_D4 { get; set; }
        public decimal? Ctap_D5 { get; set; }
        public decimal? Ctap_D6 { get; set; }
        public string Ope_Iva { get; set; }
        public string? Ctag_Id { get; set; }
        public string? Ctap_Obs_Op { get; set; }
        public string? Ctap_Obs_Precios { get; set; }
        public char Ctap_Habilitada { get; set; }
        public string? Id_Old { get; set; }
        

        //public virtual ICollection<proveedores_grupos> Proveedores_Gruposs { get; set; }

    }
}
