using System;
using System.Reflection;
using AutoMapper;
using Bcss.Reference.Business;
using Bcss.Reference.Business.Impl;
using Bcss.Reference.Config;
using Bcss.Reference.Data;
using Bcss.Reference.Data.Grpc;
using Bcss.Reference.Grpc;
using Bcss.Reference.Web.FeatureFlagging;
using Bcss.Reference.Web.Middleware;
using Bcss.Reference.Web.ModelBinders;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Bcss.Reference.Bootstrap
{
    /// <summary>
    /// The Startup class is responsible for initializing the applications object graph, as well as
    /// configuring what features of the framework the application will make use of. This class
    /// exposes two primary methods of interest, both of which are invoked, in order, by the framework
    /// during startup.
    ///
    /// ConfigureServices() is given an IServiceCollection object (see below for more details on that).
    /// This method gives applications the ability to register their interfaces and classes with the
    /// dependency injection (DI) framework. The DI framework is responsible for managing the lifetime
    /// of all objects, as well as providing dependencies to classes who request them in their constructors.
    /// In effect, this means you should rarely need to "new up" a class, unless it is a simple data transfer object.
    /// No class with any appreciable logic should ever need to be manually created. Instead, register the class
    /// (and any interfaces it implements) with the DI framework and let the framework provide the object to you.
    ///
    /// Configure() takes in an IApplicationBuilder and an IWEbHostEnvironment object. 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// A reference to the assembly containing our API code. We require this reference
        /// when configuring the framework in ConfigureServices().
        /// </summary>
        private Assembly WebAssembly { get; }

        /// <summary>
        /// Gets the Configuration object which contains the full set of key/value pairs that were
        /// registered by all configuration providers.
        /// </summary>
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            WebAssembly = Assembly.Load("Bcss.Reference.Web");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // `services` is an `IServiceCollection`, which is a collection object containing 1-N `ServiceDescriptor`
        // objects. These objects contain information about what objects are available to be injected via the
        // dependency injection system.
        // There are several different ways to register objects, some of which are shown below.
        public void ConfigureServices(IServiceCollection services)
        {
            // This sets up the class with the Options system, which is a powerful addition to the Configuration system. The Options system
            // provides a set of classes that can be used to read configuration data at run time using strongly typed POCOs (Plain-Old-C#-Objects),
            // rather than relying on directly referencing the configuration system and using string key lookups.
            // For an example of how this is used by dependent classes, see the RequestLoggingMiddleware.
            services.AddOptions<RequestHandlingOptions>().Bind(Configuration.GetSection("RequestHandlingOptions"));
            
            // AutoMapper provides this extension method, which accepts 1-N references to an assembly. AutoMapper then
            // uses reflection to scan the assemblies for any classes that extend `Profile`. All we need to do
            // is tell AutoMapper which assemblies contain profiles and it will wire everything else up for us.
            var businessAssembly = Assembly.Load("Bcss.Reference.Business.Impl");
            var grpcClientAssembly = Assembly.Load("Bcss.Reference.Data.Grpc");
            services.AddAutoMapper(WebAssembly, businessAssembly, grpcClientAssembly);

            // Here, we register our interface->implementation type mappings with the DI framework.
            // As the DI framework is responsible for the lifecycle of all objects it manages, the framework
            // provides various methods to apply the appropriate rules based on your needs.
            //
            // AddTransient<>() will treat the registered type as transient, meaning whenever any class requests
            // an instance of the registered type, a brand new instance will be created every time, even for different
            // consumers within a single request.
            //
            // AddScoped<>() treats the registered type as having a lifecycle equivalent to that of the incoming request.
            // That is, all classes which depend on the registered type will receive the same instance as that type,
            // but once the request has been completed, that instance is destroyed and a new instance will be made
            // for the next request. Even if two requests are processed concurrently, the object is never shared.
            //
            // AddSingleton() treats the registered type as a singleton, meaning there will only ever be one instance
            // of the class in memory. Be careful here: This is NOT the same as a class which implements the singleton pattern
            // using a static "Current" or "Instance" property. THAT approach is fundamentally flawed and should almost never
            // be used. The AddSingleton() method allows you to reap the same benefits you get from a singleton object without
            // having to implement the singleton pattern yourself. This lifecycle can be dangerous if used improperly,
            // as it's easy to accidentally allow yourself to maintain state within the class between requests (which should
            // NEVER be done in any circumstance: applications in a micro-service architecture must be as stateless as possible).
            //
            // One final thing to note is that because different objects can be given different lifecycles, there are some rules
            // enforced by the framework at runtime. Specifically, no type can depend on a type whose lifecycle is shorter than
            // that of itself. In other words, a singleton cannot depend on a scoped service, since the scoped object will be destroyed
            // at the end of a request but the singleton will not. This does require you to think very carefully about your object graph
            // to ensure you don't put yourself in a situation where you grant a longer lifecycle to an object than you intended.

            // StartupFilters are only run once, at application startup, so they are a perfect candidate for a transient lifecycle.
            services.AddTransient<IStartupFilter, BlackCatLogoLoggingStartupFilter>();

            // As the WeatherForecastService doesn't directly implement any sort of long-lived or expensive actions, we can safely
            // mark it as scoped.
            services.AddScoped<IWeatherForecastService, WeatherForecastService>();

            // Since the GrpcWeatherForecastClient maintains a reference to the gRPC client, which holds an HttpClient connection,
            // it's better to make this class a singleton so we're not constantly disconnecting/reconnecting the client.
            //
            // This pattern is required when configuring a class which implements multiple interfaces to ensure that only one
            // instance of the implementation class is ever created (aka 'forwarding' in DI terms). For more information, see:
            // https://andrewlock.net/how-to-register-a-service-with-multiple-interfaces-for-in-asp-net-core-di/
            //
            // This syntax is basically saying "When a dependent class is requesting an implementation of the given interface,
            // call the provided action method (the lambda passed as a parameter to AddSingleton) and use the result for the dependency".
            //
            // The lambda is given an IServiceProvider, which is a provider class from the DI framework that exposes the GetService() and GetRequiredService()
            // methods. This provider represents the full set of all registered types with the DI framework. This is made possible by the fact
            // that the action can never be invoked until after the app has finished initializing.
            //
            // The provider exposes the methods GetService(), which will will return null if no service can be found,
            // and GetRequiredService, which will throw an exception.
            services.AddSingleton<GrpcWeatherForecastClient>();
            services.AddSingleton<IWeatherForecastReader>(provider => provider.GetRequiredService<GrpcWeatherForecastClient>());
            services.AddSingleton<IWeatherForecastWriter>(provider => provider.GetRequiredService<GrpcWeatherForecastClient>());

            // This is actually a fix required to enable the use of unencrypted Http2 connections since we're talking to
            // the gRPC service over an insecure channel. If this is omitted, the connection will not be formed
            // and you'll see an RpcException of Internal with an inner IOException saying the response ended prematurely.
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // This extension method is provided by the Grpc.Net.ClientFactory NuGet package, and provides
            // a fluent interface with which we can configure a GrpcClient class.
            services.AddGrpcClient<WeatherService.WeatherServiceClient>(options =>
                {
                    // Here is an example of how you can retrieve configuration data within `ConfigureServices`,
                    // before you are able to access any `IOptions<>` classes (as those are created as part of
                    // the initialization of the DI framework, which is still on-going at this point in the
                    // apps execution).
                    var weatherServiceHost = Configuration.GetValue<string>("WeatherServiceHost");
                    options.Address = new Uri(weatherServiceHost);
                })
                .ConfigureChannel(grpcChannelOptions =>
                {
                    grpcChannelOptions.Credentials = ChannelCredentials.Insecure;
                });

            // This extension method registers the feature management systems classes with the service provider, including
            // `IFeatureManager` which application code can depend on to provide information about what features are enabled.
            services.AddFeatureManagement();

            // The feature management system also allows applications to implement the `IDisabledFeaturesHandler` to register
            // for notifications when callers attempt to access a feature that is blocked.
            services.AddSingleton<IDisabledFeaturesHandler, DisabledFeatureLoggingHandler>();

            // Here, we use the AddMvc() extension method to gain access to AppApplicationPart() and AddControllersAsServices().
            // These methods together allow us to move our API code into a separate assembly from where Startup.cs lives.
            services.AddMvc(options =>
                {
                    // "Model Binding" is the act of taking some input data (typically in json) and assigning, or 'binding', the values
                    // in the data to a known object type. This is the behavior that occurs in both controllers and gRPC servers when
                    // your application code is handed a strongly typed object by the framework. The framework gives you the ability to
                    // implement your own custom model binding as needed. Here, we're going to use it to fix an issue with .Net Cores
                    // DateTime model binder where it does not properly handle UTC ISO timestamps.
                    options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
                })
                .AddApplicationPart(WebAssembly)
                .AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline,
        // primarily through the ordering of middleware and to wrap up any final app configuration.
        // At this stage, the DI framework is fully assembled, and this method is specially designed
        // to accept arbitrary parameters from it. Here, we use it to obtain a reference to the IMapper
        // interface to invoke some initialization sanity testing.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMapper mapper)
        {
            // The IWebHostEnvironment object `env` provides environment information about the application.
            // It exposes a handful of helpful methods such as IsDevelopment() which can be used to make
            // determinations of how the application should behave based on environment. The determination
            // for whether env returns true for `IsDevelopment()` or any other environmental method is made
            // by examining the value of the `ASPNETCORE_ENVIRONMENT` environment variable.
            if (env.IsDevelopment())
            {
                // Here, we want to use the developer exception page as a global exception handlers for any
                // uncaught issues. However, we only want to expose this in developer mode because showing
                // stack traces to end-users is ugly, and also a potential security risk.
                app.UseDeveloperExceptionPage();
            }

            // Automatically tests all configured mappings set up through AutoMapper. This looks for at registered mappings
            // and validates that a proper mapping of all properties is being performed. This will throw an exception
            // if any mapping is invalid, halting execution at startup rather than at request time.
            mapper.ConfigurationProvider.AssertConfigurationIsValid();

            // The enables azure app configurations client library to refresh feature flag values at the configured interval.
            app.UseAzureAppConfiguration();

            // The below methods all register some of the built-in middleware classes from the framework with
            // the application. In .Net Core, middleware is used to perform actions against ALL requests coming
            // into the system, regardless of their targeted controller/action. The framework
            // gathers all middleware registered in this method and orders them according to the order each
            // middleware is registered. This bears repeating:
            //
            // **The order in which middleware is registered dictates the order in which middleware will be executed.**
            //
            // One thing to keep in mind when writing middleware is performance. Any code you put into a middleware class
            // will be run for every single request coming into the system. Be mindful not to perform any long-running
            // tasks that can't be done on a separate thread (and if it can be done on a separate thread, be sure to do so).

            // UseRouting() injects the middleware that will be responsible for establishing routing rules for requests based on
            // the incoming request path. Per the framework rules, this middleware MUST be declared before UseEndpoints().
            // Any middleware in between UseRouting() and UseEndpoints() can alter these routing rules as needed.
            app.UseRouting();

            // The UseAuthorization() middleware is responsible for establishing a users `Identity`, which can be found on
            // `HttpContext.User.Identity` while inside a controller. In our case, since we never established any authentication
            // or actual authorization rules, all users will be given an Anonymous identity by default.
            app.UseAuthorization();

            // This extension method is part of this repository and injects a middleware class responsible for logging request data.
            app.UseRequestLoggingMiddleware();

            // UseEndpoints() should be one of the last middleware in the chain, as it is responsible for actually routing
            // requests to the appropriate controller based on the routing rules that were initially established by UseRouting(),
            // and optionally altered by any middleware in between.
            app.UseEndpoints(endpoints =>
            {
                // This method is what establishes the convention-based routing of actions, alleviating the need
                // for direct [Route] attributes on every action.
                endpoints.MapControllers();
            });
        }
    }
}
