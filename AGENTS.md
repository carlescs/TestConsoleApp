# AGENTS.md

> **Keep this file updated** as the project evolves — add new conventions, patterns, and architectural decisions as they are introduced.

## Project Overview

`TestConsoleApp` is a .NET 10 console application that presents an interactive menu driven by [Spectre.Console](https://spectreconsole.net/). Commands are discovered and registered automatically at startup via a Roslyn incremental source generator (`.NET Standard 2.0`).

### Projects

| Project | Target | Purpose |
|---|---|---|
| `TestConsoleApp` | .NET 10 | Runnable console app — UI, commands, entry point |
| `TestConsoleApp.Generators` | .NET Standard 2.0 | Roslyn source generator — auto-registers commands |

---

## Architecture

```
TestConsoleApp/
├── Program.cs                         # Entry point — wires CommandRegistry into MainMenu
├── Application/
│   └── Abstractions/
│       ├── IMenuCommand.cs            # Command contract (Title + ExecuteAsync)
│       ├── CommandRegistry.cs         # Runtime list of registered commands
│       └── SubMenuAttribute.cs        # [SubMenu("Name")] — groups commands under a submenu
└── Presentation/
    ├── Menus/
    │   ├── MainMenu.cs                # Spectre.Console interactive selection loop
    │   └── SubMenuCommand.cs          # IMenuCommand that renders a nested selection loop
    └── Commands/
        ├── SayHelloCommand.cs         # Standalone command example
        ├── ShowDateTimeCommand.cs     # Submenu command example ([SubMenu("Utilities")])
        └── GenerateGuidCommand.cs     # Submenu command example ([SubMenu("Utilities")])

TestConsoleApp.Generators/
├── CommandRegistrationGenerator.cs   # IIncrementalGenerator — emits CommandRegistrationInitializer.g.cs
└── IsExternalInit.cs                  # netstandard2.0 polyfill for record struct support
```

### Auto-Registration Flow

1. `CommandRegistrationGenerator` scans the compilation for every non-abstract class that:
   - implements `IMenuCommand`, and
   - has a `public` parameterless constructor.
2. Commands **without** `[SubMenu]` are emitted as `CommandRegistry.Register(new TCommand())`.
3. Commands **with** `[SubMenu("Group")]` are grouped by name and emitted as a single `CommandRegistry.Register(new SubMenuCommand("Group", [new T1(), new T2()]))` per group.
4. The emitted `CommandRegistrationInitializer.g.cs` uses `[ModuleInitializer]` so all registrations happen before `Program.cs` executes.

---

## Adding a New Command

1. Create a `public sealed class` in `TestConsoleApp/Presentation/Commands/`.
2. Implement `IMenuCommand`:
   - `string Title { get; }` — label shown in the menu.
   - `Task ExecuteAsync(CancellationToken cancellationToken = default)` — command logic.
3. **No manual registration needed.** The source generator picks up the new class automatically on the next build.

```csharp
// Example skeleton
using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

public sealed class MyNewCommand : IMenuCommand
{
    public string Title => "My New Command";

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[green]Hello from MyNewCommand![/]");
        return Task.CompletedTask;
    }
}
```

---

## Adding a Submenu

Decorate any `IMenuCommand` class with `[SubMenu("Group Name")]` to place it inside a named submenu instead of the root menu. All commands sharing the same group name are collected into one `SubMenuCommand` entry automatically on the next build.

```csharp
using Spectre.Console;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands;

[SubMenu("Utilities")]
public sealed class MyUtilityCommand : IMenuCommand
{
    public string Title => "My Utility";

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[green]Running utility...[/]");
        return Task.CompletedTask;
    }
}
```

> **Note:** Classes that implement `IMenuCommand` but lack a `public` parameterless constructor (e.g., `SubMenuCommand` itself) are automatically excluded from registration.

---

## Coding Conventions

- **Language version**: latest C# features (target .NET 10 in the app project).
- **Nullability**: `#nullable enable` — avoid nullable warnings.
- **Immutability**: prefer `sealed` classes; use `readonly` fields and collection expressions (`[]`).
- **Async**: use `async`/`await`; honour `CancellationToken` where applicable.
- **UI**: all console output goes through `Spectre.Console` (`AnsiConsole`). Do not use `Console.Write` directly except where Spectre.Console has no equivalent (e.g., `Console.ReadKey`).
- **Namespaces**: follow the folder structure — `TestConsoleApp.<Layer>.<SubLayer>`.
- **Generator project**: targets `.NET Standard 2.0` (Roslyn requirement). Keep it free of app-level dependencies.

---

## Build & Run

```powershell
# Restore and build
dotnet build

# Run the app
dotnet run --project TestConsoleApp/TestConsoleApp.csproj
```

---

## Dependencies

| Package | Used in | Purpose |
|---|---|---|
| `Spectre.Console` | `TestConsoleApp` | Rich terminal UI (menus, prompts, markup) |
| `Microsoft.CodeAnalysis.CSharp` | `TestConsoleApp.Generators` | Roslyn APIs for the source generator |
| *(polyfill)* `IsExternalInit` | `TestConsoleApp.Generators` | Enables `record struct` on netstandard2.0 |
