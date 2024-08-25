namespace gc.api.core.Entidades
{
    public partial class Deposito : EntidadBase
    {
        public Deposito()
        {
            Depo_Id = string.Empty;
            Depo_Nombre = string.Empty;
            Adm_Id = string.Empty;
        }

        public string Depo_Id { get; set; }
        public string Depo_Nombre { get; set; }
        public char Depo_Pvta { get; set; }
        public char Depo_Dev_Prov { get; set; }
        public char Depo_Pelab { get; set; }
        public char Depo_Re_Prov { get; set; }
        public char Depo_Ri { get; set; }
        public string Adm_Id { get; set; }
        public char? Depo_Activa { get; set; }


        //public virtual ICollection<box> Boxs { get; set; }
        //public virtual ICollection<productos_depositos> Productos_Depositoss { get; set; }
        //public virtual ICollection<stk_movimientos> Stk_Movimientoss { get; set; }

    }
}
