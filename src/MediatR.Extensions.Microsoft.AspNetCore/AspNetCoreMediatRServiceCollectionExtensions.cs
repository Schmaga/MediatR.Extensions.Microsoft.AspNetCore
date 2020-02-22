namespace MediatR.Extensions.Microsoft.AspNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using global::Microsoft.AspNetCore.Http;
    using global::Microsoft.Extensions.DependencyInjection;
    using MediatR.Extensions.Microsoft.AspNetCore.Mediator;

    public static class AspNetCoreMediatRServiceCollectionExtensions
    {
        /// <summary>
        /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the provided assemblies for handlers and requests
        /// </summary>
        /// <param name="mvcBuilder">Mvc Builder</param>
        /// <param name="assemblies">Assemblies to scan for requests and handlers</param>
        /// <returns></returns>
        public static IMvcBuilder AddAndConfigureMediatRToUseRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, params Assembly[] assemblies)
            => mvcBuilder.AddAndConfigureMediatRToUseRequestAbortedCancellationToken(null, assemblies);

        /// <summary>
        /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the provided assemblies for handlers and requests
        /// </summary>
        /// <param name="mvcBuilder">Mvc Builder</param>
        /// <param name="configuration">The action used to optionally configure the MediatR options</param>
        /// <param name="assemblies">Assemblies to scan for requests and handlers</param>
        /// <returns></returns>
        public static IMvcBuilder AddAndConfigureMediatRToUseRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, Action<MediatRServiceConfiguration> configuration, params Assembly[] assemblies)
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
        public static IMvcBuilder AddAndConfigureMediatRToUseRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, params Type[] handlerAssemblyMarkerTypes)
            => mvcBuilder.AddAndConfigureMediatRToUseRequestAbortedCancellationToken(handlerAssemblyMarkerTypes, null);

        /// <summary>
        /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the assemblies of the provided types for handlers and requests
        /// </summary>
        /// <param name="mvcBuilder">Mvc Builder</param>
        /// <param name="configuration">The action used to optionally configure the MediatR options</param>
        /// <param name="handlerAssemblyMarkerTypes">Assembly handler marker types</param>
        /// <returns></returns>
        public static IMvcBuilder AddAndConfigureMediatRToUseRequestAbortedCancellationToken(this IMvcBuilder mvcBuilder, Action<MediatRServiceConfiguration> configuration, params Type[] handlerAssemblyMarkerTypes)
            => mvcBuilder.AddAndConfigureMediatRToUseRequestAbortedCancellationToken(handlerAssemblyMarkerTypes, configuration);

        /// <summary>
        /// Registers MediatR which is configured to use the HttpContext.RequestAborted CancellationToken in ASP.NET Core Environments, and scans the assemblies of the provided types for handlers and requests
        /// </summary>
        /// <param name="mvcBuilder">>Mvc Builder</param>
        /// <param name="handlerAssemblyMarkerTypes">Assembly handler marker types</param>
        /// <param name="configuration">The action used to optionally configure the MediatR options</param>
        /// <returns></returns>
        public static IMvcBuilder AddAndConfigureMediatRToUseRequestAbortedCancellationToken(
            this IMvcBuilder mvcBuilder, IEnumerable<Type> handlerAssemblyMarkerTypes, Action<MediatRServiceConfiguration> configuration)
            => mvcBuilder.AddAndConfigureMediatRToUseRequestAbortedCancellationToken(configuration, handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly).ToArray());

        private static void ConfigureMediatorDecorator(IServiceCollection services, MediatRServiceConfiguration serviceConfiguration)
        {
            services.Add(new ServiceDescriptor(serviceConfiguration.MediatorImplementationType, serviceConfiguration.MediatorImplementationType, serviceConfiguration.Lifetime));
            services.AddScoped<IMediator, RequestAbortedCancellationTokenMediatorDecorator>(
                provider => new RequestAbortedCancellationTokenMediatorDecorator(
                    (IMediator) provider.GetService(serviceConfiguration.MediatorImplementationType),
                    provider.GetService<IHttpContextAccessor>()));
        }
    }
}
