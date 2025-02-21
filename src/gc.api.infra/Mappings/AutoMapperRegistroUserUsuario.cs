using AutoMapper;
using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.infra.Mappings
{
    public class AutoMapperRegistroUserUsuario:Profile
    {
        public AutoMapperRegistroUserUsuario()
        {
            CreateMap<RegistroUserDto, Usuario>()
                .ForMember(dest => dest.Usu_id, org => org.MapFrom(src => src.User))
                .ForMember(dest => dest.Usu_email, org => org.MapFrom(src => src.Correo))
                .ForMember(dest => dest.Usu_password, org => org.MapFrom(src => src.Password))
                .ForMember(dest => dest.Tdo_codigo, org => org.MapFrom(src => src.TipoDocumento))
                .ForMember(dest => dest.Usu_documento, org => org.MapFrom(src => src.Documento))
                .ForMember(dest => dest.Usu_apellidoynombre, org => org.MapFrom(src => src.ApellidoYNombre));

        }
    }
}
