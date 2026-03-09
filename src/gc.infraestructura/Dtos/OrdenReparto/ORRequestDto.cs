namespace gc.infraestructura.Dtos.OrdenReparto
{
    public class ORRequestDto:Dto
    {
        public bool HasFecha { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public bool HasEstado { get; set; }
        public char Ore_list { get; set; }
        public bool HasRepartidor { get; set; }
        public string RP_List { get; set; }= string.Empty;
        public bool HasId { get; set; }
        public string OR_Compte { get; set; } = string.Empty;

    }
}
