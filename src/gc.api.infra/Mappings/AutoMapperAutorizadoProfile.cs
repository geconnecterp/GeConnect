namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos;

    public class AutoMapperAutorizadoProfile : Profile
    {
        public AutoMapperAutorizadoProfile()
        {
            CreateMap<Autorizado, AutorizadoDto>()
                .ForMember(dest => dest.UsuarioContrasena, org => org.MapFrom(src => src.Usuario.Contrasena))
                .ForMember(dest => dest.RolNombre, org => org.MapFrom(src => src.Role.Nombre))
;
            CreateMap<AutorizadoDto, Autorizado>();

        }
    }
}
