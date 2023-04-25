namespace Klinkby.CleanApi;

public interface ICommandInvoker<out T> where T : ICommand
{
    T Command { get; }
    IHttpContextAccessor HttpContextAccessor { get; }
    ValueTask ExecuteAsync(CancellationToken cancellation);
}