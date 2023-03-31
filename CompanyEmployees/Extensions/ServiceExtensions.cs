using AspNetCoreRateLimit;
using CompanyEmployees.Presentation.Controllers;
using Contracts;
using LoggerService;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
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
                    systemTextJsonOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.apiroot+json");
                }

                var xmlOutputFormatter = config.OutputFormatters
                    .OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

                if (xmlOutputFormatter != null)
                {
                    xmlOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.hateoas+xml");
                    xmlOutputFormatter.SupportedMediaTypes
                        .Add("application/vnd.codemaze.apiroot+xml");
                }
            });
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("api-version"); //for sending version in header
                //normally it can be send in params (FromQuery such query string)
                //and also in the Route url for example (Route[api/{v:apiversion}/companies])

                //for reading it from Query string with modification like if we want
                //options.ApiVersionReader = new QueryStringApiVersionReader("api-version");


                //If we have a lot of versions of a single controller, we can assign these
                //versions in the configuration instead of inside these controllers' attribute
                //IF we use that, we can remove the [ApiVersion] attribute from the controllers.
                /*
                options.Conventions.Controller<CompaniesController>().HasApiVersion(new ApiVersion(1, 0));
                options.Conventions.Controller<CompaniesV2Controller>().HasApiVersion(new ApiVersion(2, 0));
                 */

            });
        }

        public static void ConfigureResponseCaching(this IServiceCollection services) =>
            services.AddResponseCaching();

        public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
            services.AddHttpCacheHeaders(
                (expirationOpt) =>
                {
                    expirationOpt.MaxAge = 65;
                    expirationOpt.CacheLocation = CacheLocation.Private;
                },
                (validationOpt) =>
                {
                    validationOpt.MustRevalidate = true;
                });


        public static void ConfigureRateLimitingOptions(this IServiceCollection services)
        {
            var rateLimitRules = new List<RateLimitRule>
            {
                new RateLimitRule
                {
                    Endpoint = "*", //for any endpoint in our API
                    Limit = 5,
                    Period = "5m"
                }
            };

            services.Configure<IpRateLimitOptions>(opt => { opt.GeneralRules = rateLimitRules; });
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }
    }
}
