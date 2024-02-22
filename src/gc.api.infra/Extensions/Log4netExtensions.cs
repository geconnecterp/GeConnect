using log4net;
using log4net.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.infra.Extensions
{
    public static class Log4netExtensions
    {
        public static void AddLog4net(this IServiceCollection services)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            //services.AddSingleton(LogManager.GetLogger(typeof(Program)));
        }
    }
}
