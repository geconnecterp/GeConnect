namespace gc.infraestructura.Dtos.General
{
    public partial class ProductoDepositoDto
    {
        public ProductoDepositoDto()
        {
            P_Id = string.Empty;
            Box_Id = string.Empty;
            Depo_Id = string.Empty;
            Ps_Fv = string.Empty;
        }
        public string P_Id { get; set; }
        public string Box_Id { get; set; }
        public string Depo_Id { get; set; }
        public decimal Ps_Stk { get; set; }
        public decimal Ps_Stk_B { get; set; }
        public string Ps_Fv { get; set; }
        public char Ps_Actu { get; set; }
        public decimal Ps_Transito { get; set; }

    }
}
