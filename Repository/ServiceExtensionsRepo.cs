using Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public static class ServiceExtensionsRepo
    {
        public static void ConfigureRepoInternal(IServiceCollection services)
        {
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));
        }
    }
}
