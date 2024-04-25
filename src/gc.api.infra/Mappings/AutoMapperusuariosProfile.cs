namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos;

    public class AutoMapperusuariosProfile : Profile
    {
        public AutoMapperusuariosProfile()
        {
            CreateMap<Usuario, UsuariosDto>()
;
            CreateMap<UsuariosDto, Usuario>();

        }
    }
}
