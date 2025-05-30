﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.CuentaComercial
{
    public class BuscarCuentaDto
    {
        public string Cuenta { get; set; } = string.Empty;
        public SelectList ComboDeposito { get; set; } = new SelectList(new List<Dto>());
        public string IdTipoCompte { get; set; } = string.Empty;
        public string NroCompte { get; set; } = string.Empty;
        public string rp { get; set; } = string.Empty;
        public string Rpe_id { get; set; } = "P";
        public string Nota { get; set; } = string.Empty;
        public string FechaTurno { get; set; } = string.Empty;
        public string Depo_id { get; set; } = string.Empty;
        public RPRComptesDeRPDto Compte { get; set; } = new();
        public int CantidadUL { get; set; }
        public string TituloVista { get; set; } = string.Empty;
    }
}
