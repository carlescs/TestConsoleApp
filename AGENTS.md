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
│       └── CommandRegistry.cs        # Runtime list of registered commands
└── Presentation/
    ├── Menus/
    │   └── MainMenu.cs                # Spectre.Console interactive selection loop
    └── Commands/
        └── SayHelloCommand.cs         # Example command implementation

TestConsoleApp.Generators/
└── CommandRegistrationGenerator.cs   # IIncrementalGenerator — emits CommandRegistrationInitializer.g.cs
```

### Auto-Registration Flow

1. `CommandRegistrationGenerator` scans the compilation for every non-abstract class that implements `IMenuCommand`.
2. It emits `CommandRegistrationInitializer.g.cs` (in `TestConsoleApp.Generated`) containing a `[ModuleInitializer]` method that calls `CommandRegistry.Register(new TCommand())` for each discovered type.
3. At runtime, the module initializer fires before `Program.cs`, so `CommandRegistry.Commands` is already populated when `MainMenu` is constructed.

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
