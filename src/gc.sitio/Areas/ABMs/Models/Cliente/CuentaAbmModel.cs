﻿using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
    public class CuentaAbmModel
    {
        public SelectList ComboAfip { get; set; }
        public SelectList ComboNatJud { get; set; }
        public SelectList ComboTipoDoc { get; set; }
        public SelectList ComboIngBruto { get; set; }
        public SelectList ComboProvincia { get; set; }
        public SelectList ComboDepartamento { get; set; }
        public SelectList ComboTipoCuentaBco { get; set; }
        public SelectList ComboTipoNegocio { get; set; }
        public SelectList ComboListaDePrecios { get; set; }
        public SelectList ComboTipoCanal { get; set; }
        public SelectList ComboVendedores { get; set; }
        public SelectList ComboDiasDeLaSemana { get; set; }
        public SelectList ComboZonas { get; set; }
        public SelectList ComboRepartidores { get; set; }
        public SelectList ComboFinancieros { get; set; }
        public CuentaABMDto Cliente { get; set; }
        public GridCoreSmart<CuentaFPDto> CuentaFormasDePago { get; set; }
        public GridCoreSmart<CuentaContactoDto> CuentaContactos { get; set; }
        public GridCoreSmart<CuentaObsDto> CuentaObs { get; set; }
        public GridCoreSmart<CuentaNotaDto> CuentaNota { get; set; }

        public CuentaAbmModel()
        {
            Cliente = new CuentaABMDto();
        }
    }
}
