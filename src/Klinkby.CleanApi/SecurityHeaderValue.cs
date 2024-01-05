namespace Klinkby.CleanApi;

/// <summary>
///     Response headers to harden the service as recommended by https://observatory.mozilla.org/faq/
/// </summary>
internal static class SecurityHeaderValue
{
    public const string ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
    public const string XContentTypeOptions = "nosniff";
    public const int HstsMaxAge = 63072000;
}