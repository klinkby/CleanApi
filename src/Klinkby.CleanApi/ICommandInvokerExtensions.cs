using System.Diagnostics;

namespace Klinkby.CleanApi;

/// <summary>
/// Helper extensions for invoking commands and mapping to HTTP response.
/// </summary>
public static class ICommandInvokerExtensions
{
    /// <summary>
    /// Handle a GET query using the command, then map the result to to HTTP 200 OK service response.
    /// </summary>
    /// <remarks>GET operations are always <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> and
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see> and 
    /// sometimes <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cachable</see>.</remarks>
    /// <typeparam name="TCommand">Command to invoke</typeparam>
    /// <typeparam name="TResponse">Service response</typeparam>
    /// <param name="commandInvoker">Command invoker</param>
    /// <param name="mapResponse">Convert the invoked command's response to service response</param>
    /// <param name="cancellation">For cancellling the command invocation</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> with the service response</returns>
    internal static async ValueTask<TResponse> GetAsync<TCommand, TResponse>(
        this ICommandInvoker<TCommand> commandInvoker,
        Func<TCommand, TResponse> mapResponse,
        CancellationToken cancellation)
        where TCommand : ICommand
    {
        await commandInvoker.ExecuteAsync(cancellation);
        // implicit status 200 OK
        return mapResponse(commandInvoker.Command);
    }

    /// <summary>
    /// Handle a POST request using the command and provide the location of the created resource in location header of a HTTP 201 Created response.
    /// </summary>
    /// <remarks>POST operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see>,
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see> or 
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cachable</see>. If the resource require idempotency, use 
    /// <see cref="PostAsync{TCommand}(ICommandInvoker{TCommand}, Func{TCommand, Uri}, CancellationToken)"/> instead for upserting.</remarks>
    /// <typeparam name="TCommand">Command to invoke</typeparam>
    /// <param name="commandInvoker">Command invoker</param>
    /// <param name="cancellation">For cancellling the command invocation</param>
    /// <returns>Empty <see cref="ValueTask"/></returns>
    internal static async ValueTask PostAsync<TCommand>(
        this ICommandInvoker<TCommand> commandInvoker,
        Func<TCommand, Uri> mapCreatedResourceLocation,
        CancellationToken cancellation)
        where TCommand : ICommand
    {
        HttpResponse response = await ExecuteCoreAsync(commandInvoker, cancellation);
        response.GetTypedHeaders().Location = mapCreatedResourceLocation(commandInvoker.Command);
        response.StatusCode = StatusCodes.Status201Created;
    }

    /// <summary>
    /// Perform a PUT request using the command. If <paramref name="mapCreatedResourceLocation"/> is provided and returns a location 
    /// (to the created resource) it is used for location header of a HTTP 201 Created response. Otherwise assume an existing resource
    /// was updated, responding HTTP 204 No Content.
    /// </summary>
    /// <remarks>PUT operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> or 
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cachable</see>, but can be 
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see>. 
    /// <typeparam name="TCommand">Command to invoke</typeparam>
    /// <param name="commandInvoker">Command invoker</param>
    /// <param name="cancellation">For cancellling the command invocation</param>
    /// <returns>Empty <see cref="ValueTask"/></returns>
    internal static async ValueTask PutAsync<TCommand>(
       this ICommandInvoker<TCommand> commandInvoker,
       Func<TCommand, Uri?>? mapCreatedResourceLocation,
       CancellationToken cancellation)
       where TCommand : ICommand
    {
        HttpResponse response = await ExecuteCoreAsync(commandInvoker, cancellation);
        var newResourceLocation = mapCreatedResourceLocation is not null
            ? mapCreatedResourceLocation(commandInvoker.Command)
            : null;
        if (newResourceLocation is null)
        {
            response.StatusCode = StatusCodes.Status204NoContent;
        }
        else
        {
            response.StatusCode = StatusCodes.Status201Created;
            response.GetTypedHeaders().Location = newResourceLocation;
        }
    }

    /// <summary>
    /// Perform a DELETE request using the command, responding HTTP 204 No Content. 
    /// </summary>
    /// <remarks>DELETE operations are never <see href="https://developer.mozilla.org/en-US/docs/Glossary/Safe/HTTP">Safe</see> or 
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Cacheable">Cachable</see>, but can be 
    /// <see href="https://developer.mozilla.org/en-US/docs/Glossary/Idempotent">Idempotent</see>. 
    /// <typeparam name="TCommand">Command to invoke</typeparam>
    /// <param name="commandInvoker">Command invoker</param>
    /// <param name="cancellation">For cancellling the command invocation</param>
    /// <returns>Empty <see cref="ValueTask"/></returns>
    internal static async ValueTask DeleteAsync<TCommand>(
       this ICommandInvoker<TCommand> commandInvoker,
       CancellationToken cancellation)
       where TCommand : ICommand
    {
        HttpResponse response = await ExecuteCoreAsync(commandInvoker, cancellation);
        response.StatusCode = StatusCodes.Status204NoContent;
    }

    /// <summary>
    /// Executes a command using invoker then returns a mapped the result.
    /// </summary>
    /// <typeparam name="TCommand">Command to invoke</typeparam>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="invoker"Command invoker></param>
    /// <param name="map">Maps executed command to return value</param>
    /// <param name="cancellation">For cancellling the command invocation</param>
    /// <returns>Mapped result</returns>
    public static async ValueTask<TResult> ExecuteAndMapAsync<TCommand, TResult>(
        this ICommandInvoker<TCommand> invoker,
        Func<TCommand, TResult> map,
        CancellationToken cancellation)
        where TCommand : ICommand
    {
        await invoker.ExecuteAsync(cancellation);
        return map(invoker.Command);
    }

    private static async Task<HttpResponse> ExecuteCoreAsync<TCommand>(
        ICommandInvoker<TCommand> commandInvoker,
        CancellationToken cancellation) where TCommand : ICommand
    {
        await commandInvoker.ExecuteAsync(cancellation);
        var response = commandInvoker.HttpContextAccessor.HttpContext!.Response;
        Debug.Assert(!response.HasStarted);
        return response;
    }
}
