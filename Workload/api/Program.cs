using TemplateWorkload.Services;
using TemplateWorkload.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Configure Newtonsoft.Json for MVC/API controllers to ensure enums serialize as strings
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
  options.SerializerSettings.Converters.Add(new StringEnumConverter());
  options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
  options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
});

// Core Services
builder.Services.AddSingleton<IAzureTableClient, AzureTableClient>();
builder.Services.AddSingleton<IBlobStorageClient, BlobStorageClient>();
builder.Services.AddSingleton<IKeyVaultAccess, KeyVaultAccess>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IFabricApi, FabricApi>();

//--------------------Services--------------------
// Add your custom services here
// builder.Services.AddScoped<ISampleService, SampleService>();

//--------------------Controllers--------------------
// Controllers are automatically registered, but you can add custom configuration here

// Middleware (uncomment as needed)
// builder.Services.AddSingleton<IMiddlewareService, MiddlewareService>();
// builder.Services.AddSingleton<PermissionsMiddleware>();
// builder.Services.AddSingleton<AdminCheckMiddleware>();

// Use middleware (uncomment as needed)
// builder.UseWhen<RequestLoggingMiddleware>(
//     (context) => RequestLoggingMiddleware.ShouldRun(context, context.GetHttpContext())
// );
// builder.UseWhen<ExceptionHandlingMiddleware>(
//     (context) => ExceptionHandlingMiddleware.ShouldRun(context, context.GetHttpContext())
// );
// builder.UseWhen<RetrieveUserClientPrincipal>(
//     (context) => RetrieveUserClientPrincipal.ShouldRun(context, context.GetHttpContext())
// );
// builder.UseWhen<PermissionsMiddleware>(
//     (context) => PermissionsMiddleware.ShouldRun(context, context.GetHttpContext())
// );

builder.Build().Run();
