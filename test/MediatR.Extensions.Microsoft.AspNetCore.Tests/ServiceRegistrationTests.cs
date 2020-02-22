namespace MediatR.Extensions.Microsoft.AspNetCore.Tests
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using global::Microsoft.Extensions.DependencyInjection;
    using MediatR.Extensions.Microsoft.AspNetCore.Mediator;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceRegistrationTests
    {
        [Test]
        public void Ensure_that_the_decorator_replaces_default_mediator_implementation_after_registration()
        {
            var services = new ServiceCollection();
            services.AddControllers().AddMediatRUsingRequestAbortedCancellationToken(typeof(FakeRequest).Assembly);
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            mediator.Should().BeOfType<RequestAbortedCancellationTokenMediatorDecorator>();
        }

        [Test]
        public void Ensure_that_the_decorator_will_have_the_provided_mediator_implementation_injected()
        {
            var services = new ServiceCollection();
            services.AddControllers().AddMediatRUsingRequestAbortedCancellationToken(
                config => { config.Using<MyFakeMediator>().AsSingleton(); },
                typeof(FakeRequest).Assembly);
            var provider = services.BuildServiceProvider();
            var mediator = provider.GetService<IMediator>();
            var mediatorDecorator = provider.GetService<MyFakeMediator>();
            var fakeRequest = new FakeRequest();

            mediator.Send(fakeRequest);

            mediatorDecorator.ReceivedRequest.Should().Be(fakeRequest);
        }

        [Test]
        public void Test_type_handler_service_registration_extension_method()
        {
            var services = new ServiceCollection();
            services.AddControllers().AddMediatRUsingRequestAbortedCancellationToken(typeof(FakeRequest));
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            mediator.Should().BeOfType<RequestAbortedCancellationTokenMediatorDecorator>();
        }

        [Test]
        public void Test_type_handler_service_registration_extension_method_with_configuration()
        {
            var services = new ServiceCollection();
            services.AddControllers().AddMediatRUsingRequestAbortedCancellationToken(config => { }, typeof(FakeRequest));
            var provider = services.BuildServiceProvider();

            var mediator = provider.GetService<IMediator>();

            mediator.Should().BeOfType<RequestAbortedCancellationTokenMediatorDecorator>();
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    [ExcludeFromCodeCoverage]
    public class MyFakeMediator : IMediator
    {
        public object ReceivedRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            ReceivedRequest = request;
            return Task.FromResult((TResponse)new object());
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
        {
            throw new System.NotImplementedException();
        }
    }

    [ExcludeFromCodeCoverage]
    public class FakeRequest : IRequest
    {
    }
}
