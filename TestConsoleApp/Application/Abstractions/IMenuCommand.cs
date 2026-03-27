namespace TestConsoleApp.Application.Abstractions;

public interface IMenuCommand
{
    string Title { get; }
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
