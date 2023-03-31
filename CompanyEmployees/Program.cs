using AspNetCoreRateLimit;
using CompanyEmployees.Extensions;
using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Utility;
using Contracts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using Service.DataShaping;
using Shared.DataTransferObjects;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

//for logging
LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureRepositoryManager();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();
builder.Services.AddScoped<IEmployeeLinks, EmployeeLinks>();
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching();
builder.Services.ConfigureHttpCacheHeaders();
builder.Services.AddMemoryCache();
builder.Services.ConfigureRateLimitingOptions();
builder.Services.AddHttpContextAccessor();

/*
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("link")
    .AllowAnyHeader()
    .AllowAnyMethod()));
*/


//Without this, ApiController attribute don't give a chance to us to show our own replies in 400
//error to the client, so we added this to prevent it and show our replies
//suppresses this behavior of apicontroller attribute
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


//Action filter service
builder.Services.AddScoped<ValidationFilterAttribute>();
builder.Services.AddScoped<ValidateMediaTypeAttribute>();


//just for json
/*
builder.Services.AddControllers()
     .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);
*/

//supports other types like xml and custom written Csv
builder.Services.AddControllers(config =>
{
    config.RespectBrowserAcceptHeader = true;
    config.ReturnHttpNotAcceptable = true;
    config.InputFormatters.Insert(0, GetJsonPatchInputFormatter());
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 });
})/*.AddNewtonsoftJson()*/  
    //Best Place for nwSJ. This helps the patch to work, and in here it doesn't block json 
    //Sometimes this AddNewtonsoftJson doesn't allow program to work with new media types, so blocked for now
  .AddXmlDataContractSerializerFormatters() //and returns what type you want
  .AddCustomCsvFormatter()
  .AddApplicationPart(typeof(CompanyEmployees.Presentation.AssemblyReference).Assembly);
//If we write newtonsoftjson here it can't return json, only returns xml

builder.Services.AddCustomMediaTypes();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(logger);

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
    //adds Strict-Transport-Security header
    app.UseHsts();

//redirection from HTTP to HTTPS
app.UseHttpsRedirection();

//
app.UseStaticFiles();

//forwards proxy headers to the current request and will help during application deployment
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});


app.UseIpRateLimiting();
app.UseCors("CorsPolicy"); //app.UseCors();
app.UseResponseCaching();
app.UseHttpCacheHeaders();

app.UseAuthorization();

app.MapControllers();

app.Run();


//for Patch
NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter() =>
    new ServiceCollection().AddLogging().AddMvc().AddNewtonsoftJson()
    .Services.BuildServiceProvider()
    .GetRequiredService<IOptions<MvcOptions>>().Value.InputFormatters
    .OfType<NewtonsoftJsonPatchInputFormatter>().First();


