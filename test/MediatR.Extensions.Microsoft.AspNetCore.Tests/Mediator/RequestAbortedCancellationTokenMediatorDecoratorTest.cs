namespace MediatR.Extensions.Microsoft.AspNetCore.Tests.Mediator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Http;
    using global::Microsoft.AspNetCore.Http.Features;
    using MediatR.Extensions.Microsoft.AspNetCore.Mediator;
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    public class RequestAbortedCancellationTokenMediatorDecoratorTest
    {
        [SetUp]
        public void SetUp()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _httpContextStub = new HttpContextStub {RequestAborted = new CancellationToken()};
            _httpContextAccessor.HttpContext.Returns(_httpContextStub);
            _mediator = Substitute.For<IMediator>();
            _subject = new RequestAbortedCancellationTokenMediatorDecorator(
                _mediator,
                _httpContextAccessor);
        }

        private IHttpContextAccessor _httpContextAccessor;
        private RequestAbortedCancellationTokenMediatorDecorator _subject;
        private HttpContextStub _httpContextStub;
        private IMediator _mediator;

        [Test]
        public async Task Strongly_typed_Send_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeRequest = Substitute.For<IRequest<object>>();

            await _subject.Send(fakeRequest);

            await _mediator.Received().Send(fakeRequest, _httpContextStub.RequestAborted);
        }

        [Test]
        public async Task Strongly_typed_Send_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeRequest = Substitute.For<IRequest<object>>();
            var cancellationToken = new CancellationTokenSource().Token;
            _httpContextAccessor.HttpContext.Returns((HttpContext) null);

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received().Send(fakeRequest, cancellationToken);
        }

        [Test]
        public async Task Object_typed_Send_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeRequest = new object();

            await _subject.Send(fakeRequest);

            await _mediator.Received().Send(fakeRequest, _httpContextStub.RequestAborted);
        }

        [Test]
        public async Task Object_typed_Send_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeRequest = new object();
            var cancellationToken = new CancellationTokenSource().Token;
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received().Send(fakeRequest, cancellationToken);
        }

        [Test]
        public async Task Strongly_typed_Publish_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeNotification = Substitute.For<INotification>();

            await _subject.Publish(fakeNotification);

            await _mediator.Received().Publish(fakeNotification, _httpContextStub.RequestAborted);
        }

        [Test]
        public async Task Strongly_typed_Publish_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeNotification = Substitute.For<INotification>();
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);
            var cancellationToken = new CancellationTokenSource().Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received().Publish(fakeNotification, cancellationToken);
        }

        [Test]
        public async Task Object_typed_Publish_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeNotification = new object();

            await _subject.Publish(fakeNotification);

            await _mediator.Received().Publish(fakeNotification, _httpContextStub.RequestAborted);
        }

        [Test]
        public async Task Object_typed_Publish_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeNotification = new object();
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);
            var cancellationToken = new CancellationTokenSource().Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received().Publish(fakeNotification, cancellationToken);
        }

        [ExcludeFromCodeCoverage]
        private class HttpContextStub : HttpContext
        {
            public override ClaimsPrincipal User { get; set; }
            public override IDictionary<object, object> Items { get; set; }
            public override IServiceProvider RequestServices { get; set; }
            public override CancellationToken RequestAborted { get; set; }
            public override string TraceIdentifier { get; set; }
            public override ISession Session { get; set; }

            public override void Abort()
            {
                throw new NotImplementedException();
            }

            // ReSharper disable UnassignedGetOnlyAutoProperty
            public override IFeatureCollection Features { get; }
            public override HttpRequest Request { get; }
            public override HttpResponse Response { get; }
            public override ConnectionInfo Connection { get; }
            public override WebSocketManager WebSockets { get; }
            // ReSharper restore UnassignedGetOnlyAutoProperty
        }
    }
}