using NSubstitute;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Cli;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests.Presentation.Cli;

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
        bool executed = false;
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
        bool executed = false;
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

    [Fact]
    public void Configure_WithCliParameterisedRootCommand_DoesNotThrow()
    {
        var command = new TrackingCliCommand(new CommandState());
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [command])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_CliParameterisedRootCommand_CallsWithSettingsAndExecutes()
    {
        var state = new CommandState();
        var command = new TrackingCliCommand(state);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [command]));

        app.Run(["tracking-command"]);

        Assert.True(state.WithSettingsCalled);
        Assert.True(state.ExecuteCalled);
    }

    [Fact]
    public void Configure_CliParameterisedRootCommand_PassesTypedSettingsToWithSettings()
    {
        var state = new CommandState();
        var command = new TrackingCliCommand(state);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [command]));

        app.Run(["tracking-command"]);

        Assert.IsType<FakeSettings>(state.ReceivedSettings);
    }

    [Fact]
    public void Configure_WithCliParameterisedBranchCommand_DoesNotThrow()
    {
        var command = new TrackingCliCommand(new CommandState());
        var subMenu = new SubMenuCommand("My Tools", [command]);
        var app = new CommandApp();

        var exception = Record.Exception(() =>
            app.Configure(config => CliCommandBuilder.Configure(config, [subMenu])));

        Assert.Null(exception);
    }

    [Fact]
    public void Configure_CliParameterisedBranchCommand_CallsWithSettingsAndExecutes()
    {
        var state = new CommandState();
        var command = new TrackingCliCommand(state);
        var subMenu = new SubMenuCommand("My Tools", [command]);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [subMenu]));

        app.Run(["my-tools", "tracking-command"]);

        Assert.True(state.WithSettingsCalled);
        Assert.True(state.ExecuteCalled);
    }

    [Fact]
    public void Configure_CliParameterisedBranchCommand_PassesTypedSettingsToWithSettings()
    {
        var state = new CommandState();
        var command = new TrackingCliCommand(state);
        var subMenu = new SubMenuCommand("My Tools", [command]);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [subMenu]));

        app.Run(["my-tools", "tracking-command"]);

        Assert.IsType<FakeSettings>(state.ReceivedSettings);
    }

    [Fact]
    public void Configure_DeeplyNestedBranchCommand_CanBeInvoked()
    {
        bool executed = false;
        var leaf = Substitute.For<IMenuCommand>();
        leaf.Title.Returns("Deep Leaf");
        leaf.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { executed = true; return Task.CompletedTask; });
        var level2 = new SubMenuCommand("Level Two", [leaf]);
        var level1 = new SubMenuCommand("Level One", [level2]);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [level1]));

        app.Run(["level-one", "level-two", "deep-leaf"]);

        Assert.True(executed);
    }

    [Theory]
    [InlineData("command-alpha")]
    [InlineData("command-beta")]
    public void Configure_BranchWithMultipleLeafCommands_EachCanBeInvoked(string targetName)
    {
        bool executed = false;
        var cmdAlpha = Substitute.For<IMenuCommand>();
        cmdAlpha.Title.Returns("Command Alpha");
        var cmdBeta = Substitute.For<IMenuCommand>();
        cmdBeta.Title.Returns("Command Beta");

        var target = targetName == "command-alpha" ? cmdAlpha : cmdBeta;
        target.ExecuteAsync(Arg.Any<CancellationToken>())
            .Returns(_ => { executed = true; return Task.CompletedTask; });

        var subMenu = new SubMenuCommand("Branch", [cmdAlpha, cmdBeta]);
        var app = new CommandApp();
        app.Configure(config => CliCommandBuilder.Configure(config, [subMenu]));

        app.Run(["branch", targetName]);

        Assert.True(executed);
    }

    // -------------------------------------------------------------------------
    // Helpers for ICliParameterised tests
    // -------------------------------------------------------------------------

    private sealed class CommandState
    {
        public bool WithSettingsCalled { get; set; }
        public bool ExecuteCalled { get; set; }
        public CommandSettings? ReceivedSettings { get; set; }
    }

    private sealed class FakeSettings : MenuCommandSettings { }

    private sealed class TrackingCliCommand(CommandState state, string title = "Tracking Command")
        : IMenuCommand, ICliParameterised
    {
        public string Title => title;

        Type ICliParameterised.SettingsType => typeof(FakeSettings);

        IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        {
            state.WithSettingsCalled = true;
            state.ReceivedSettings = settings;
            return new TrackingCliCommand(state, title);
        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            state.ExecuteCalled = true;
            return Task.CompletedTask;
        }
    }
}
