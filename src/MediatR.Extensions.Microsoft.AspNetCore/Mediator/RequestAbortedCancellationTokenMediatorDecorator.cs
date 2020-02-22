namespace MediatR.Extensions.Microsoft.AspNetCore.Mediator
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Microsoft.AspNetCore.Http;

    /// <summary>
    /// Mediator Decorator implementation that wraps around the regular registered mediator and passes the HttpContext.RequestAborted CancellationToken to the request handlers, if it is available
    /// </summary>
    public class RequestAbortedCancellationTokenMediatorDecorator : IMediator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        /// <summary>
        /// Initializes a new decorator instance
        /// </summary>
        /// <param name="mediator">The mediator instance that gets decorated</param>
        /// <param name="httpContextAccessor">The http context accessor of the corresponding environment</param>
        public RequestAbortedCancellationTokenMediatorDecorator(
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

#pragma warning disable 1591 // Disable xml comment warnings because documentation should be based on IMediator interface summaries
        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            var cancellationTokenToUse = GetRequestAbortedCancellationTokenOrUseTokenPassedFromInitialCall(cancellationToken);
            return await _mediator.Send(request, cancellationTokenToUse);
        }

        public async Task<object> Send(object request, CancellationToken cancellationToken = new CancellationToken())
        {
            var cancellationTokenToUse = GetRequestAbortedCancellationTokenOrUseTokenPassedFromInitialCall(cancellationToken);
            return await _mediator.Send(request, cancellationTokenToUse);
        }

        public async Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
        {
            var cancellationTokenToUse = GetRequestAbortedCancellationTokenOrUseTokenPassedFromInitialCall(cancellationToken);
            await _mediator.Publish(notification, cancellationTokenToUse);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
        {
            var cancellationTokenToUse = GetRequestAbortedCancellationTokenOrUseTokenPassedFromInitialCall(cancellationToken);
            await _mediator.Publish(notification, cancellationTokenToUse);
        }
#pragma warning restore 1591

        private CancellationToken GetRequestAbortedCancellationTokenOrUseTokenPassedFromInitialCall(CancellationToken cancellationToken)
        {
            return _httpContextAccessor?.HttpContext?.RequestAborted ?? cancellationToken;
        }
    }
}
