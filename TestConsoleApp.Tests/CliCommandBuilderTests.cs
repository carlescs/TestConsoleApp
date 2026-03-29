using NSubstitute;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Cli;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests;

public sealed class CliCommandBuilderTests
{
    [Fact]
    public void Configure_WithEmptyList_DoesNotThrow()
    {
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_WithSingleLeafCommand_DoesNotThrow()
    {
        var command = Substitute.For<IMenuCommand>();
        command.Title.Returns("Say Hello");
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [command])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_WithMultipleLeafCommands_DoesNotThrow()
    {
        var cmd1 = Substitute.For<IMenuCommand>();
        var cmd2 = Substitute.For<IMenuCommand>();
        cmd1.Title.Returns("Say Hello");
        cmd2.Title.Returns("Generate GUID");
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [cmd1, cmd2])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_WithSubMenuCommand_DoesNotThrow()
    {
        var child = Substitute.For<IMenuCommand>();
        child.Title.Returns("Child Command");
        var subMenu = new SubMenuCommand("My Tools", [child]);
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [subMenu])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_WithNestedSubMenus_DoesNotThrow()
    {
        var leaf = Substitute.For<IMenuCommand>();
        leaf.Title.Returns("Leaf");
        var inner = new SubMenuCommand("Inner Menu", [leaf]);
        var outer = new SubMenuCommand("Outer Menu", [inner]);
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [outer])));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData("Say Hello", "say-hello")]
    [InlineData("Generate GUID", "generate-guid")]
    [InlineData("Show Date & Time", "show-date-time")]
    [InlineData("Tools", "tools")]
    [InlineData("  Leading Spaces  ", "leading-spaces")]
    public void Configure_LeafCommand_CanBeInvokedByKebabCasedName(string title, string kebabName)
    {
        var executed = false;
        var command = Substitute.For<IMenuCommand>();
        command.Title.Returns(title);
        command.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { executed = true; return Task.CompletedTask; });
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [command]));

        app.Run([kebabName]);

        Assert.True(executed);
    }

    [Fact]
    public void Configure_BranchLeafCommand_CanBeInvoked()
    {
        var executed = false;
        var leaf = Substitute.For<IMenuCommand>();
        leaf.Title.Returns("Leaf Command");
        leaf.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { executed = true; return Task.CompletedTask; });
        var subMenu = new SubMenuCommand("My Tools", [leaf]);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [subMenu]));

        app.Run(["my-tools", "leaf-command"]);

        Assert.True(executed);
    }
}
