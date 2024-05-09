namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Billeteras;

    public class AutoMapperBOrdenEstadoProfile : Profile
    {
        public AutoMapperBOrdenEstadoProfile()
        {
            CreateMap<BOrdenEstado, BOrdenEstadoDto>()
;
            CreateMap<BOrdenEstadoDto, BOrdenEstado>();

        }
    }
}
