using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class Cuenta : EntidadBase
    {
        public Cuenta()
        {
            Cta_Id = string.Empty;
            Cta_Denominacion = string.Empty;
            Tdoc_Id = string.Empty;
            Cta_Documento = string.Empty;
            Cta_Domicilio = string.Empty;
            Cta_Localidad = string.Empty;
            Cta_Cpostal = string.Empty;
            Dep_Id = string.Empty;
            Cta_Te = string.Empty;
            Cta_Celu = string.Empty;
            Cta_Email = string.Empty;
            Cta_Www = string.Empty;
            Afip_Id = string.Empty;
            Nj_Id = string.Empty;
            Cta_Ib_Nro = string.Empty;
            Cta_Bco_Cuenta_Nro = string.Empty;
            Cta_Bco_Cuenta_Cbu = string.Empty;
            Cta_Obs = string.Empty;
            Cta_Emp_Legajo = string.Empty;
            Cta_Emp_Ctaf = string.Empty;

            Proveedores = new HashSet<Proveedor>();
        }

        public string Cta_Id { get; set; }
        public string Cta_Denominacion { get; set; }
        public string? Tdoc_Id { get; set; }
        public string? Cta_Documento { get; set; }
        public string? Cta_Domicilio { get; set; }
        public string? Cta_Localidad { get; set; }
        public string? Cta_Cpostal { get; set; }
        public char? Prov_Id { get; set; }
        public string? Dep_Id { get; set; }
        public string? Cta_Te { get; set; }
        public string? Cta_Celu { get; set; }
        public string? Cta_Email { get; set; }
        public string? Cta_Www { get; set; }
        public string? Afip_Id { get; set; }
        public string? Nj_Id { get; set; }
        public string? Cta_Ib_Nro { get; set; }
        public char? Cta_Ib_Regimen { get; set; }
        public char? Tcb_Id { get; set; }
        public string? Cta_Bco_Cuenta_Nro { get; set; }
        public string? Cta_Bco_Cuenta_Cbu { get; set; }
        public DateTime? Cta_Alta { get; set; }
        public string? Cta_Obs { get; set; }
        public DateTime? Cta_Cuit_Vto { get; set; }
        public char Cta_Emp { get; set; }
        public string? Cta_Emp_Legajo { get; set; }
        public string? Cta_Emp_Ctaf { get; set; }
        public DateTime? Cta_Actu_Fecha { get; set; }
        public char? Cta_Actu { get; set; }


        //public virtual ICollection<clientes> Clientess { get; set; }
        //public virtual ICollection<ctacte> Ctactes { get; set; }
        //public virtual ICollection<cuentas_cortesias> Cuentas_Cortesiass { get; set; }
        //public virtual ICollection<cuentas_notas> Cuentas_Notass { get; set; }
        public virtual ICollection<Proveedor> Proveedores { get; set; }

    }
}
