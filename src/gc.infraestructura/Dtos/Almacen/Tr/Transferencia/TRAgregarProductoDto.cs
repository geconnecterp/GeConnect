﻿using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRAgregarProductoDto : Dto
	{
        public GridCoreSmart<TRProductoParaAgregar> Productos { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string adm_id { get; set; } = string.Empty;
        public string Leyenda { get; set; } = string.Empty;
        public TRAgregarProductoDto() 
        {
			Productos = new GridCoreSmart<TRProductoParaAgregar>();
        }
    }
}
