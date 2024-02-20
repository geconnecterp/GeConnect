using AutoMapper;
using gc.api.core.Entidades;
using gc.infraestructura.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace gc.api.infra.Mappings
{
    public class AutoMapperTestProfile : Profile
    {
        public AutoMapperTestProfile()
        {
            CreateMap<Test, TestDto>()
;
            CreateMap<TestDto, Test>();

        }
    }
}
