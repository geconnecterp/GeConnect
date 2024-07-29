namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Almacen;

    public class AutoMapperProductoProfile : Profile
    {
        public AutoMapperProductoProfile()
        {
            CreateMap<Producto, ProductoDto>()
;
            CreateMap<ProductoDto, Producto>();

        }
    }
}
