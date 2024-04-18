namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;

    public class AutoMapperusuariosProfile : Profile
    {
        public AutoMapperusuariosProfile()
        {
            CreateMap<Usuarios, usuariosDto>()
;
            CreateMap<usuariosDto, usuarios>();

        }
    }
}
