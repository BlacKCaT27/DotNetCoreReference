using System;
using System.Collections.Generic;
using Bcss.Reference.Bootstrap;
using Bcss.Reference.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bcss.Reference.Web.Test
{
    public static class TestServerFactory
    {
        /// <summary>
        /// A helper method to consolidate the configuration of a test server.
        /// </summary>
        /// <param name="seedData">An action which tests can specify as needed to seed the mock grpc service 'database' with data.</param>
        /// <param name="testServices">An optional IServiceCollection which will be injected into the web application factories test services. Use this collection to specify any additional mocks you may want to use.</param>
        /// <param name="configBuilder">An optional IConfigurationBuilder to allow tests to override various configuration parameters as needed.</param>
        /// <param name="useAzureAppConfig">A flag indicating whether Azure App Configuration should be used. Defaults to false.</param>
        /// <returns></returns>
        public static WebApplicationFactory<Startup> CreateWebApplicationFactory(Action<IDictionary<int, WeatherForecastData>> seedData = null, IServiceCollection testServices = null, IConfigurationBuilder configBuilder = null, bool useAzureAppConfig = false)
        {
            // We can use an environment variable to drive the use of app config since Env Vars are pulled in prior to azure app config being configured.
            Environment.SetEnvironmentVariable("UseAzureAppConfiguration", useAzureAppConfig.ToString());
            testServices ??= new ServiceCollection();

            // Here, we create a MockGrpcWeatherService object which will take the place of our grpc client for the web, since the actual
            // grpc server won't be running for our tests.
            var mockGrpcService = new MockGrpcWeatherService(seedData);

            // The WebApplicationFactory class is an extremely powerful testing tool. This tool allows us to spin up `TestServer` objects
            // while providing hooks to let us tweak the servers configuration in any way our tests need. We can override service mappings to inject mocks,
            // alter configuration parameters, change the url the server listens to, etc. Essentially, you can think of this class as a substitute for
            // Program.cs.
            //
            // Here, we're going to inject a mock grpc weather service to act as a stand-in for our gRPC client, since there's no actual gRPC server to talk to
            // when tests are running.
            return new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    // The builder object exposes the same methods found in `Program.cs`, but also exposes additional methods for testing.
                    // ConfigureTestServices runs after ConfigureServices, and allows us to specify mappings to the DI framework just as we would
                    // inside Startup.cs. Since ConfigureTestServices runs later, however, it allows us to override the mappings found in Startup.cs.
                    // This makes it trivial to inject mocks to change the behavior of the system however needed.
                    builder.ConfigureTestServices(services =>
                    {
                        // We have to follow the same pattern seen in Startup.cs to ensure we are referencing the same object for both interfaces.
                        services.AddSingleton(mockGrpcService);
                        services.AddSingleton<IWeatherForecastReader>(provider => provider.GetRequiredService<MockGrpcWeatherService>());
                        services.AddSingleton<IWeatherForecastWriter>(provider => provider.GetRequiredService<MockGrpcWeatherService>());

                        foreach (var testServiceDescriptor in testServices)
                        {
                            services.Add(testServiceDescriptor);
                        }
                    });

                    builder.ConfigureAppConfiguration((context, config) =>
                    {
                        if (configBuilder == null)
                        {
                            return;
                        }

                        foreach (var source in configBuilder.Sources)
                        {
                            config.Sources.Add(source);
                        }

                        foreach (var property in configBuilder.Properties)
                        {
                            config.Properties.Add(property);
                        }
                    })
                    .UseEnvironment("Development")
                    .UseUrls("http://localhost:5000");
                });
        }
    }
}