using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bcss.Reference.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bcss.Reference.Web.Middleware
{
    /// <summary>
    /// This class is a middleware implementation responsible for logging the request data coming in for every request.
    /// This includes the targeted request path as well as the request body, if one exists.
    ///
    /// Middleware classes do not implement an explicit interface. Instead, they must simply implement a method with the following signature:
    ///
    /// public async Task Invoke(HttpContext httpContext)
    ///
    /// Additionally, they must take in a `RequestDelegate` object in their constructor (though they're not limited to only that dependency, see below).
    ///
    /// The classes are then registered within the Configure() method of Startup.cs with the `app.UseMiddleware()` method, or, more appropriately,
    /// via an extension method which wraps that call. The extension method makes the code within Startup.cs read more cleanly, which is important
    /// since over time, Startup.cs can quickly grow to be very large, and being able to understand the code flow quickly and at a glance becomes even
    /// more crucial. /> 
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// Instantiates a new instance of <see cref="RequestLoggingMiddleware"/>.
        ///
        /// Note that since middleware aren't actually used until after app initialization, they're still
        /// candidates for dependency injection. We can take advantage of this fact here and obtain a logger
        /// with which we can capture request data.
        /// </summary>
        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to perform its task before it either awaits the RequestDelegate or short-circuits the
        /// request (see below).
        ///
        /// Note that since the Invoke method is a special method in terms of the framework and its interactions with middleware,
        /// the framework allows for dependency injection into the Invoke method as well as the constructor. This can be useful
        /// when you want to access a scoped object from within a middleware class (which are registered as singletons).
        /// However, you must be sure to never store a reference to the object provided by the DI framework, as it will be destroyed
        /// before the next request comes in.
        /// </summary>
        /// 
        /// <param name="httpContext">HttpContext objects represent the lifecycle of a given request,
        /// and contain references to all request and response information. They also contain user and session information,
        /// as well as information about the connection over which the request was made.
        ///
        /// In other words, HttpContext objects encapsulate all forms of interaction with the caller.</param>
        /// 
        /// <param name="optionsSnapshot">
        /// The Options system allows you to request either an `IOptions`, `IOptionsSnapshot`,
        /// or `IOptionsMonitor` object with the bound configuration class contained within.
        /// IOptions objects allow you to access the configuration values provided by the configuration system.
        ///
        /// IOptionsSnapshots also allow this, but also monitor for changes to configuration values and
        /// will automatically reflect those changes in the first request after the change is made. This class
        /// is registered as a Scoped service, so it cannot be consumed by singletons.
        ///
        /// IOptionsMonitor, on the other hand, performs similarly to IOptionsSnapshot in that you can receive
        /// real-time changes to configuration values by subscribing to an event on the object. IOptionsMonitor
        /// is a singleton and can thus be consumed by other singletons.</param>
        public async Task Invoke(HttpContext httpContext, IOptionsSnapshot<RequestHandlingOptions> optionsSnapshot)
        {
            var maxContentLength = optionsSnapshot.Value.MaxRequestBodySizeToLog;
            await LogRequest(httpContext.Request, maxContentLength);

            // It is the responsibility of any implemented middleware to do one of two things with the request delegate:
            // either await it, or ignore it.
            //
            // Awaiting the request delegate is how the middleware system chains all the middleware together. Examining the stack trace
            // from within a given middleware class will show you the exact order the middleware were invoked in (which can be useful
            // if you're seeing strange behavior you weren't expecting).
            //
            // If you do NOT await the request delegate, and do something else such as throw an exception, the request will not proceed
            // any further through the system and the application will return whatever response it has built thus far to the caller.
            // This is known as "short-circuiting" the request pipeline, and it can be a valuable but potentially hazardous technique.
            // Middleware implementers must take great care to ensure they can always await the request delegate unless they are purpose built
            // to detect a state where short-circuiting is desired behavior (such as a global exception handler).
            await _next(httpContext);
        }

        private async Task LogRequest(HttpRequest httpRequest, long maxContentLength)
        {
            if (httpRequest == null)
            {
                return;
            }

            // Here, we see the Options system in action. We reference the current value of the bound configuration object
            // with the .Value property, then can reference the configuration data as a strongly typed property of our configuration
            // object.

            _logger.LogInformation("Request Path: {path}", httpRequest.Path);

            // The null-coalescing operator (??) can also utilize nullable value types by checking the `HasValue` flag of a nullable type
            // to determine the 'null' state of the object.
            var contentLength = httpRequest.ContentLength ?? 0;
            _logger.LogInformation("Request Content Length: {ContentLength}", contentLength);

            if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength.Value != 0)
            {
                // Since the request body is a stream, we want to avoid unpacking very large request bodies to ensure we don't use too much memory
                // just to log the body. Instead, we'll log a warning that the limit was exceeded.
                if (httpRequest.ContentLength.Value > maxContentLength)
                {
                    _logger.LogWarning("Request body size of '{ContentLength}' exceeds maximum configured value '{MaxContentLength}'...omitting body from logs.",
                        contentLength, maxContentLength);
                    return;
                }

                httpRequest.EnableBuffering();

                // Request and response bodies are implemented as streams which can, depending on the request, be streamed directly back to the caller.
                // Be sure to leave the stream open so other middleware (including the Endpoints middleware which actually unpacks the request) can read it.
                using var reader = new StreamReader(httpRequest.Body, Encoding.UTF8, false, 1024, true);
                var body = await reader.ReadToEndAsync();

                _logger.LogInformation("Request body: {body}", body);

                // Make sure we seek back to the start of the stream so other consumers of the request body can access it.
                httpRequest.Body.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}