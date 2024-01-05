using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApplication1;

internal static class ResultMappers
{
    // OK results
    public static Task<Results<Ok<TResult>, ProblemHttpResult>> ToOkResult<TValue, TResult>(this Task<TValue> task,
        Func<TValue, TResult> mapper)
        where TValue : notnull
    {
        return task.ContinueWith(t => (Results<Ok<TResult>, ProblemHttpResult>)TypedResults.Ok(mapper(t.Result)));
    }

    public static Task<Results<Ok<TResult>, ValidationProblem, ProblemHttpResult>> ToValidatedOkResult<TValue, TResult>(
        this Task<TValue> task, Func<TValue, TResult> mapper)
        where TValue : notnull
    {
        return task.ContinueWith(t =>
            (Results<Ok<TResult>, ValidationProblem, ProblemHttpResult>)TypedResults.Ok(mapper(t.Result)));
    }

    public static Task<Created> ToCreatedResult(this Task<Uri> task)
    {
        return task.ContinueWith(t => TypedResults.Created(t.Result));
    }

    public static Task<NoContent> ToResult(this Task task)
    {
        return task.ContinueWith(t => TypedResults.NoContent());
    }
}