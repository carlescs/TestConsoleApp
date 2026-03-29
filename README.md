# 🖥️ TestConsoleApp

[![.NET](https://github.com/carlescs/TestConsoleApp/actions/workflows/dotnet.yml/badge.svg)](https://github.com/carlescs/TestConsoleApp/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.txt)

A **.NET 10 global dotnet tool** that presents a rich interactive menu powered by [Spectre.Console](https://spectreconsole.net/). Every command is also accessible directly from the command line via [Spectre.Console.Cli](https://spectreconsole.net/cli), so the tool works equally well interactively or in scripts. Commands are discovered and registered **automatically** at startup through a Roslyn incremental source generator — no manual wiring needed.

---

## ✨ Features

- 🎨 **Rich interactive menu** — Navigate with arrow keys; submenus are supported out of the box.
- ⚡ **Direct CLI access** — Every menu command is available as a kebab-case CLI sub-command.
- 🔌 **Auto-registration** — Drop a class that implements `IMenuCommand` and it appears in the menu on the next build.
- 🗂️ **Submenu grouping** — Decorate a command with `[SubMenu("Group")]` to nest it under a named group.
- 🔢 **Automatic versioning** — [MinVer](https://github.com/adamralph/minver) drives semantic versioning from git tags; no CI commits required.
- 📦 **GitHub Packages publishing** — The tool is packed and pushed automatically on tagged releases.

---

## 📋 Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or newer.

---

## 📦 Installation

Install the tool globally from **GitHub Packages**:

```bash
dotnet tool install --global TestConsoleApp \
  --add-source https://nuget.pkg.github.com/carlescs/index.json
```

> **Note:** You may need a [GitHub personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token) with `read:packages` scope to authenticate against the feed.

---

## 🚀 Usage

### 🖱️ Interactive menu

Run without arguments to open the interactive menu:

```bash
testconsoleapp
```

Use the **↑ / ↓** arrow keys to highlight a command and **Enter** to run it. Select **Exit** (or press **Ctrl+C**) to quit.

### 🔧 Direct CLI commands

Pass a command name to run it directly without the interactive UI:

```bash
# Standalone command
testconsoleapp say-hello

# Commands nested inside a submenu
testconsoleapp utilities show-date-time
testconsoleapp utilities generate-guid

# Built-in help
testconsoleapp --help
testconsoleapp utilities --help
```

Command titles are automatically converted to **kebab-case** (e.g., `"Say Hello"` → `say-hello`). Submenu group names become CLI branches using the same rule.

---

## 🏗️ Architecture

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

### 🔄 Auto-Registration Flow

1. `CommandRegistrationGenerator` scans the compilation for every non-abstract class that:
   - implements `IMenuCommand`, and
   - has a `public` parameterless constructor.
2. Commands **without** `[SubMenu]` are emitted as `CommandRegistry.Register(new TCommand())`.
3. Commands **with** `[SubMenu("Group")]` are grouped by name and emitted as a single `CommandRegistry.Register(new SubMenuCommand("Group", [new T1(), new T2()]))` per group.
4. The emitted `CommandRegistrationInitializer.g.cs` uses `[ModuleInitializer]` so all registrations happen **before** `Program.cs` executes.

---

## ➕ Adding a New Command

1. Create a `public sealed class` in `TestConsoleApp/Presentation/Commands/`.
2. Implement `IMenuCommand`:
   - `string Title { get; }` — label shown in the menu.
   - `Task ExecuteAsync(CancellationToken cancellationToken = default)` — command logic.
3. **No manual registration needed.** 🎉 The source generator picks up the new class automatically on the next build.

```csharp
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

### 🗂️ Adding a Submenu

Decorate the class with `[SubMenu("Group Name")]` to nest it under a named group. All commands sharing the same group name are collected into one submenu entry automatically:

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

> **ℹ️ Note:** Classes that implement `IMenuCommand` but lack a `public` parameterless constructor (e.g., `SubMenuCommand` itself) are automatically excluded from registration.

---

## 🔨 Build & Run

```bash
# Restore and build
dotnet build

# Run the interactive menu
dotnet run --project TestConsoleApp/TestConsoleApp.csproj

# Run a specific CLI command directly
dotnet run --project TestConsoleApp/TestConsoleApp.csproj -- say-hello
```

---

## 🧪 Tests

```bash
dotnet test
```

The solution includes two test projects:

| Project | What it tests |
|---|---|
| `TestConsoleApp.Tests` | Unit tests for `CommandRegistry`, `MainMenu`, and `SubMenuCommand` |
| `TestConsoleApp.Generators.Tests` | Verifies that the source generator emits correct registration code |

---

## 🔢 Versioning

Versioning is fully automated via **MinVer** — no pipeline commits required.

- MinVer reads the nearest reachable git tag of the form `v<major>.<minor>.<patch>` and computes the version from commit height.
- Without a tag, the version defaults to `1.0.0-alpha.0.<height>` (controlled by `MinVerMinimumMajorMinor=1.0` in the csproj).
- To cut a stable release, push a tag: `git tag v1.2.3 && git push --tags`. The next CI run will pack and publish `1.2.3`.

---

## 📤 Publishing

The CI pipeline (`.github/workflows/dotnet.yml`) builds, tests, packs, and pushes the NuGet package to the **GitHub Packages** registry. It is triggered either by a direct `v*` tag push or dispatched directly by the `Create Version Tag` workflow after it creates the tag.

- `--skip-duplicate` is used so re-running the workflow for the same version is safe.
- Packages are published to: `https://nuget.pkg.github.com/carlescs/index.json`

---

## 📜 License

This project is licensed under the [GNU General Public License v3.0](LICENSE.txt).