namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Administracion;

    public class AutoMapperadministracionesProfile : Profile
    {
        public AutoMapperadministracionesProfile()
        {
            CreateMap<Administracion, AdministracionDto>()
;
            CreateMap<AdministracionDto, Administracion>();

        }
    }
}
