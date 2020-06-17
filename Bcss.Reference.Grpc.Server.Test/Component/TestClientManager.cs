using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Bcss.Reference.Grpc.Server.Test.Component
{
    /// <summary>
    /// This class is responsible for managing a test client of a given type.
    ///
    /// Because WebApplicationFactory implements IDisposable, we also implement it
    /// to give test classes a way to ensure the factory is properly disposed at runtime.
    /// 
    /// Test classes should create TestClientManager during OneTimeSetUp and `Dispose()` during
    /// OneTimeTearDown to ensure factories don't end up leaking resources during the test run.
    ///
    /// Note that this class is merely being used as an example of how you could share the factory creation across
    /// tests. You could easily extend the capabilities of this class to allow tests to override internal behaviors of the server
    /// just as is done in `TestServerFactory` in Bcss.Reference.Web.Test, as needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TestClientManager<T> : IDisposable where T : class
    {
        // As part of a proper implementation of IDisposable, we must track if we've already been disposed.
        private bool _isDisposed;

        private T _instance;
        public T Instance => _instance ??= CreateClient();

        private WebApplicationFactory<Startup> _factory;

        public TestClientManager()
        {
            _instance = CreateClient();
        }

        // The two methods below showcase how to properly implement the Dispose pattern in .Net Core.
        // This pattern is used to help the garbage collector properly clean up any unmanaged resources
        // and help prevent memory leaks. While the interface for `IDisposable` only requires you to implement
        // `Dispose()`, the compiler will trigger warnings if you do not properly implement the full pattern as described by
        // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose
        // and
        // https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1063?view=vs-2019

        /// <summary>
        /// This method is made public with no boolean argument to keep the Dispose implementation wholly encapsulated
        /// within its implementers. As discussed below in the protected Dispose method, a class implementing IDisposable
        /// must clean up both managed and unmanaged resources, but sometimes under different circumstances. By wrapping
        /// the boolean argument with this public method, we can hide those circumstances from client classes who shouldn't
        /// need to know know if the boolean should be 'true' or 'false'.
        /// </summary>
        public void Dispose()
        {
            // Here, we're calling our protected Dispose method with a value of 'true' to indicate that this is an
            // explicit disposing of the object by our application, and not just the garbage collector freeing up an
            // unreferenced object.
            Dispose(true);

            // In the case where our application disposes of an object in application code, we tell the garbage collector
            // not to use this classes Finalizer (similar to a deconstructor in other languages, though something you should rarely implement explicitly
            // unless you're dealing with unmanaged resources). If we failed to suppress the finalizer, the default finalizer implementation
            // would attempt to call dispose again, which, if we had any references to unmanaged memory, would cause a null pointer exception.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method must be implemented to satisfy the compiler because it's the only way to safely ensure that
        /// any future child classes will can have the ability to properly clean up their parent classes via Dispose().
        /// </summary>
        protected void Dispose(bool disposing)
        {
            // Ensure we don't get disposed of multiple times, which could cause memory leaks or null pointer exceptions.
            if (_isDisposed)
            {
                return;
            }

            // Only call Dispose() on other managed objects if we're told to.
            if (disposing)
            {
                // free managed resources
                _factory.Dispose();
            }

            // This location in the code is where you should always clean up any unmanaged resources, regardless of the state of
            // the `disposing` parameter.
            // If your code does hold references to unmanaged resources, you MUST also implement the finalizer yourself as follows:
            //
            // public ~TestClientManager()
            // {
            //     Dispose(false);
            // }
            //
            // This ensures that you will always clean up unmanaged resources when being destroyed by the garbage collector,
            // preventing memory leaks. If your class does NOT directly handle unmanaged resources, do not implement the finalizer.

            _isDisposed = true;
        }

        /// <summary>
        /// Creates a gRPC client of type T and returns it.
        ///
        /// There are several important tweaks we must make to the factory and the client in order to get the calls
        /// to complete properly. Most of these tweaks are around setting up the connection between the client and TestServer.
        /// GRPC requires Http/2 which is not enabled by default. See the comments below for the necessary steps.
        /// </summary>
        /// <returns></returns>
        private T CreateClient()
        {
            Environment.SetEnvironmentVariable("UseAzureAppConfiguration", "false");

            const string host = "http://localhost:5001";

            _factory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    // We must configure our gRPC Service's Kestrel web server to use Http/2 by default.
                    builder.ConfigureKestrel(options =>
                    {
                        options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
                    });

                    // We must also configure the url the server will listen on to match the url the client will point to.
                    builder.UseUrls(host);
                });

            // We make a custom HttpResponseHandler which will let us maintain the Http/2 version for both requests and responses.
            var responseVersionHandler = new ResponseVersionHandler
            {
                InnerHandler = _factory.Server.CreateHandler()
            };

            // Rather than using the HttpClient provided by the TestServer, we'll make our own client that can properly make use
            // of Http/2, pointed at the same host as the server.
            var client = new HttpClient(responseVersionHandler)
            {
                BaseAddress = new Uri(host)
            };

            // Here, we're making use of the "Grpc.Net.ClientFactory" NuGet package, which lets us make use of the ServiceCollection extension method
            // to create a gRPC client.
            var services = new ServiceCollection();
            services.AddGrpcClient<T>(options =>
                {
                    options.Address = new Uri(host);
                })
                .ConfigureChannel(channelOptions =>
                {
                    channelOptions.HttpClient = client;
                    channelOptions.Credentials = ChannelCredentials.Insecure;
                });

            // Finally, we build the ServiceProvider from the collection and use it to obtain a reference to our client.
            var provider = services.BuildServiceProvider();
            var weatherClient = provider.GetRequiredService<T>();
            return weatherClient;
        }
    }

    /// <summary>
    /// A custom HTTP Handler responsible for ensuring that the response Http version always matches the version of the request.
    /// </summary>
    public class ResponseVersionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            response.Version = request.Version;

            return response;
        }
    }
}