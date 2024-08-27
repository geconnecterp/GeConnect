namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Almacen;

    public class AutoMapperdepositosProfile : Profile
    {
        public AutoMapperdepositosProfile()
        {
            CreateMap<Deposito, DepositoDto>()
;
            CreateMap<DepositoDto, Deposito>();

        }
    }
}
