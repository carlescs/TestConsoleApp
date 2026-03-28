using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Cli;
using TestConsoleApp.Presentation.Menus;

if (args.Length == 0)
{
    var menu = new MainMenu(CommandRegistry.Commands);
    await menu.RunAsync();
    return 0;
}

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("testconsoleapp");
    CliCommandBuilder.Configure(config, CommandRegistry.Commands);
});
return await app.RunAsync(args);
