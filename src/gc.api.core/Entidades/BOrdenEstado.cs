namespace gc.api.core.Entidades
{
    public partial class BOrdenEstado : EntidadBase
    {
        public BOrdenEstado()
        {
            Boe_id = string.Empty;
            Boe_desc = string.Empty;
        }

        public string Boe_id { get; set; }
        public string Boe_desc { get; set; }


        public virtual ICollection<BilleteraOrden> Billeteras_ordeness { get; set; }

    }
}
