namespace gc.infraestructura.Dtos.Almacen
{
    public partial class RubroDto:Dto
    {
        public RubroDto()
        {
            Rub_Id = string.Empty;
            Rub_Desc = string.Empty;
            Rubg_Id = string.Empty;
        }
        public string Rub_Id { get; set; }
        public string Rub_Desc { get; set; }
        public string Rubg_Id { get; set; }
        public char Rub_Feteado { get; set; }
        public char Rub_Ctlstk { get; set; }
        public char Rub_Actu { get; set; }

    }
}
