
namespace gc.infraestructura.Dtos.Users.Request
{
	public class BuscarUsuarioRequest
	{
		public bool id { get; set; }
		public string id_d { get; set; } = "aaaaaaaaaa";
		public string id_h { get; set; } = "zzzzzzzzzz";
		public bool deno { get; set; }
		public string deno_like { get; set; } = "%";
		public int registros { get; set; } = 1000;
		public int pagina { get; set; } = 1;
		public string ordenar { get; set; } = "usu_apellidoynombre";
	}
}
