using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Bcss.Reference.Grpc.Server.Interceptors
{
    /// <summary>
    /// Grpc Interceptors are very similar to Http Middleware in that they allow code to interact with all incoming or outgoing requests/responses.
    /// The primary difference is that while http middleware acts on the layer of an Http request (using the HttpContext), gRPC interceptors work
    /// at the gRPC layer, so they have different hooks to handle different types of gRPC requests/responses.
    ///
    /// Here, we've implemented a server interceptor which our gRPC server will use to log request header information. Client and Server gRPC
    /// interceptors are different only in what methods they override. You could also implement an interceptor that can act on both clients and servers,
    /// should you so desire.
    /// </summary>
    public class RequestLoggingGrpcServerInterceptor : Interceptor
    {
        private readonly ILogger<RequestLoggingGrpcServerInterceptor> _logger;

        public RequestLoggingGrpcServerInterceptor(ILogger<RequestLoggingGrpcServerInterceptor> logger)
        {
            _logger = logger;
        }

        public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context, ClientStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogRequest(context);
            return base.ClientStreamingServerHandler(requestStream, context, continuation);
        }

        public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogRequest(context);
            return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
        }

        public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            LogRequest(context);
            return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
        }

        public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            LogRequest(context);
            return base.UnaryServerHandler(request, context, continuation);
        }

        private void LogRequest(ServerCallContext context)
        {
            var stringBuilder = new StringBuilder();

            foreach (var header in context.RequestHeaders)
            {
                stringBuilder.Append($"\tHeader Name: {header.Key}\n");
                stringBuilder.Append($"\tHeader Value: {header.Value}\n");
            }

            _logger.LogInformation("Grpc Request headers:\n{headers}", stringBuilder.ToString());
        }
    }
}