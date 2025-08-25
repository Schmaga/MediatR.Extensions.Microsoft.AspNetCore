using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MediatR.Extensions.Microsoft.AspNetCore.Mediator;

namespace MediatR.Extensions.Microsoft.AspNetCore;

/// <summary>
/// Entry point for registration extension methods that work on top of IMvcBuilder
/// </summary>
public static class AspNetCoreMediatRServiceCollectionExtensions
{
    /// <summary>
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, using the given configuration
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="configuration">The action used to optionally configure the MediatR options</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, Action<MediatRServiceConfiguration> configuration)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        configuration.Invoke(serviceConfig);

        mvcBuilder.Services.AddMediatR(configuration);
        ConfigureMediatorDecorator(mvcBuilder.Services, serviceConfig);

        return mvcBuilder;
    }

    private static void ConfigureMediatorDecorator(IServiceCollection services, MediatRServiceConfiguration serviceConfiguration)
    {
        services.Add(new ServiceDescriptor(serviceConfiguration.MediatorImplementationType, serviceConfiguration.MediatorImplementationType, serviceConfiguration.Lifetime));
        services.AddScoped<IMediator, RequestAbortedCancellationTokenMediatorDecorator>(
            provider =>
            {
                var mediator = (IMediator?)provider.GetService(serviceConfiguration.MediatorImplementationType)
                    ?? throw new InvalidOperationException($"Could not resolve Mediator implementation {serviceConfiguration.MediatorImplementationType} from service provider");
                var httpContextAccessor = provider.GetService<IHttpContextAccessor>()
                    ?? throw new InvalidOperationException("Could not resolve HttpContextAccessor from service provider.");
                return new RequestAbortedCancellationTokenMediatorDecorator(mediator, httpContextAccessor);
            });
    }
}
