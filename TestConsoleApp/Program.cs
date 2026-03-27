using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

var menu = new MainMenu(CommandRegistry.Commands);
await menu.RunAsync();
