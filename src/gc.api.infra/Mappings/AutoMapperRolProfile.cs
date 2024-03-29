namespace gc.api.infra.Mappings
{
    using AutoMapper;
    using gc.api.core.Entidades;
    using gc.infraestructura.DTOs;

    public class AutoMapperRolProfile : Profile
    {
        public AutoMapperRolProfile()
        {
            CreateMap<Role, RolDto>()
;
            CreateMap<RolDto, Role>();

        }
    }
}
