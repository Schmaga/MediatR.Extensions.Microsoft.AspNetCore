using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;

namespace MediatR.Extensions.Microsoft.AspNetCore.Mediator;
/// <summary>
/// Mediator Decorator implementation that wraps around the regular registered mediator and passes the HttpContext.RequestAborted CancellationToken to the request handlers, if it is available.
/// If an explicit token is passed to the Send or Publish methods, it will create a combined token source using both HttpContext.RequestAborted and the passed token
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
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return await PossiblyWrapSendCallWithLinkedCancellationToken(
            async token => await _mediator.Send(request, token).ConfigureAwait(false),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return await PossiblyWrapSendCallWithLinkedCancellationToken(
            async token => await _mediator.Send(request, token).ConfigureAwait(false),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return PossiblyWrapCreateStreamCallWithNewCancellationToken(
            token => _mediator.CreateStream(request, token),
            cancellationToken);
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return PossiblyWrapCreateStreamCallWithNewCancellationToken(
            token => _mediator.CreateStream(request, token),
            cancellationToken);
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        await PossiblyWrapPublishCallWithNewCancellationToken(
                async token => await _mediator.Publish(notification, token).ConfigureAwait(false),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        await PossiblyWrapPublishCallWithNewCancellationToken(
                async token => await _mediator.Publish(notification, token).ConfigureAwait(false),
                cancellationToken)
            .ConfigureAwait(false);
    }

#pragma warning restore 1591

    private async Task<TResponse> PossiblyWrapSendCallWithLinkedCancellationToken<TResponse>(Func<CancellationToken, Task<TResponse>> wrappedSend, CancellationToken cancellationToken)
    {
        if (cancellationToken != default && _httpContextAccessor.HttpContext != null)
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _httpContextAccessor.HttpContext.RequestAborted);
            var token = cancellationTokenSource.Token;
            return await wrappedSend(token).ConfigureAwait(false);
        }
        else if (cancellationToken == default && _httpContextAccessor.HttpContext != null)
            return await wrappedSend(_httpContextAccessor.HttpContext.RequestAborted).ConfigureAwait(false);
        else
            return await wrappedSend(cancellationToken).ConfigureAwait(false);
    }

    private async IAsyncEnumerable<TResponse> PossiblyWrapCreateStreamCallWithNewCancellationToken<TResponse>(
        Func<CancellationToken, IAsyncEnumerable<TResponse>> wrappedCreateStream, 
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (cancellationToken != default && _httpContextAccessor.HttpContext != null)
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _httpContextAccessor.HttpContext.RequestAborted);
            var token = cancellationTokenSource.Token;
            await foreach (var value in wrappedCreateStream(token).ConfigureAwait(false))
            {
                yield return value;
            }
        }
        else if (cancellationToken == default && _httpContextAccessor.HttpContext != null)
        {
            await foreach (var value in wrappedCreateStream(_httpContextAccessor.HttpContext.RequestAborted).ConfigureAwait(false))
            {
                yield return value;
            }
        }
        else
        {
            await foreach (var value in wrappedCreateStream(cancellationToken).ConfigureAwait(false))
            {
                yield return value;
            }
        }
    }

    private async Task PossiblyWrapPublishCallWithNewCancellationToken(Func<CancellationToken, Task> wrappedPublish, CancellationToken cancellationToken)
    {
        if (cancellationToken != default && _httpContextAccessor.HttpContext != null)
        {
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _httpContextAccessor.HttpContext.RequestAborted);
            var token = cancellationTokenSource.Token;
            await wrappedPublish(token).ConfigureAwait(false);
        }
        else if (cancellationToken == default && _httpContextAccessor.HttpContext != null)
            await wrappedPublish(_httpContextAccessor.HttpContext.RequestAborted).ConfigureAwait(false);
        else
            await wrappedPublish(cancellationToken).ConfigureAwait(false);
    }
}
