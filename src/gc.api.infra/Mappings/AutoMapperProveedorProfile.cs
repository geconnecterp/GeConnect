namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Almacen;

    public class AutoMapperProveedorProfile : Profile
    {
        public AutoMapperProveedorProfile()
        {
            CreateMap<Proveedor, ProveedorDto>()
;
            CreateMap<ProveedorDto, Proveedor>();

            CreateMap<ProveedorLista, ProveedorListaDto>();

        }
    }
}
