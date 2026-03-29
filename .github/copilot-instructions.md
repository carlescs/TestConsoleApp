# GitHub Copilot Instructions

## Commit Messages — Conventional Commits

All commit messages **must** follow the [Conventional Commits](https://www.conventionalcommits.org/) specification so that the `version.yml` workflow can automatically derive the correct semantic version bump.

### Format

```
<type>[optional scope]: <short description>

[optional body]

[optional footer(s)]
```

### Types

| Type | When to use | Version bump |
|---|---|---|
| `feat` | A new feature or capability | **minor** |
| `fix` | A bug fix | patch |
| `perf` | A performance improvement | patch |
| `refactor` | Code change that neither fixes a bug nor adds a feature | patch |
| `test` | Adding or updating tests | patch |
| `chore` | Build process, tooling, dependency updates | patch |
| `docs` | Documentation-only changes | patch |
| `style` | Code formatting, whitespace (no logic changes) | patch |
| `ci` | Changes to CI/CD configuration | patch |
| `revert` | Reverts a previous commit | patch |

### Breaking Changes → major bump

Append `!` after the type/scope **or** add a `BREAKING CHANGE:` footer:

```
feat!: remove deprecated --output flag

feat(cli)!: redesign argument parser

feat: switch config format

BREAKING CHANGE: JSON config files are no longer supported; migrate to TOML.
```

### Examples

```
feat(commands): add ExportCommand to Utilities submenu
fix: prevent NullReferenceException when registry is empty
chore: update Spectre.Console to 0.54.0
docs: document SubMenuAttribute usage in AGENTS.md
ci: add Create Version Tag workflow
feat!: replace IMenuCommand with ICommand interface
```

### Rules

- Use the **imperative mood** in the description ("add", not "added" or "adds").
- Keep the first line ≤ 72 characters.
- Do **not** end the description with a period.
- Separate the body from the subject with a blank line.
- Reference issues in the footer: `Closes #42` or `Fixes #17`.
