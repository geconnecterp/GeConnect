namespace gc.infraestructura.Dtos.Almacen
{
    public partial class DepositoDto:Dto
    {
        public DepositoDto()
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

    }
}
