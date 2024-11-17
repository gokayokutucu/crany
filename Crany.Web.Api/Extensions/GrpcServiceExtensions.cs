using Crany.Shared.Protos;

namespace Crany.Web.Api.Extensions;


/// <summary>
/// Extension methods for gRPC services
/// </summary>
public static class GrpcServiceExtensions
{
    /// <summary>
    /// Add gRPC client to the service collection
    /// </summary>
    /// <param name="serviceCollection">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns></returns>
    /// <exception cref="MissingFieldException"></exception>
    public static IServiceCollection AddGrpcClient(
        this IServiceCollection serviceCollection, IConfiguration configuration
    )
    {
        var host =
           configuration.GetSection("Grpc:Endpoint:Host").Value
            ?? throw new MissingFieldException("'Grpc:Endpoint:Host' is missing in the configuration");
        
        var port =
            configuration.GetSection("Grpc:Endpoint:port").Value
            ?? throw new MissingFieldException("'Grpc:Endpoint:Port' is missing in the configuration");

        serviceCollection.AddGrpcClient<AuthService.AuthServiceClient>(
                (_, options) => { options.Address = new Uri($"https://{host}:{port}"); }
            )
            .ConfigureChannel(o =>
            {
                o.HttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                };
            });

        return serviceCollection;
    }
}