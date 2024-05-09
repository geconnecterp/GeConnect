namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Billeteras;

    public class AutoMapperBilleteraOrdenProfile : Profile
    {
        public AutoMapperBilleteraOrdenProfile()
        {
            CreateMap<BilleteraOrden, BilleteraOrdenDto>()
;
            CreateMap<BilleteraOrdenDto, BilleteraOrden>();

        }
    }
}
