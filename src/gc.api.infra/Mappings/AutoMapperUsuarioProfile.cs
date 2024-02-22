namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.DTOs;

    public class AutoMapperUsuarioProfile : Profile
    {
        public AutoMapperUsuarioProfile()
        {
            CreateMap<Usuario, UsuarioDto>()
;
            CreateMap<UsuarioDto, Usuario>();

        }
    }
}
