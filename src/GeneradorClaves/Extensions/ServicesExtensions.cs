using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorClaves.Extensions
{
    public static class ServicesExtensions
    {
        public static ServiceCollection AddServicios(this ServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration)
                .AddScoped<IGeneradorClave, GeneradorClave>();

            return services;
        }
    }
}
