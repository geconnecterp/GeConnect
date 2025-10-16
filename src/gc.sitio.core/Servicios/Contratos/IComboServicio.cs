using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.Dtos.Productos.PromoCombo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IComboServicio
    {
        /// <summary>
        /// Obtiene los tipos disponibles para combos y promociones
        /// </summary>
        Task<RespuestaGenerica<ComboTipoDto>> ObtenerComboTipos(string token);

        /// <summary>
        /// Obtiene los estados disponibles para combos y promociones
        /// </summary>
        Task<RespuestaGenerica<ComboEstadoDto>> ObtenerComboEstados(string token);

        /// <summary>
        /// Busca combos y promociones según los filtros especificados
        /// </summary>
        Task<RespuestaGenerica<ComboListaDto>> BuscarCombos(QueryFilters filtros, string token);

        /// <summary>
        /// Metodo que permitirá obtener los canales en donde se encuentra la Promo/Combo
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RespuestaGenerica<ComboCanalDto>> ObtenerCanalesDeCombo(string id, string token);

        /// <summary>
        /// Metodo que permite obtener los datos del Combo/Promoción
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RespuestaGenerica<ComboDatosDto>> ObtenerComboPorId(string id, string token);
        
        /// <summary>
        /// Obtiene los productos asociados a un combo específico
        /// </summary>
        /// <param name="id">Identificador del combo</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Lista de productos asociados al combo</returns>
        Task<RespuestaGenerica<ComboProductoDto>> ObtenerProductosDeCombo(string id, string token);
        
        /// <summary>
        /// Obtiene los productos sustitutos asociados a un producto dentro de un combo específico
        /// </summary>
        /// <param name="comboId">Identificador del combo</param>
        /// <param name="productoId">Identificador del producto</param>
        /// <param name="token">Token de autenticación</param>
        /// <returns>Lista de productos sustitutos</returns>
        Task<RespuestaGenerica<ComboSustitutoDto>> ObtenerProductosSustitutosDeCombo(string comboId, string productoId, string token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<RespuestaGenerica<RespuestaDto>> ConfirmarCombo(AbmPlusGenDto req, string token);
    }
}
