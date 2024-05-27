namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Cajas;

    public class AutoMapperCajaProfile : Profile
    {
        public AutoMapperCajaProfile()
        {
            CreateMap<Caja, CajaDto>()
;
            CreateMap<CajaDto, Caja>();

        }
    }
}
