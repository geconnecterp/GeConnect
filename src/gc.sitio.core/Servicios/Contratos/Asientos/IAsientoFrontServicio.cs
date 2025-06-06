﻿using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.Asientos
{
    public interface IAsientoFrontServicio:IServicio<Dto>
    {
        /// <summary>
        /// Obtiene la lista de ejercicios contables
        /// </summary>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Lista de ejercicios</returns>
        Task<RespuestaGenerica<EjercicioDto>> ObtenerEjercicios(string token);

        /// <summary>
        /// Obtiene los tipos de asiento disponibles
        /// </summary>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Lista de tipos de asiento</returns>
        Task<RespuestaGenerica<TipoAsientoDto>> ObtenerTiposAsiento(string token);

        /// <summary>
        /// Obtiene los usuarios asociados a un ejercicio específico
        /// </summary>
        /// <param name="eje_nro">Número de ejercicio</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Lista de usuarios del ejercicio</returns>
        Task<RespuestaGenerica<UsuAsientoDto>> ObtenerUsuariosDeEjercicio(int eje_nro, string token);
    }
}
