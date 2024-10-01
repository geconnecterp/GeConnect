namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.General;

    public class AutoMapperProductoDepositoProfile : Profile
    {
        public AutoMapperProductoDepositoProfile()
        {
            CreateMap<ProductoDeposito, ProductoDepositoDto>();
            CreateMap<ProductoDepositoDto, ProductoDeposito>();

        }
    }
}
