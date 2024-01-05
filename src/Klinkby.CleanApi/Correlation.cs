using System.Net.Http.Headers;

namespace Klinkby.CleanApi;

internal class Correlation(IHttpContextAccessor httpContextAccessor)
{
    private const string HeaderName = "X-Correlation-Id";
    private const string ItemKey = HeaderName;

    public string Id
    {
        get => GetCorrelationId(httpContextAccessor.HttpContext!) ?? (Id = NewCorrelationId());
        private set => SetCorrelationId(httpContextAccessor.HttpContext!, value);
    }

    public bool TryAddCorrelationHeader(IHeaderDictionary headers)
    {
        return headers.TryAdd(HeaderName, Id);
    }

    public bool TryAddCorrelationHeader(HttpRequestHeaders headers)
    {
        return headers.TryAddWithoutValidation(HeaderName, Id);
    }

    private static string? GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(ItemKey, out var correlationIdFromItems)
            && correlationIdFromItems is not null)
            return (string)correlationIdFromItems;
        if (context.Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            var correlationIdFromRequest = headerValues.ToString();
            if (!string.IsNullOrEmpty(correlationIdFromRequest)) return correlationIdFromRequest;
        }

        return default;
    }

    private static void SetCorrelationId(HttpContext context, string correlationId)
    {
        context.Items[ItemKey] = correlationId;
    }

    private static string NewCorrelationId()
    {
        return Guid.NewGuid().ToString();
    }
}