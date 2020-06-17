using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using Bcss.Reference.Grpc.Business;
using Bcss.Reference.Grpc.Business.Impl;
using Bcss.Reference.Grpc.Server.FeatureFlagging;
using Bcss.Reference.Grpc.Server.Interceptors;
using Bcss.Reference.Grpc.Server.Services;
using Bcss.Reference.Grpc.Shared.Config;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Grpc.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Bcss.Reference.Grpc.Server
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // AutoMapper provides this extension method, which accepts 1-N references to an assembly. AutoMapper then
            // uses reflection to scan the assemblies for any classes that extend `Profile`. All we need to do
            // is tell AutoMapper which assemblies contain profiles and it will wire everything else up for us.
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // This will add the our gRPC interceptors to all gRPC services. Interceptors
            // are similar to http middleware (in fact, they're implemented with some of the same code
            // in C#), but are used in a slightly different way since they act on the gRPC layer, not the HTTP layer.
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;

                // This interceptor logs the request headers of incoming requests.
                options.Interceptors.Add<RequestLoggingGrpcServerInterceptor>();
            });

            // Instantiate a singleton instance of the Grpc health check service. The gRPC health check service is an
            // optional service that gRPC servers can run which report on the health of the server. If your application has
            // the ability and need to report when it's in an unhealthy but still running state, you can implement overrides
            // for the health service endpoints to report the status as needed. By default, the service will be set to Serving,
            // and as long as your service is available no other work needs to be done for that status to continue to be reported.
            //
            // Note that unlike other services, this service is registered as an object *instance* rather than a type.
            // The result is that the provided object will be used as the singleton for all dependents of the type of the object.
            // This pattern is required at times but should only be used when necessary.
            var healthService = new HealthServiceImpl();
            healthService.SetStatus("", HealthCheckResponse.Types.ServingStatus.Serving);
            services.AddSingleton(healthService);

            // The reflection service acts as a reporting endpoint which will use reflection to identify all gRPC services that
            // are running on this server and provide a gRPC service that can report on this information. This can be very helpful
            // as it allows various gRPC tools to act as clients and learn about what services are available.
            // Tools such as BloomRPC can also make use of this information to provide skeleton requests, making the service easier to consume.
            var reflectionServiceImpl = new ReflectionServiceImpl(
                new List<Google.Protobuf.Reflection.ServiceDescriptor>
                {
                    Health.Descriptor
                }
            );
            services.AddSingleton(reflectionServiceImpl);

            // This extension method registers the feature management systems classes with the service provider, including
            // `IFeatureManager` which application code can depend on to provide information about what features are enabled.
            services.AddFeatureManagement(Configuration);

            services.AddOptions<V2RepositorySettings>()
                .Bind(Configuration.GetSection("V2RepositorySettings"))
                .ValidateDataAnnotations();

            // Here, we make use of the feature management system to determine which repository implementation should be used
            // at runtime. We register a singleton of each repository (since they hold data in memory as a mock "database"),
            // then map their shared interface to a delegate which extracts the feature manager from the provider to determine
            // which concrete type the interface should map to for the given request. By making the interface scoped, we
            // ensure that if the feature flag ever changes, the repository will be recreated with the new backing implementation
            // for each request.
            services.AddScoped<WeatherForecastRepository>();
            services.AddScoped<WeatherForecastRepositoryV2>();
            services.AddScoped<IWeatherForecastRepository>(provider =>
            {
                var featureManager = provider.GetRequiredService<IFeatureManager>();

                // Normally you should avoid calling `.Result` on a Task<T> object, but we have no choice here as the delegate
                // we're using to make use of the feature flag is not asynchronous. Thankfully, this blocking call will only occur
                // once at startup.
                if (featureManager.IsEnabledAsync(nameof(FeatureFlags.UseExperimentalRepository)).Result)
                {
                    return provider.GetRequiredService<WeatherForecastRepositoryV2>();
                }
                
                return provider.GetRequiredService<WeatherForecastRepository>();
            });

            // The feature management system also allows applications to implement the `IDisabledFeaturesHandler` to register
            // for notifications when callers attempt to access a feature that is blocked.
            services.AddSingleton<IDisabledFeaturesHandler, DisabledFeatureLoggingHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Automatically tests all configured mappings set up through AutoMapper. This looks for all invocations of
            // `IMapper.Map<T, U>()` and validates that a proper mapping for T -> U exists, throwing an exception and
            // blocking the start of the application if one does not exist.
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            app.UseAzureAppConfiguration();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ReflectionServiceImpl>();
                endpoints.MapGrpcService<HealthServiceImpl>();
                endpoints.MapGrpcService<GrpcWeatherForecastService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
