using gc.api.core.Entidades;
using System;
using System.Collections.Generic;
using System.Text;

namespace gc.api.core.Entidades
{
    public class Persona : EntidadBase
    {
        public string? NombreCompleto { get { return $"{Apellido}, {Nombre}"; } }

        public Persona()
        {

        }

        public Guid Id { get; set; }
        public int CodigoInterno { get; set; }
        public bool EsCliente { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? NNCompleto { get; set; }
        public string? RazonSocial { get; set; }
        public string? NombreFantasia { get; set; }
        public string? Direccion { get; set; }
        public bool EsFisica { get; set; }
        public string? Telefono { get; set; }
        public string? Celular { get; set; }
        public int LocalidadId { get; set; }
        public int TipoDocumentoId { get; set; }
        public string? CUIT { get; set; }
        public bool RealizarPercepcion { get; set; }
        public string? IngresosBrutos { get; set; }
        public string? Email { get; set; }
        public bool TieneCuentaCorriente { get; set; }
        public bool EsEmpleado { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? ContactoNombre { get; set; }
        public string? ContactoTelefono { get; set; }
        public bool Activo { get; set; }
        public decimal? LimiteVenta { get; set; }
        public int? DiasMora { get; set; }
        public bool? ProveedorServicios { get; set; }
        public bool? EsSucursal { get; set; }
        public bool? EsDueno { get; set; }
        public string? BancoProveedor { get; set; }
        public string? BancoCBU { get; set; }
        public bool? EsLaboratorio { get; set; }
        public string? VistaFormulario { get; set; }


    }
}
