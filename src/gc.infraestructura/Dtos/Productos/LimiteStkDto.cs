namespace gc.infraestructura.Dtos.Productos
{
    public class LimiteStkDto
    {
        public string P_Id { get; set; }=string.Empty;
        public string P_Stk_Min { get; set; }=string.Empty ;
        public string P_Stk_Max { get; set; } = string.Empty;
        public string Adm_Id { get; set; } = string.Empty;
        public string Adm_Nombre { get; set; } = string.Empty;
        public string Adm_Lista { get; set; } = string.Empty;
    }
}
