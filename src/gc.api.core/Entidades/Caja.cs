using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public partial class Caja : EntidadBase
    {
        public Caja()
        {
            Caja_Id = string.Empty;
            Adm_Id = string.Empty;
            Depo_Id = string.Empty;
            Dia_Movi = string.Empty;
            Usu_Id = string.Empty;
            Caja_Nombre = string.Empty;
            Caja_Modalidad = string.Empty;
            Caja_Maquina = string.Empty;
            Caja_Nro_Proceso = string.Empty;
        }

        public string Caja_Id { get; set; }
        public string Adm_Id { get; set; }
        public string Depo_Id { get; set; }
        public string? Dia_Movi { get; set; }
        public short? Ctrl_Id { get; set; }
        public string? Usu_Id { get; set; }
        public string Caja_Nombre { get; set; }
        public char Caja_Estado { get; set; }
        public char Caja_Habilitadas { get; set; }
        public string Caja_Modalidad { get; set; }
        public DateTime? Caja_Apertura { get; set; }
        public DateTime? Caja_Cierre { get; set; }
        public string? Caja_Maquina { get; set; }
        public string? Caja_Nro_Proceso { get; set; }
        public short? Caja_Nro_Cierre { get; set; }
        public int Caja_Nro_Operacion { get; set; }
        public char Caja_Activa { get; set; }
        public char Caja_Manual { get; set; }
        public char Caja_Actu { get; set; }
        public string? Caja_Mepa_Categoria { get; set; }
        public string? Caja_Mepa_Id { get; set; }

        //public virtual ICollection<cajas_canales_ope> Cajas_Canales_Opes { get; set; }
        //public virtual ICollection<cajas_fondos> Cajas_Fondoss { get; set; }
        //public virtual ICollection<cajas_numeraciones> Cajas_Numeracioness { get; set; }

    }
}
