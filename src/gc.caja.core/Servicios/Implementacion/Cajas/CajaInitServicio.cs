using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class CajaInitServicio : Servicio<Dto>, ICajaInitServicio
    {
        public CajaInitServicio(IOptions<AppSettings> options, ILogger<CajaInitServicio> logger) : base(options, logger)
        {

        }

        public (bool, string) ValidarDatosIniciales(CajaSettings caja)
        {
            //            public string? caja_nro_proceso { get; set; }
            //public string? caja_nro_cierre { get; set; }
            //public string? caja_nro_operacion { get; set; }
            //public string caja_activa { get; set; } = string.Empty;
            //     public bool min { get; set; }
            //public string lp_id_min { get; set; } = string.Empty;
            //public decimal? lp_id_min_porc { get; set; }

            //public bool may { get; set; }
            //public string lp_id_may { get; set; } = string.Empty;
            //public decimal? lp_id_may_porc { get; set; }

            //se deben validar los datos de la caja. Si algun dato no es valido o no se encuentra, se debe retornar false.
            //Si todos los datos son validos, se retorna true.

            if (string.IsNullOrEmpty(caja.CajaId))
            {
                _logger.LogError("El ID de la caja no puede ser nulo o vacío.");
                return (false, "El ID de la caja no puede ser nulo o vacío.");
            }

            if (string.IsNullOrEmpty(caja.Caja.caja_nro_proceso))
            {
                _logger.LogError("No se encontro el Nro de Proceso");
                return (false, "No se encontro el Nro de Proceso");
            }

            if (string.IsNullOrEmpty(caja.Caja.caja_nro_cierre))
            {
                _logger.LogError("No se encontro el Nro de Cierre");
                return (false, "No se encontro el Nro de Cierre");  
            }

            if (string.IsNullOrEmpty(caja.Caja.caja_nro_operacion))
            {
                _logger.LogError("No se encontro el Nro de Operacion");
                return (false, "No se encontro el Nro de Operacion");   
            }

            if (string.IsNullOrEmpty(caja.Caja.caja_activa))
            {
                _logger.LogError("No se encontro el estado de la caja");
                return (false, "No se encontro el estado de la caja");
            }

            if (caja.Caja.min)
            {
                if (string.IsNullOrEmpty(caja.Caja.lp_id_min))
                {
                    _logger.LogError("No se encontro el ID del Listado de Precios para Minimo");
                    return (false, "No se encontro el ID del Listado de Precios para Minimo");
                }
                if (!caja.Caja.lp_id_min_porc.HasValue)
                {
                    _logger.LogError("No se encontro el porcentaje del Listado de Precios para Minimo");
                    return (false, "No se encontro el porcentaje del Listado de Precios para Minimo");
                }
            }

            if (caja.Caja.may)
            {
                if (string.IsNullOrEmpty(caja.Caja.lp_id_may))
                {
                    _logger.LogError("No se encontro el ID del Listado de Precios para Maximo");    
                    return (false, "No se encontro el ID del Listado de Precios para Maximo");
                }
                if (!caja.Caja.lp_id_may_porc.HasValue)
                {
                    _logger.LogError("No se encontro el porcentaje del Listado de Precios para Maximo");
                    return (false, "No se encontro el porcentaje del Listado de Precios para Maximo");  
                }
            } 

            return (true, "Todos los datos son válidos");
        }
    }
}
