using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace gc.api.core.Servicios
{
    public class LinkServicio : Servicio<EntidadBase>, ILinkServicio
    {
        private readonly IConfiguration _configuration;
        public LinkServicio(IUnitOfWork uow, IConfiguration configuration) : base(uow)
        {
            _configuration = configuration;
        }
        public ReporteLinkResponseDto CrearLink(ReporteSolicitudDto solicitud, string usu_id, string? clienteId = null)
        {
            var codigo = GenerarCodigoUnicoAsync();
            var minutosExpiracion = _configuration.GetValue<int>("Reportes:LinkExpiracionMinutos", 60);

            var sp = ConstantesGC.StoredProcedures.SP_REPO_INSERTAR;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Codigo", codigo),
                new SqlParameter("@PayloadJson", JsonConvert.SerializeObject(solicitud)),
                new SqlParameter("@FechaCreacionUtc", DateTime.UtcNow),
                new SqlParameter("@FechaExpiracionUtc", DateTime.UtcNow.AddMinutes(minutosExpiracion)),
                new SqlParameter("@ClienteId", clienteId),
                new SqlParameter("@CreadoPor", usu_id),
                new SqlParameter("@Id", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
            };

            var resultado = _repository.InvokarSpNQuery(sp, ps);
            var idGenerado = (long)ps.FirstOrDefault(p => p.ParameterName == "@Id").Value;
            if (idGenerado == 0 || idGenerado < 0)
                throw new Exception("Error al crear el link del reporte.");

            //la URL se construye en el controlador GestorImpresionController del Sitio Geco.
            return new ReporteLinkResponseDto
            {
                Codigo = codigo,
                ExpiraEnUtc = DateTime.UtcNow.AddMinutes(minutosExpiracion)
            };
        }


        public ReporteSolicitudDto ObtenerSolicitud(string codigo)
        {
            var sp = ConstantesGC.StoredProcedures.SP_REPO_EXISTE;

            var ps = new List<SqlParameter>
            {
                new SqlParameter("@Codigo", codigo)
            };

            //se verifica si existe el código, si no existe o ya fue usado o expiró, se retorna null. Si es válido, se marca como usado y se retorna la solicitud.


            var reg = _repository.InvokarSpScalar(sp, ps);


            if ((int)reg == 0)
            {
                throw new NegocioException("El código no existe");
            }

            //si existe, se obtiene la entidad completa para verificar si ya fue usado o expiró, y para marcarlo como usado.
            sp = ConstantesGC.StoredProcedures.SP_REPO_RESUELVE;

            ps = new List<SqlParameter>
                {
                    new SqlParameter("@Codigo", codigo)
                };

            var entity = _repository.EjecutarLstSpExt<ReporteLinkDto>(sp, ps, true);

            if (entity == null || entity.Count == 0)
            {
                throw new NegocioException("Error al resolver el código");
            }

            int estado = entity[0].Estado;

            switch (estado)
            {
                case 1:
                    throw new NegocioException("El código no existe");
                case 2:
                    throw new NegocioException("El código ya fue usado");
                case 3:
                    throw new NegocioException("El código ha expirado");
                default:
                    break;
            }


            //if (entity[0].FechaExpiracionUtc < DateTime.UtcNow)
            //    throw new NegocioException("El código ha expirado");          

            return JsonConvert.DeserializeObject<ReporteSolicitudDto>(entity[0].PayloadJson);
        }


        private string GenerarCodigoUnicoAsync()
        {
            var sp = ConstantesGC.StoredProcedures.SP_REPO_EXISTE;

            while (true)
            {
                var codigo = GenerarCodigoCorto(8);
                var ps = new List<SqlParameter>
                {
                    new SqlParameter("@Codigo", codigo)
                };
                var existe = _repository.InvokarSpScalar(sp, ps);

                if ((int)existe == 0)
                    return codigo;
            }
        }

        private static string GenerarCodigoCorto(int longitud)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = RandomNumberGenerator.GetBytes(longitud);
            var result = new char[longitud];

            for (int i = 0; i < longitud; i++)
                result[i] = chars[bytes[i] % chars.Length];

            return new string(result);
        }

    }
}
