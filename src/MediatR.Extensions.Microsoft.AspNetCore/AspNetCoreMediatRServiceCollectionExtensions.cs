﻿
using System.Reflection;
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
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the provided assemblies for handlers and requests
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="assemblies">Assemblies to scan for requests and handlers</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, params Assembly[] assemblies)
        => mvcBuilder.AddMediatRUsingRequestAbortedCancellationToken(null, assemblies);

    /// <summary>
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the provided assemblies for handlers and requests
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="configuration">The action used to optionally configure the MediatR options</param>
    /// <param name="assemblies">Assemblies to scan for requests and handlers</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, Action<MediatRServiceConfiguration>? configuration, params Assembly[] assemblies)
    {
        var serviceConfig = new MediatRServiceConfiguration();
        configuration?.Invoke(serviceConfig);

        mvcBuilder.Services.AddMediatR(assemblies, configuration);
        ConfigureMediatorDecorator(mvcBuilder.Services, serviceConfig);

        return mvcBuilder;
    }

    /// <summary>
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the assemblies of the provided types for handlers and requests
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="handlerAssemblyMarkerTypes">Assembly handler marker types</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, params Type[] handlerAssemblyMarkerTypes)
        => mvcBuilder.AddMediatRUsingRequestAbortedCancellationToken(handlerAssemblyMarkerTypes, null);

    /// <summary>
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the assemblies of the provided types for handlers and requests
    /// </summary>
    /// <param name="mvcBuilder">Mvc Builder</param>
    /// <param name="configuration">The action used to optionally configure the MediatR options</param>
    /// <param name="handlerAssemblyMarkerTypes">Assembly handler marker types</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, Action<MediatRServiceConfiguration> configuration, params Type[] handlerAssemblyMarkerTypes)
        => mvcBuilder.AddMediatRUsingRequestAbortedCancellationToken(handlerAssemblyMarkerTypes, configuration);

    /// <summary>
    /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the assemblies of the provided types for handlers and requests
    /// </summary>
    /// <param name="mvcBuilder">>Mvc Builder</param>
    /// <param name="handlerAssemblyMarkerTypes">Assembly handler marker types</param>
    /// <param name="configuration">The action used to optionally configure the MediatR options</param>
    /// <returns></returns>
    public static IMvcBuilder AddMediatRUsingRequestAbortedCancellationToken(
        this IMvcBuilder mvcBuilder, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration>? configuration)
        => mvcBuilder.AddMediatRUsingRequestAbortedCancellationToken(configuration, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly).ToArray());

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
