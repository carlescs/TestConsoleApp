using Spectre.Console;
using Spectre.Console.Cli;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Presentation.Commands.RollDice;

/// <summary>
/// Holds the histogram data for a single face value of the dice.
/// </summary>
/// <param name="Face">The face value (1 to sides).</param>
/// <param name="Count">How many times this face has been rolled.</param>
/// <param name="FillRatio">
/// Proportion of the bar to fill relative to the most-rolled face (0.0–1.0).
/// </param>
/// <param name="IsLastRoll">Whether this face was the most recently rolled value.</param>
internal sealed record HistogramRow(int Face, int Count, double FillRatio, bool IsLastRoll);

/// <summary>
/// A menu command in the <c>Utilities</c> submenu that rolls a dice with a configurable
/// number of sides, showing a live histogram of the distribution after each roll.
/// </summary>
[SubMenu("Utilities")]
[Hotkey('R', ConsoleModifiers.Control)]
[CommandDescription("Rolls one or more dice with configurable sides (default: 1d6). Press any key to roll again, Esc to exit.")]
public sealed class RollDiceCommand(IAnsiConsole? console = null, Func<int, int, int>? roll = null, RollDiceSettings? cliSettings = null) : IMenuCommand, ICliParameterised
{
    private const int BarWidth = 20;

    private readonly IAnsiConsole _console = console ?? AnsiConsole.Console;
    private readonly Func<int, int, int> _roll = roll ?? ((min, max) => Random.Shared.Next(min, max));
    private readonly RollDiceSettings? _cliSettings = cliSettings;

    Type ICliParameterised.SettingsType => typeof(RollDiceSettings);

    IMenuCommand ICliParameterised.WithSettings(CommandSettings settings)
        => new RollDiceCommand(_console, _roll, settings as RollDiceSettings);

    /// <inheritdoc/>
    public string Title => "Roll Dice";

    /// <inheritdoc/>
    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _console.Clear();

        int sides = _cliSettings?.Sides ?? _console.Prompt(
            new TextPrompt<int>("How many sides?")
                .DefaultValue(6)
                .Validate(n => n >= 2
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be at least 2.[/]")));

        int numDice = _cliSettings?.NumDice ?? _console.Prompt(
            new TextPrompt<int>("Number of dice?")
                .DefaultValue(1)
                .Validate(n => n >= 1
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Must be at least 1.[/]")));

        var counts = new Dictionary<int, int>();
        int totalRolls = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            _console.Clear();
            int[] rolls = Enumerable.Range(0, numDice).Select(_ => _roll(1, sides + 1)).ToArray();
            int result = rolls.Sum();
            counts[result] = counts.GetValueOrDefault(result) + 1;
            totalRolls++;

            string label = numDice == 1 ? $"a {sides}-sided dice" : $"{numDice}d{sides}";
            _console.MarkupLine($"[bold]Rolling {label}:[/] [bold green]{result}[/] [dim](#{totalRolls})[/]");
            if (numDice > 1)
                _console.MarkupLine($"[dim]  ({string.Join(", ", rolls)})[/]");
            _console.WriteLine();
            RenderHistogram(BuildHistogramData(counts, numDice, numDice * sides, result));
            _console.MarkupLine("\n[dim]Any key to roll again  \u00b7  Esc to exit[/]");
            if (_console.Input.ReadKey(intercept: true)?.Key == ConsoleKey.Escape)
                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Builds histogram rows from the current roll counts — pure logic with no I/O.
    /// </summary>
    /// <param name="counts">Accumulated roll counts keyed by face or sum value.</param>
    /// <param name="minFace">The lowest possible value (1 for a single die, numDice for multiple).</param>
    /// <param name="maxFace">The highest possible value (sides for a single die, numDice × sides for multiple).</param>
    /// <param name="lastRoll">The value of the most recent roll or sum.</param>
    internal static IReadOnlyList<HistogramRow> BuildHistogramData(
        IReadOnlyDictionary<int, int> counts,
        int minFace,
        int maxFace,
        int lastRoll)
    {
        int maxCount = counts.Values.DefaultIfEmpty(0).Max();
        return Enumerable.Range(minFace, maxFace - minFace + 1)
            .Select(face =>
            {
                int count = counts.GetValueOrDefault(face);
                return new HistogramRow(
                    face,
                    count,
                    maxCount > 0 ? (double)count / maxCount : 0.0,
                    face == lastRoll);
            })
            .ToList();
    }

    private void RenderHistogram(IReadOnlyList<HistogramRow> rows)
    {
        foreach (var row in rows)
        {
            int filled = (int)Math.Round(row.FillRatio * BarWidth);
            string bar = new string('\u2588', filled) + new string('\u2591', BarWidth - filled);
            string face = row.IsLastRoll ? $"[bold green]{row.Face,2}[/]" : $"[dim]{row.Face,2}[/]";
            string count = row.IsLastRoll ? $"[bold green]{row.Count}[/]" : $"[dim]{row.Count}[/]";
            string arrow = row.IsLastRoll ? " [bold green]\u2190[/]" : string.Empty;
            _console.MarkupLine($" {face} {bar} {count}{arrow}");
        }
    }
}
