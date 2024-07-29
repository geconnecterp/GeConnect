namespace geco_0000.Infraestructura.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Almacen;

    public class AutoMapperCuentaProfile : Profile
    {
        public AutoMapperCuentaProfile()
        {
            CreateMap<Cuenta, CuentaDto>()
;
            CreateMap<CuentaDto, Cuenta>();

        }
    }
}
