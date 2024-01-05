using Klinkby.CleanApi;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Extension for <see cref="IHttpClientBuilder" /> to support correlation id.
/// </summary>
public static class CleanApiHttpClientBuilderExtensions
{
    /// <summary>
    ///     Sets the x-correlation-id header on outgoing requests.
    ///     If in scope of a HTTP request it will use the incoming x-correlation-id header.
    ///     Add to your service collection like this:
    ///     <code>
    /// services.AddHttpClient&lt;GetProductsHandler&gt;()
    ///         .AddCorrelation();
    /// </code>
    /// </summary>
    /// <param name="builder">The HttpClient to support correlation id</param>
    /// <returns>builder</returns>
    public static IHttpClientBuilder AddCorrelation(this IHttpClientBuilder builder)
    {
        return builder!.AddHttpMessageHandler<CorrelationDelegatingHandler>();
    }
}