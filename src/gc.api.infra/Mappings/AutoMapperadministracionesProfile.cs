namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.Dtos.Administracion;

    public class AutoMapperadministracionesProfile : Profile
    {
        public AutoMapperadministracionesProfile()
        {
            CreateMap<Administracion, AdministracionDto>();
            CreateMap<AdministracionDto, Administracion>();

            CreateMap<Administracion,AdministracionLoginDto>()
                .ForMember(dest=>dest.Id, org=>org.MapFrom(src=>src.Adm_id))
                .ForMember(dest=>dest.Descripcion,org=>org.MapFrom(src=>src.Adm_nombre));

        }
    }
}
