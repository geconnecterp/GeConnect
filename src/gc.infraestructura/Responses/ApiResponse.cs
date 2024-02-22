using gc.infraestructura.Core.EntidadesComunes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace gc.infraestructura.Core.Responses
{
    public class ApiResponse<T>
    {

        public ApiResponse(T data)
        {
            Data = data;

        }

        public T Data { get; set; }
        public Metadata? Meta { get; set; }
    }
}
