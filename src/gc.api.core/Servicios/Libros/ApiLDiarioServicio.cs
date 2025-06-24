using DocumentFormat.OpenXml.Office2013.Word;
using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios.Libros
{
    public class ApiLDiarioServicio : Servicio<EntidadBase>, IApiLDiarioServicio
    {
        public ApiLDiarioServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public List<AsientoDetalleLDDto> ObtenerAsientoLibroDiario(
            int eje_nro,
            bool periodo,
            DateTime desde,
            DateTime hasta,
            bool hasCarga,
            DateTime Cdesde,
            DateTime Chasta,
            string movimientos,
            bool conTemporales,
            int regs,
            int pag,
            string orden)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_LIBRO_DIARIO;// "spgeco_conta_asiento_tmp_datos"; // Usar el SP que mencionaste

            var ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@eje_nro", eje_nro));
            ps.Add(new SqlParameter("@fa", periodo));
            if (periodo)
            {
                ps.Add(new SqlParameter("@fa_desde", desde));
                ps.Add(new SqlParameter("@fa_hasta", hasta));
            }

            ps.Add(new SqlParameter("@fc", hasCarga));
            if (hasCarga)
            {
                ps.Add(new SqlParameter("@fc_desde", Cdesde));
                ps.Add(new SqlParameter("@fc_hasta", Chasta));
            }
            bool movi = !string.IsNullOrEmpty(movimientos);  //si movimientos es null o vacío, movi será false
            ps.Add(new SqlParameter("@movi", movi));
            ps.Add(new SqlParameter("@movi_like", movimientos));
            ps.Add(new SqlParameter("@incluye_tmp", conTemporales));
            ps.Add(new SqlParameter("@registros", regs));
            ps.Add(new SqlParameter("@pagina", pag));
            ps.Add(new SqlParameter("@ordenar", orden));


            // Ejecutar el procedimiento almacenado
            var resultados = _repository.EjecutarLstSpExt<AsientoPlanoLibroDiarioDto>(sp, ps, true);

            if (resultados == null || resultados.Count == 0)
            {
                return null;
            }

            List<AsientoDetalleLDDto> asientos = [];
            AsientoDetalleLDDto asientoDetalle = new();
            string moviId = string.Empty;

            // Agregar cada registro como una línea de detalle
            foreach (var registro in resultados)
            {
                if (!moviId.Equals(registro.dia_movi))
                {
                    //la primera vez la variable movi sera empty por lo que 
                    //no ingresará en el "if"
                    if (!string.IsNullOrEmpty(moviId))
                    {
                        asientos.Add(asientoDetalle);
                    }
                    moviId = registro.dia_movi;

                    // Crear el objeto AsientoDetalleDto a partir del primer registro
                    asientoDetalle = new AsientoDetalleLDDto()
                    {
                        Total_paginas = registro.Total_paginas,
                        Total_registros = registro.Total_registros,
                        Dia_movi = registro.dia_movi,
                        Dia_fecha = registro.dia_fecha,
                        Dia_tipo = registro.dia_tipo,
                        Dia_lista = registro.dia_lista,
                        Dia_desc_asiento = registro.dia_desc_asiento,
                        Detalles = new List<AsientoLineaDto>(),
                        TotalDebe = 0,
                        TotalHaber = 0,
                        eje_nro = registro.eje_nro,
                        esTemporal = registro.temporal
                    };
                }

                var linea = new AsientoLineaDto
                {
                    Dia_movi = registro.dia_movi,
                    Dia_nro = registro.dia_nro,
                    Ccb_id = registro.ccb_id,
                    Ccb_desc = registro.ccb_desc,
                    Dia_desc = registro.dia_desc,
                    Debe = registro.dia_debe,
                    Haber = registro.dia_haber
                };

                asientoDetalle.Detalles.Add(linea);
                asientoDetalle.TotalDebe += linea.Debe;
                asientoDetalle.TotalHaber += linea.Haber;
            }
            asientos.Add(asientoDetalle);
            return asientos;
        }

        public List<LibroDiarioResumen> ObtenerAsientoLibroDiarioResumen(int eje_nro, bool periodo, DateTime desde, DateTime hasta, bool hasCarga, DateTime Cdesde, DateTime Chasta, string movimientos, bool conTemporales, int regs, int pag, string orden)
        {
            var sp = ConstantesGC.StoredProcedures.SP_ASIENTO_LIBRO_DIARIO_RESUMEN;// "spgeco_conta_asiento_tmp_datos"; // Usar el SP que mencionaste

            var ps = new List<SqlParameter>();

            ps.Add(new SqlParameter("@eje_nro", eje_nro));
            ps.Add(new SqlParameter("@fa", periodo));
            if (periodo)
            {
                ps.Add(new SqlParameter("@fa_desde", desde));
                ps.Add(new SqlParameter("@fa_hasta", hasta));
            }

            ps.Add(new SqlParameter("@fc", hasCarga));
            if (hasCarga)
            {
                ps.Add(new SqlParameter("@fc_desde", Cdesde));
                ps.Add(new SqlParameter("@fc_hasta", Chasta));
            }
            bool movi = !string.IsNullOrEmpty(movimientos);  //si movimientos es null o vacío, movi será false
            ps.Add(new SqlParameter("@movi", movi));
            ps.Add(new SqlParameter("@movi_like", movimientos));
            ps.Add(new SqlParameter("@incluye_tmp", conTemporales));
            ps.Add(new SqlParameter("@registros", regs));
            ps.Add(new SqlParameter("@pagina", pag));
            ps.Add(new SqlParameter("@ordenar", orden));


            // Ejecutar el procedimiento almacenado
            var resultados = _repository.EjecutarLstSpExt<LibroDiarioResumen>(sp, ps, true);

            
            return resultados;
        }
    }
}
