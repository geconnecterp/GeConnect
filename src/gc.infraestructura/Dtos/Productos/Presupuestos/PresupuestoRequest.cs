using System.Data;
namespace gc.infraestructura.Dtos.Productos.Presupuestos
{
    public class PresupuestoRequest
    {
        public int Registros { get; set; }
        public int Pagina { get; set; }

        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public string? cli_list { get; set; }
        public string? pree_list { get; set; }
        public string? usu_list { get; set; }
        public string? adm_list { get; set; }
    }
}
