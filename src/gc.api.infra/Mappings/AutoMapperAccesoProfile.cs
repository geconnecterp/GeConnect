namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.DTOs;

    public class AutoMapperAccesoProfile : Profile
    {
        public AutoMapperAccesoProfile()
        {
            CreateMap<Acceso, AccesoDto>()
                .ForMember(dest => dest.UsuarioContrasena, org => org.MapFrom(src => src.Usuario.Contrasena))
;
            CreateMap<AccesoDto, Acceso>();

        }
    }
}
