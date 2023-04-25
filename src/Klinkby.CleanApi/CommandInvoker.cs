using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Klinkby.CleanApi;

internal class CommandInvoker<T> : ICommandInvoker<T> where T : ICommand
{
    private readonly T command;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<CommandInvoker<T>> logger;

    public CommandInvoker(T command, IHttpContextAccessor httpContextAccessor, ILogger<CommandInvoker<T>> logger)
    {
        this.command = command;
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    public T Command => command;

    public IHttpContextAccessor HttpContextAccessor => httpContextAccessor;

    public async ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        var watch = Stopwatch.StartNew();
        using var scope = logger.BeginScope("Execute {command}", typeof(T).Name);
        try
        {
            await command.ExecuteAsync(cancellation).ConfigureAwait(false);
            logger.LogInformation("Command success in {executionTime} mS", watch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Command failed in {executionTime} mS", watch.ElapsedMilliseconds);
        }
    }
}
