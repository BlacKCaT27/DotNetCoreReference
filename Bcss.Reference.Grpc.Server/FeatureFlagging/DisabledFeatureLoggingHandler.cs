using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement.Mvc;

namespace Bcss.Reference.Grpc.Server.FeatureFlagging
{
    /// <summary>
    /// This class implements a DisabledFeaturesHandler interface to log warnings when
    /// callers attempt to access a blocked feature.
    /// </summary>
    public class DisabledFeatureLoggingHandler : IDisabledFeaturesHandler
    {
        private readonly ILogger<DisabledFeatureLoggingHandler> _logger;

        public DisabledFeatureLoggingHandler(ILogger<DisabledFeatureLoggingHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleDisabledFeatures(IEnumerable<string> features, ActionExecutingContext context)
        {
            foreach (var feature in features)
            {
                _logger.LogWarning("Caller with IP Address {ipAddress} attempted to access feature {feature} but was blocked.", context.HttpContext.Connection.RemoteIpAddress, feature);
            }

            // Hide the blocked feature behind a 404.
            context.HttpContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
            return Task.CompletedTask;
        }
    }
}