using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Klinkby.CleanApi.Serialization;

/// <summary>
///     Extension methods for <see cref="JsonSerializerContext" />
/// </summary>
internal static class JsonSerializerContextExtensions
{
    /// <summary>
    ///     Use source generation for high performance serializing to HTTP Response. AOT friendly.
    /// </summary>
    public static Task WriteJsonAsync<T, TContext>(
        this TContext context,
        T value,
        HttpResponse response,
        CancellationToken cancellationToken = default)
        where T : class
        where TContext : JsonSerializerContext
    {
        response.ContentType = MediaTypeNames.Application.Json;
        response.StatusCode = StatusCodes.Status200OK;
        return WriteJsonCoreAsync(context, value, response, cancellationToken);
    }

    public static Task WriteProblemDetailsJsonAsync<T, TContext>(
        this TContext context,
        T value,
        HttpResponse response,
        CancellationToken cancellationToken = default)
        where T : ProblemDetails
        where TContext : JsonSerializerContext
    {
        response.ContentType = MediaTypeNames.Application.ProblemJson;
        if (value.Status is not null) response.StatusCode = value.Status.Value;
        return WriteJsonCoreAsync(context, value, response, cancellationToken);
    }

    private static Task WriteJsonCoreAsync<T, TContext>(
        this TContext context,
        T value,
        HttpResponse response,
        CancellationToken cancellationToken = default)
        where T : class
        where TContext : JsonSerializerContext
    {
        var jsonString = JsonSerializer.Serialize(
            value,
            typeof(T),
            context);
        return response.WriteAsync(jsonString, cancellationToken);
    }
}