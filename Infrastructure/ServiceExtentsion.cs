using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class ServiceExtentsion
    {

        public static void AddInfrastructure(this IServiceCollection services)
        {
            //    services.AddAutoMapper(Assembly.GetExecutingAssembly());
            //    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            //}

        }
    }
}
