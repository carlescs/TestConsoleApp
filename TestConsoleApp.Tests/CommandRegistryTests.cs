using System.Reflection;
using NSubstitute;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Tests;

public sealed class CommandRegistryTests : IDisposable
{
    private readonly List<IMenuCommand> _commandsField;
    private readonly List<IMenuCommand> _backup;

    public CommandRegistryTests()
    {
        _commandsField = (List<IMenuCommand>)typeof(CommandRegistry)
            .GetField("_commands", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        _backup = [.._commandsField];
        _commandsField.Clear();
    }

    public void Dispose()
    {
        _commandsField.Clear();
        _commandsField.AddRange(_backup);
    }

    [Fact]
    public void Commands_IsEmpty_WhenNothingRegistered()
    {
        Assert.Empty(CommandRegistry.Commands);
    }

    [Fact]
    public void Register_AddsCommandToCommands()
    {
        var command = Substitute.For<IMenuCommand>();

        CommandRegistry.Register(command);

        Assert.Contains(command, CommandRegistry.Commands);
    }

    [Fact]
    public void Register_MultipleCommands_AllAppearInCollection()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();

        CommandRegistry.Register(cmd1);
        CommandRegistry.Register(cmd2);

        Assert.Equal(2, CommandRegistry.Commands.Count);
        Assert.Contains(cmd1, CommandRegistry.Commands);
        Assert.Contains(cmd2, CommandRegistry.Commands);
    }

    [Fact]
    public void Register_PreservesInsertionOrder()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        var cmd3 = Substitute.For<IMenuCommand>();

        CommandRegistry.Register(cmd1);
        CommandRegistry.Register(cmd2);
        CommandRegistry.Register(cmd3);

        Assert.Equal(cmd1, CommandRegistry.Commands[0]);
        Assert.Equal(cmd2, CommandRegistry.Commands[1]);
        Assert.Equal(cmd3, CommandRegistry.Commands[2]);
    }

    [Fact]
    public void Commands_IsReadOnly()
    {
        Assert.IsAssignableFrom<IReadOnlyList<IMenuCommand>>(CommandRegistry.Commands);
    }
}
