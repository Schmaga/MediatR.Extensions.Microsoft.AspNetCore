# MediatR.Extensions.Microsoft.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/MediatR.Extensions.Microsoft.AspNetCore)](https://www.nuget.org/packages/MediatR.Extensions.Microsoft.AspNetCore/)

# Project summary

Some useful extensions to make the integration of MediatR into ASP.NET Core Application easier. It currently only adds one new feature
 to the integration of MediatR into ASP.NET Core: Automatic wire-up of the HttpContext.RequestAborted CancellationToken into
the the handlers. I might add a few more features in the future as well, as I already have some ideas. 

As this library is in an early stage, there might also be some breaking changes in the future.

## Integration into ASP.NET Core

The extension is registered in an ASP.NET Core Application like this:
```
services
    .AddControllers()
    .AddMediatRUsingRequestAbortedCancellationToken(config => config.AsScoped(), typeof(Startup).Assembly)
```

**Important:** You will not have to register MediatR using _AddMediatR_ now anymore. So remove that call from ConfigureServices and move
any configuration you might have to the overloads of _AddMediatRUsingRequestAbortedCancellationToken_.

## Some more explanation

This library removes the need to explicitly pass the CancellationToken in Controllers on every call manually, which until now had to be like this:
```
mediator.Send(request, HttpContext.RequestAborted);
mediator.Publish(notification, HttpContext.RequestAborted);
```
Adding this was easily forgotten. Using this library, calls to MediatR can simply be used like this:
```
mediator.Send(request);
mediator.Publish(notification);
```
And the library will pass the _HttpContext.RequestAborted_ _CancellationToken_ to the handlers.

The integration was designed to be as simple as possible, and tries to keep all the existing MediatR extension methods for registering.
To avoid confusion with the regular MediatR registration and to explicitly make it clear
that this library depends on the ASP.NET Core API (because it makes use of the _IHttpContextAccessor_), the registration of this library is
based of the _IMvcBuilder_ interface returned by the _AddControllers_ methods of the IServiceCollection.


