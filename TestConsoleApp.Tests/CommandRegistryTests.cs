using System.Reflection;
using NSubstitute;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Tests;

public sealed class CommandRegistryTests : IDisposable
{
    private readonly List<IMenuCommand> _commandsField;
    private readonly List<IMenuCommand> _backup;
    private readonly Dictionary<Type, (char Key, ConsoleModifiers Modifiers)> _hotkeysField;
    private readonly Dictionary<(char Key, ConsoleModifiers Modifiers), IMenuCommand> _globalHotkeysField;

    public CommandRegistryTests()
    {
        _commandsField = (List<IMenuCommand>)typeof(CommandRegistry)
            .GetField("_commands", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        _backup = [.._commandsField];
        _commandsField.Clear();

        _hotkeysField = (Dictionary<Type, (char Key, ConsoleModifiers Modifiers)>)typeof(CommandRegistry)
            .GetField("_hotkeys", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        _hotkeysField.Clear();

        _globalHotkeysField = (Dictionary<(char Key, ConsoleModifiers Modifiers), IMenuCommand>)typeof(CommandRegistry)
            .GetField("_globalHotkeys", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        _globalHotkeysField.Clear();
    }

    public void Dispose()
    {
        _commandsField.Clear();
        _commandsField.AddRange(_backup);
        _hotkeysField.Clear();
        _globalHotkeysField.Clear();
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

    [Fact]
    public void GetHotkey_ReturnsNull_WhenNoHotkeyRegistered()
    {
        var command = Substitute.For<IMenuCommand>();

        var result = CommandRegistry.GetHotkey(command);

        Assert.Null(result);
    }

    [Fact]
    public void RegisterHotkey_StoresHotkey_RetrievableByGetHotkey()
    {
        var command = Substitute.For<IMenuCommand>();

        CommandRegistry.RegisterHotkey('H', (ConsoleModifiers)0, command);

        var result = CommandRegistry.GetHotkey(command);
        Assert.NotNull(result);
        Assert.Equal('H', result!.Value.Key);
        Assert.Equal((ConsoleModifiers)0, result.Value.Modifiers);
    }

    [Fact]
    public void RegisterHotkey_WithModifier_StoresModifier()
    {
        var command = Substitute.For<IMenuCommand>();

        CommandRegistry.RegisterHotkey('H', ConsoleModifiers.Control, command);

        var result = CommandRegistry.GetHotkey(command);
        Assert.NotNull(result);
        Assert.Equal(ConsoleModifiers.Control, result!.Value.Modifiers);
    }

    [Fact]
    public void FindGlobalCommand_ReturnsNull_WhenNothingRegistered()
    {
        var keyInfo = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);

        var result = CommandRegistry.FindGlobalCommand(keyInfo);

        Assert.Null(result);
    }

    [Fact]
    public void FindGlobalCommand_ReturnsCommand_OnExactMatch()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('H', (ConsoleModifiers)0, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));

        Assert.Same(command, result);
    }

    [Fact]
    public void FindGlobalCommand_IsCaseInsensitive()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('h', (ConsoleModifiers)0, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('H', ConsoleKey.H, true, false, false));

        Assert.Same(command, result);
    }

    [Fact]
    public void FindGlobalCommand_MatchesCtrlHotkey()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('H', ConsoleModifiers.Control, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('\x08', ConsoleKey.H, false, false, true));

        Assert.Same(command, result);
    }

    [Fact]
    public void FindGlobalCommand_ReturnsNull_WhenModifierDiffers()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('H', ConsoleModifiers.Control, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false));

        Assert.Null(result);
    }

    [Fact]
    public void FindGlobalCommand_MatchesAltHotkey()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('A', ConsoleModifiers.Alt, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('\0', ConsoleKey.A, false, true, false));

        Assert.Same(command, result);
    }

    [Fact]
    public void FindGlobalCommand_ReturnsNull_WhenAltRequired_ButNotPressed()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('A', ConsoleModifiers.Alt, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

        Assert.Null(result);
    }

    [Fact]
    public void FindGlobalCommand_MatchesAltHotkey_CaseInsensitive()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('a', ConsoleModifiers.Alt, command);

        var result = CommandRegistry.FindGlobalCommand(new ConsoleKeyInfo('\0', ConsoleKey.A, false, true, false));

        Assert.Same(command, result);
    }

    [Fact]
    public void RegisterHotkey_OverwritesPreviousHotkey_WhenRegisteredTwiceForSameInstance()
    {
        var command = Substitute.For<IMenuCommand>();
        CommandRegistry.RegisterHotkey('H', (ConsoleModifiers)0, command);
        CommandRegistry.RegisterHotkey('X', (ConsoleModifiers)0, command);

        var result = CommandRegistry.GetHotkey(command);

        Assert.NotNull(result);
        Assert.Equal('X', result!.Value.Key);
    }
}
