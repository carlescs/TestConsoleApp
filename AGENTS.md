# AGENTS.md

> **Keep this file updated** as the project evolves — add new conventions, patterns, and architectural decisions as they are introduced.

## Project Overview

`TestConsoleApp` is a .NET 10 **global dotnet tool** that presents an interactive menu driven by [Spectre.Console](https://spectreconsole.net/). It also exposes every command directly via [Spectre.Console.Cli](https://spectreconsole.net/cli) so it can be scripted without the interactive UI. Commands are discovered and registered automatically at startup via a Roslyn incremental source generator (`.NET Standard 2.0`). Versioning is handled by [MinVer](https://github.com/adamralph/minver) — no pipeline commits required.

### Projects

| Project | Target | Purpose |
|---|---|---|
| `TestConsoleApp` | .NET 10 | Runnable console app — UI, commands, entry point |
| `TestConsoleApp.Generators` | .NET Standard 2.0 | Roslyn source generator — auto-registers commands |

---

## Architecture

```
TestConsoleApp/
├── Program.cs                         # Entry point — menu (no args) or CLI routing (args present)
├── Application/
│   └── Abstractions/
│       ├── IMenuCommand.cs            # Command contract (Title + ExecuteAsync)
│       ├── CommandRegistry.cs         # Runtime list of registered commands
│       └── SubMenuAttribute.cs        # [SubMenu("Name")] — groups commands under a submenu
└── Presentation/
    ├── Cli/
    │   └── CliCommandBuilder.cs       # Maps IMenuCommand tree → Spectre.Console.Cli CommandApp
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

## CLI Direct Access

When the tool is invoked with arguments, `Program.cs` hands control to a `Spectre.Console.Cli` `CommandApp` instead of showing the interactive menu. `CliCommandBuilder` walks `CommandRegistry.Commands` and registers each `IMenuCommand` as a kebab-case CLI command (e.g., `"Say Hello"` → `say-hello`). Sub-menus become CLI branches.

```
# Interactive menu (no args)
testconsoleapp

# Direct command access
testconsoleapp say-hello
testconsoleapp utilities show-date-time
testconsoleapp utilities generate-guid

# Built-in help
testconsoleapp --help
testconsoleapp utilities --help
```

---

## Versioning

Versioning is fully automated via **MinVer** — no pipeline commits required.

- MinVer reads the nearest reachable git tag of the form `v<major>.<minor>.<patch>` and computes the version from commit height.
- Without a tag, the version is `1.0.0-alpha.0.<height>` (controlled by `MinVerMinimumMajorMinor=1.0` in the csproj).
- To release a stable version, push a tag: `git tag v1.2.3 && git push --tags`. The next CI run will publish `1.2.3`.
- The pipeline (`dotnet.yml`) uses `fetch-depth: 0` so MinVer can walk the full tag history.

---

## Publishing

Packages are published to the **GitHub Packages NuGet registry** on every push of a `v*` tag.

- The tool can be installed globally: `dotnet tool install --global TestConsoleApp --add-source https://nuget.pkg.github.com/carlescs/index.json`
- `--skip-duplicate` is used in the push step so re-running the workflow for the same version is safe.

> **Required secret:** The `Create Version Tag` workflow (`version.yml`) must be granted a repository secret named **`PAT`** containing a [GitHub Personal Access Token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) with `repo` (or `contents: write`) scope. This is necessary because GitHub does not allow events created by `GITHUB_TOKEN` to trigger downstream workflows — without a PAT, pushing a version tag from the workflow would **not** trigger the build and publish pipeline in `dotnet.yml`.

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
