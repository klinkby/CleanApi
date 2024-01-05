namespace Klinkby.CleanApi;

internal class CorrelationDelegatingHandler : DelegatingHandler
{
    private readonly Correlation _correlation;

    public CorrelationDelegatingHandler(Correlation correlation)
    {
        _correlation = correlation;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _correlation.TryAddCorrelationHeader(request.Headers);
        return base.SendAsync(request, cancellationToken);
    }
}