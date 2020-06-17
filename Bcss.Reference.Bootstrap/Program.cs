using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bcss.Reference.Bootstrap
{
    public class Program
    {
        /// <summary>
        /// Main is the very first entry point of the application. This is where it all starts.
        /// </summary>
        public static void Main(string[] args)
        {
            // .Net Core applications are all initialized using a common "IHostBuilder" object which
            // is a builder responsible for building your applications object graph at runtime.
            // Here, we call CreateHostBuilder to obtain a reference to the `IHostBuilder`, then call Build()
            // to create the `IHost`, and finally Run() to start the application.
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Kestrel is a built-in HTTP Server which is provided with the .Net Core framework, akin to
                    // nginx or Apache.
                    webBuilder.UseKestrel()
                        // ContentRoot is the location where web requests will look for static files such as .css or .js files.
                        // As we're developing an API and not an application that will serve a browser directly, this will be used
                        // solely to locate the configuration json files (see below).
                        .UseContentRoot(Directory.GetCurrentDirectory())

                       /*
                        * .Net Core's configuration system has an extremely flexible layered architecture. At a high level, you register
                        * 1-N "IConfigurationProvider" objects, which are responsible for retrieving configuration information from 'somewhere'
                        * (e.g. a json file, environment variables, or even a remote server), which are then fed as key-values pairs into
                        * a dictionary shared across all configuration providers. Once all providers have reported their entries, the collection
                        * is 'flattened', and any conflicting keys are resolved by "last one wins". Keep in mind that means that the order
                        * of provider registration is critical.
                        * 
                        * Configuration data can be broken up into "sections". These represent logical groupings of configuration data that can
                        * be worked with in isolation. Sections can also have one-more "sub-sections", with no limit to the amount of nesting allowed.
                        * 
                        * Using the json file as an example, each json key-value pair is a configuration key/value,
                        * and each json object is a section. So "Logging" is a section, "LogLevel" is a sub-section of the "Logging" section,
                        * and `"Default": "Information"` is an actual configuration parameter.
                        * 
                        * Each Configuration Provider has its own rules for how to declare sections and subsections, which will be explained below.
                        */
                        .ConfigureAppConfiguration((builderContext, config) =>
                        {
                            var env = builderContext.HostingEnvironment;

                            /*
                             * This value is set by the `ASPNETCORE_ENVIRONMENT` environment variable, and is
                             * "Development" by default. Note: This env var should always be set to "Production" in a production
                             * environment, as its value drives behaviors both within application code and framework code, and
                             * "Production" will apply several optimizations that are omitted in "Development" builds which can
                             * greatly enhance the speed of an application.
                             */
                            var environmentName = env.EnvironmentName;
                            
                           /*
                            * Here we're adding three `IConfigurationProvider`s which are all provided by the framework.
                            * .AddJsonFile takes a path to a json file (relative to ContentRoot). The `optional` parameter
                            * when set to false will result in the app failing to start if the file cannot be found. Set to true,
                            * the ConfigurationProvider simply returns no key-value pairs and execution will not be stopped.
                            * The `reloadOnChange` parameter attaches file watchers to the json files and any updates will result
                            * in the changes being re-read at runtime (note: this requires use of the "Options" pattern which will be explained
                            * in another area of the code.
                            * 
                            * The .AddEnvironmentVariables() provider feeds all environment variables from the system the application is
                            * running on to the configuration system. In order to represent different configuration sections, environment
                            * variables should be declared with names using either `:` (on windows/mac) or `__` (linux. Note that is TWO underscore characters)
                            * to denote sections/subsections. Following with the logging example from above, to set the Default log level, the environment
                            * variable should be set as:
                            * 
                            * `Logging__LogLevel__Default = "Information"`
                            * 
                            * Note that since `AddEnvironmentVariables()` was called *after* `AddJsonFile()`, any collisions between key names that occur
                            * will result in the environment variables values being given priority.
                            *
                            * AddAzureAppConfiguration() configures the system to also make use of the Azure App Configuration system. The Azure App Configuration
                            * NuGet package (which the Web assembly utilizes) provides feature flag capabilities, which we configure here as well.
                            */
                           config.AddJsonFile("appsettings.json", false, true)
                               .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                               .AddEnvironmentVariables();

                            // Note that it is possible to obtain a snapshot of the current configuration values by calling `Build()`, even while initializing
                            // other configuration providers, as seen here.
                            var settings = config.Build();

                            if (!settings.GetValue<bool>("UseAzureAppConfiguration"))
                            {
                                return;
                            }

                            config.AddAzureAppConfiguration(options =>
                               {
                                   // We have to provide the connection string to Azure App Config via either the JSON files or an Environment Variable, but once provided,
                                   // all other configuration can be centrally managed by Azure App Config.
                                   // Normally you wouldn't want the app config client to invalidate its caches this quickly (a better timespan would be 5 minutes,
                                   // but this is ultimately a business decision. The short timespans used here are purely for demonstration purposes.
                                   options.Connect(settings["ConnectionStrings:AppConfig"])
                                       .UseFeatureFlags(featureFlagOptions =>
                                       {
                                           // While not showcased here, the Feature Flag system provided by Azure App Config also supports the notion of "filters" which
                                           // will be applied across all requests to filter out some requests that would otherwise be allowed to access the feature. This can
                                           // be used, for example, to implement slow-rolling of features, where features are only enabled to a small sub-set of users at a time
                                           // in case issues are detected.
                                           featureFlagOptions.CacheExpirationTime = TimeSpan.FromSeconds(5);
                                       })

                                       // ConfigureRefresh allows us to configure how often and under what conditions the app config library will refresh data from Azure App Config.
                                       // We first register a special configuration entry which is referred to as a "Sentinel". The sentinel is a marker entry which allows users
                                       // altering configuration data to "flip the switch" and have all configuration data updated at once. This will also limit the amount of network
                                       // traffic the library creates as it will ONLY monitor the sentinel for changes, and when a change is detected, only then will it will refresh all
                                       // the other entries. This is a much safer approach than letting all entries be monitored, since there may be times when multiple configuration values
                                       // are required to be changed in unison (such as setting the account name and access key to blob storage, for example). This gives the maintainer that control.
                                       .ConfigureRefresh(refresh =>
                                       {
                                           refresh.Register(KeyFilter.Any, refreshAll: true)
                                               .SetCacheExpiration(TimeSpan.FromSeconds(5));
                                       });
                               });
                        })

                        /*
                         * The Logging system follows a similar Provider pattern to that of Configuration, with the additional abstraction of a builder pattern
                         * which creates the providers. `ILoggerProvider`s provide an implementation of an `ILogger` which is the object responsible for
                         * the actual log output. To use the logging system, classes need merely request an `ILogger<{className}>` object in their constructors,
                         * and the dependency injection framework will provide them with a wrapper implementation that then dispatches log messages to each
                         * registered provider.
                         *
                         * One important fact to note is that because both configuration and logging are bootstrapped in the same location (and can be bootstrapped in
                         * any order), Logging is not able to access configuration data (and visa versa). Keep this in mind when writing Logger or Configuration providers.
                         */
                        .ConfigureLogging((hostingContext, logging) =>
                        {
                            // Logging Configuration drives various logging parameters, some shared across all providers (such as `LogLevel`), making
                            // use of the configuration system to obtain the actual settings at runtime. Logging Providers can provide hooks to allow
                            // for applications to set provider-specific keys as well, which will be ignored by loggers that don't support them.
                            logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

                            // The Console logger is responsible for outputting log messages to a terminal (which is done by writing to stdout and stderr).
                            logging.AddConsole();

                            // The Debug logger is responsible for writing messages to the debug window (the window that appears when running the application) when
                            // the application is being debugged.
                            logging.AddDebug();
                        })

                        // UseStartup allows us to specify which class should be treated as the Startup class that will be used for further application initialization.
                        .UseStartup<Startup>();
                });
    }
}
