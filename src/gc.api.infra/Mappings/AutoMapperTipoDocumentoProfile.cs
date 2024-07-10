namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos;

    public class AutoMapperTipoDocumentoProfile : Profile
    {
        public AutoMapperTipoDocumentoProfile()
        {
            CreateMap<TipoDocumento, TipoDocumentoDto>()
;
            CreateMap<TipoDocumentoDto, TipoDocumento>();

        }
    }
}
