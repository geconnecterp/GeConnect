namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Billeteras;

    public class AutoMapperBilleteraProfile : Profile
    {
        public AutoMapperBilleteraProfile()
        {
            CreateMap<Billetera, BilleteraDto>()
;
            CreateMap<BilleteraDto, Billetera>();

        }
    }
}
