﻿
namespace gc.infraestructura.Dtos.Gen
{
	public class RespuestaDto
	{
		public string resultado { get; set; } = string.Empty;
		public string resultado_msj { get; set; } = string.Empty;
	}

	public class TIRespuestaDto : RespuestaDto {
        public string Ti { get; set; }=string.Empty;
        public string Tit_id { get; set; }=string.Empty;
		
    }
}
