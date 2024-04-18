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
            CreateMap<RegistroUserDto, Usuarios>()
                .ForMember(dest => dest.usu_email, org => org.MapFrom(src => src.Correo))
                .ForMember(dest => dest.usu_password, org => org.MapFrom(src => src.Password))
                .ForMember(dest => dest.tdo_codigo, org => org.MapFrom(src => src.TipoDocumento))
                .ForMember(dest => dest.usu_ducumento, org => org.MapFrom(src => src.Documento))
                .ForMember(dest => dest.tdo_codigo, org => org.MapFrom(src => src.TipoDocumento));

        }
    }
}
