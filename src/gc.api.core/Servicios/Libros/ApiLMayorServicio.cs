using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios.Libros;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Asientos;
using gc.infraestructura.Dtos.Libros;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios.Libros
{
    public  class ApiLMayorServicio:Servicio<EntidadBase>, IApiLMayorServicio
    {
        public ApiLMayorServicio(IUnitOfWork uow):base(uow)
        {
            
        }

        public List<LMayorRegListaDto> ObtenerLibroMayor(int eje_nro, string ccb_id,bool fa, DateTime fecha_desde, DateTime fecha_hasta, bool conTemporales,int regs,int pag,string ord)
        {
            var sp = ConstantesGC.StoredProcedures.SP_LIBRO_MAYOR_LISTA;
            var ps = new List<SqlParameter>();

            // Evaluar y agregar parámetros al procedimiento almacenado
            ps.Add(new SqlParameter("@eje_nro", eje_nro));
            ps.Add(new SqlParameter("@ccb_id", ccb_id));
                ps.Add(new SqlParameter("@fa", fa));
            if (fa)
            {
                ps.Add(new SqlParameter("@fa_desde", fecha_desde)); 
                ps.Add(new SqlParameter("@fa_hasta", fecha_hasta));
            }
            else
            {
                ps.Add(new SqlParameter("@fa_desde", new DateTime(1900,1,1)));
                ps.Add(new SqlParameter("@fa_hasta", new DateTime(3500,1,1)));
            }


                ps.Add(new SqlParameter("@incluye_tmp", conTemporales));
   
            ps.Add(new SqlParameter("@registros", regs)); // Valor por defecto: 10
            ps.Add(new SqlParameter("@pagina",pag)); // Valor por defecto: 1
            ps.Add(new SqlParameter("@ordenar",ord));

            // Ejecutar el procedimiento almacenado y devolver los resultados
            return _repository.EjecutarLstSpExt<LMayorRegListaDto>(sp, ps, true);
        }
    }
}
