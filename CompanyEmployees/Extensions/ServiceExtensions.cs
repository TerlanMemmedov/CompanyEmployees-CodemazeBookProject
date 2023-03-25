using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Service;
using Service.Contracts;
using System;
using System.Runtime.Intrinsics.X86;
using static System.Net.Mime.MediaTypeNames;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services) =>
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("X-Pagination")); //for send pagination in header and allow front to use it
            });

        public static void ConfigureIISIntegration(this IServiceCollection services) =>
            services.Configure<IISOptions>(options =>
            {

            });

        public static void ConfigureLoggerService(this IServiceCollection services) =>
            services.AddSingleton<ILoggerManager, LoggerManager>();

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            //works when compRepo and emplRepo are public
            /*
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));
             */

            //works also when compRepo and emplRepo are internal :)
            //This is the best solution for internal class
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddScoped(typeof(Lazy<>), typeof(Lazy<>));
            ServiceExtensionsRepo.ConfigureRepoInternal(services);
        }

        public static void ConfigureServiceManager(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>();
        }

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<RepositoryContext>(opts => opts.UseSqlServer
            (configuration.GetConnectionString("sqlConnection")));
        //this allows an easier configuration like it replaces AddDbContext and UseSqlServer, but =>
        //it doesn’t provide all of the features the AddDbContext method provides.
        //So for more advanced options, it is recommended to use AddDbContext.
        /*
        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddSqlServer<RepositoryContext>((configuration.GetConnectionString("sqlConnection")));
        */


        public static IMvcBuilder AddCustomCsvFormatter(this IMvcBuilder builder) => 
            builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

        public static void AddCustomMediaTypes(this IServiceCollection services)
        {
            services.Configure<MvcOptions>(config =>
            {
                var systemTextJsonOutputFormatter = config.OutputFormatters
                    .OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();

                if (systemTextJsonOutputFormatter != null)
                {
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+json");
                }

                var xmlOutputFormatter = config.OutputFormatters
                    .OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+xml");
                }
            });
        }
    }
}
