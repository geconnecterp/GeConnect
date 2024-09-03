﻿using gc.infraestructura.Dtos.CuentaComercial;
using gc.sitio.Models.ViewModels;

namespace gc.sitio.Areas.Compras.Models
{
	public class DetalleOCModel
	{
        public GridCore<RPROrdenDeCompraDetalleDto> Detalle { get; set; }
        public string OCCompte { get; set; } = string.Empty;
    }
}
