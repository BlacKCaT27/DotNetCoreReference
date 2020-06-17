using Microsoft.AspNetCore.Builder;

namespace Bcss.Reference.Web.Middleware
{
    /// <summary>
    /// This class is responsible for providing extension methods to inject various middleware classes into the request pipeline.
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds request logging middleware that logs information about all incoming requests.
        /// This extension method follows a standard convention of being named "Use(name of the middleware)".
        /// Be sure to follow this convention when exposing new middleware for consistency.
        /// </summary>
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}