namespace Klinkby.CleanApi;

/// <summary>
///     Helper extensions for invoking commands and mapping to HTTP response.
/// </summary>
public static class CommandInvokerExtensions
{
    /// <summary>
    ///     Handle a GET query using the command, then map the result to to HTTP 200 OK service response.
    /// </summary>
    /// <remarks>
    ///     GET operations are always <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> and
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see> and
    ///     sometimes <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cacheable</see>.
    /// </remarks>
    /// <typeparam name="TResponse">Service response</typeparam>
    /// <param name="response">For cancelling the command invocation</param>
    /// <returns>A 200 OK result</returns>
    public static IResult ToGetResult<TResponse>(TResponse response)
        where TResponse : notnull
    {
        return Results.Ok(response);
    }

    /// <summary>
    ///     Handle a POST request using the command and provide the location of the created resource in
    ///     location header of a HTTP 201 Created response.
    /// </summary>
    /// <remarks>
    ///     POST operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see>,
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see> or
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cacheable</see>. If the resource require
    ///     idempotency, use PUT request instead for upsert.
    /// </remarks>
    /// <param name="newResourceLocation">Location of the created resource</param>
    /// <returns>A 201 Created result</returns>
    public static IResult ToPostResult(Uri newResourceLocation)
    {
        return Results.Created(newResourceLocation, null);
    }

    /// <summary>
    ///     Perform a PUT request using the command. If <paramref name="newResourceLocation" /> is provided and returns a
    ///     location (to the created resource) it is used for location header of a HTTP 201 Created response. Otherwise
    ///     assume an existing resource was updated, responding HTTP 204 No Content.
    /// </summary>
    /// <remarks>
    ///     PUT operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> or
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cacheable</see>, but can be
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see>.
    /// </remarks>
    /// <param name="newResourceLocation">Location of the created resource</param>
    /// <returns>A 201 Created or a 204 No Content result<see cref="ValueTask" /></returns>
    internal static IResult ToPutResult(Uri? newResourceLocation = default)
    {
        return newResourceLocation is null
            ? Results.NoContent()
            : Results.Created(newResourceLocation, null);
    }

    /// <summary>
    ///     Perform a DELETE request using the command, responding HTTP 204 No Content.
    /// </summary>
    /// <remarks>
    ///     DELETE operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> or
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cacheable</see>, but can be
    ///     <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see>.
    /// </remarks>
    /// <returns>A 204 No Content result</returns>
    internal static IResult ToDeleteResult()
    {
        return Results.NoContent();
    }
}