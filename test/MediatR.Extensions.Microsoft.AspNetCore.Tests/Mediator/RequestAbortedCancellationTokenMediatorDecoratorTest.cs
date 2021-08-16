namespace MediatR.Extensions.Microsoft.AspNetCore.Tests.Mediator
{
    using FluentAssertions;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Http;
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
            _requestAbortedTokenSource = new CancellationTokenSource();
            _httpContextStub = Substitute.For<HttpContext>();
            _httpContextStub.RequestAborted = _requestAbortedTokenSource.Token;
            _httpContextAccessor.HttpContext.Returns(_httpContextStub);
            _mediator = Substitute.For<IMediator>();
            _subject = new RequestAbortedCancellationTokenMediatorDecorator(_mediator, _httpContextAccessor);
        }

        private IHttpContextAccessor _httpContextAccessor;
        private RequestAbortedCancellationTokenMediatorDecorator _subject;
        private HttpContext _httpContextStub;
        private IMediator _mediator;
        private CancellationTokenSource _requestAbortedTokenSource;

        [Test]
        public async Task Strongly_typed_Send_uses_requested_aborted_cancellation_token_if_it_is_available_and_no_explicit_token_was_passed()
        {
            var fakeRequest = Substitute.For<IRequest<object>>();

            await _subject.Send(fakeRequest);

            await _mediator.Received(1).Send(fakeRequest, _httpContextStub.RequestAborted);
        }

        [Test]
        [TestCase("cancelByRequestAbortedToken")]
        [TestCase("cancelByPassedToken")]
        public async Task Strongly_typed_Send_merges_passed_cancellation_token_with_request_aborted_token_if_both_are_set(string cancellationSource)
        {
            var fakeRequest = Substitute.For<IRequest<object>>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received(1).Send(fakeRequest, Arg.Do<CancellationToken>(t =>
            {
                t.IsCancellationRequested.Should().BeFalse();
                if (cancellationSource == "cancelByRequestAbortedToken")
                    _requestAbortedTokenSource.Cancel();
                else if (cancellationSource == "cancelByPassedToken")
                    cancellationTokenSource.Cancel();
                t.IsCancellationRequested.Should().BeTrue();
            }));
        }

        [Test]
        public async Task Strongly_typed_Send_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeRequest = Substitute.For<IRequest<object>>();
            var cancellationToken = new CancellationTokenSource().Token;
            _httpContextAccessor.HttpContext.Returns((HttpContext) null);

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received(1).Send(fakeRequest, cancellationToken);
        }

        [Test]
        public async Task Object_typed_Send_uses_requested_aborted_cancellation_token_if_it_is_available_and_no_explicit_token_was_passed()
        {
            var fakeRequest = new object();

            await _subject.Send(fakeRequest);

            await _mediator.Received(1).Send(fakeRequest, _httpContextStub.RequestAborted);
        }

        [Test]
        [TestCase("cancelByRequestAbortedToken")]
        [TestCase("cancelByPassedToken")]
        public async Task Object_typed_Send_merges_passed_cancellation_token_with_request_aborted_token_if_both_are_set(string cancellationSource)
        {
            var fakeRequest = new object();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received(1).Send(fakeRequest, Arg.Do<CancellationToken>(t =>
            {
                t.IsCancellationRequested.Should().BeFalse();
                if (cancellationSource == "cancelByRequestAbortedToken")
                    _requestAbortedTokenSource.Cancel();
                else if (cancellationSource == "cancelByPassedToken")
                    cancellationTokenSource.Cancel();
                t.IsCancellationRequested.Should().BeTrue();
            }));
        }

        [Test]
        public async Task Object_typed_Send_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeRequest = new object();
            var cancellationToken = new CancellationTokenSource().Token;
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);

            await _subject.Send(fakeRequest, cancellationToken);

            await _mediator.Received(1).Send(fakeRequest, cancellationToken);
        }

        [Test]
        public async Task Strongly_typed_Publish_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeNotification = Substitute.For<INotification>();

            await _subject.Publish(fakeNotification);

            await _mediator.Received(1).Publish(fakeNotification, _httpContextStub.RequestAborted);
        }

        [Test]
        [TestCase("cancelByRequestAbortedToken")]
        [TestCase("cancelByPassedToken")]
        public async Task Strongly_typed_Publish_merges_passed_cancellation_token_with_request_aborted_token_if_both_are_set(string cancellationSource)
        {
            var fakeNotification = Substitute.For<INotification>();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received(1).Publish(fakeNotification, Arg.Do<CancellationToken>(t =>
            {
                t.IsCancellationRequested.Should().BeFalse();
                if (cancellationSource == "cancelByRequestAbortedToken")
                    _requestAbortedTokenSource.Cancel();
                else if (cancellationSource == "cancelByPassedToken")
                    cancellationTokenSource.Cancel();
                t.IsCancellationRequested.Should().BeTrue();
            }));
        }

        [Test]
        public async Task Strongly_typed_Publish_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeNotification = Substitute.For<INotification>();
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);
            var cancellationToken = new CancellationTokenSource().Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received(1).Publish(fakeNotification, cancellationToken);
        }

        [Test]
        public async Task Object_typed_Publish_uses_requested_aborted_cancellation_token_if_available()
        {
            var fakeNotification = new object();

            await _subject.Publish(fakeNotification);

            await _mediator.Received(1).Publish(fakeNotification, _httpContextStub.RequestAborted);
        }

        [Test]
        [TestCase("cancelByRequestAbortedToken")]
        [TestCase("cancelByPassedToken")]
        public async Task Object_typed_Publish_merges_passed_cancellation_token_with_request_aborted_token_if_both_are_set(string cancellationSource)
        {
            var fakeNotification = new object();
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received(1).Publish(fakeNotification, Arg.Do<CancellationToken>(t =>
            {
                t.IsCancellationRequested.Should().BeFalse();
                if (cancellationSource == "cancelByRequestAbortedToken")
                    _requestAbortedTokenSource.Cancel();
                else if (cancellationSource == "cancelByPassedToken")
                    cancellationTokenSource.Cancel();
                t.IsCancellationRequested.Should().BeTrue();
            }));
        }

        [Test]
        public async Task Object_typed_Publish_falls_back_to_passed_cancellation_token_if_no_http_context_or_request_aborted_token_is_available()
        {
            var fakeNotification = new object();
            _httpContextAccessor.HttpContext.Returns((HttpContext)null);
            var cancellationToken = new CancellationTokenSource().Token;

            await _subject.Publish(fakeNotification, cancellationToken);

            await _mediator.Received(1).Publish(fakeNotification, cancellationToken);
        }
    }
}