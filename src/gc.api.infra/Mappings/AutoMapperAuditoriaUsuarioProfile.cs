namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos;

    public class AutoMapperAuditoriaUsuarioProfile : Profile
    {
        public AutoMapperAuditoriaUsuarioProfile()
        {
            CreateMap<AuditoriaUsuario, AuditoriaUsuarioDto>()
                .ForMember(dest => dest.UsuarioContrasena, org => org.MapFrom(src => src.Usuario.Contrasena))
;
            CreateMap<AuditoriaUsuarioDto, AuditoriaUsuario>();

        }
    }
}
