using Microsoft.Extensions.Logging;

namespace Klinkby.CleanApi;

internal class CommandInvokerFactory<T> : ICommandInvoker<T> where T : ICommand
{
    private readonly ICommandInvoker<T> commandInvoker;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CommandInvokerFactory(T command, IHttpContextAccessor httpContextAccessor, ILogger<CommandInvoker<T>> logger)
    {
        commandInvoker = new CommandInvoker<T>(command, httpContextAccessor, logger);
        this.httpContextAccessor = httpContextAccessor;
    }

    public T Command => commandInvoker.Command;

    public IHttpContextAccessor HttpContextAccessor => httpContextAccessor;

    public ValueTask ExecuteAsync(CancellationToken cancellation)
    {
        return commandInvoker.ExecuteAsync(cancellation);
    }
}