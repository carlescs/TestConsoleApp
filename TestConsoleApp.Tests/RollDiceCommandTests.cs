using System.Reflection;
using Spectre.Console.Testing;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Commands;

namespace TestConsoleApp.Tests;

public sealed class RollDiceCommandTests
{
    private readonly RollDiceCommand _sut = new();

    [Fact]
    public void Title_ReturnsRollDice()
    {
        Assert.Equal("Roll Dice", _sut.Title);
    }

    [Fact]
    public void ImplementsIMenuCommand()
    {
        Assert.IsAssignableFrom<IMenuCommand>(_sut);
    }

    [Fact]
    public void HasSubMenuAttribute_InUtilitiesPath()
    {
        var attr = typeof(RollDiceCommand).GetCustomAttribute<SubMenuAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(["Utilities"], attr!.Path);
    }

    [Fact]
    public void HasHotkeyAttribute_WithKeyR()
    {
        var attr = typeof(RollDiceCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal('R', attr!.Key);
    }

    [Fact]
    public void HasHotkeyAttribute_WithControlModifier()
    {
        var attr = typeof(RollDiceCommand).GetCustomAttribute<HotkeyAttribute>();

        Assert.NotNull(attr);
        Assert.Equal(ConsoleModifiers.Control, attr!.Modifiers);
    }

    [Fact]
    public void HasCommandDescriptionAttribute()
    {
        var attr = typeof(RollDiceCommand).GetCustomAttribute<CommandDescriptionAttribute>();

        Assert.NotNull(attr);
        Assert.False(string.IsNullOrWhiteSpace(attr!.Description));
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsSidesLabel()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync();

        Assert.Contains("6-sided", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsRollResult()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 4).ExecuteAsync();

        Assert.Contains("4", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultSides_IsSix()
    {
        var console = new TestConsole();
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 6
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);
        int capturedMax = 0;

        await new RollDiceCommand(console, roll: (min, max) => { capturedMax = max; return min; }).ExecuteAsync();

        Assert.Equal(7, capturedMax); // sides + 1 = 6 + 1
    }

    [Fact]
    public async Task ExecuteAsync_RollIsCalledWithOneAsMin()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("10");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);
        int capturedMin = 0;

        await new RollDiceCommand(console, roll: (min, max) => { capturedMin = min; return min; }).ExecuteAsync();

        Assert.Equal(1, capturedMin);
    }

    [Fact]
    public async Task ExecuteAsync_RollIsCalledWithSidesPlusOneAsMax()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("20");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);
        int capturedMax = 0;

        await new RollDiceCommand(console, roll: (min, max) => { capturedMax = max; return min; }).ExecuteAsync();

        Assert.Equal(21, capturedMax); // 20 + 1
    }

    [Fact]
    public async Task ExecuteAsync_CustomSides_AppearsInOutput()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("20");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 1).ExecuteAsync();

        Assert.Contains("20-sided", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSidesFollowedByValid_UsesValidInput()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("1"); // invalid: less than 2
        console.Input.PushTextWithEnter("8"); // valid
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 5).ExecuteAsync();

        Assert.Contains("8-sided", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_PrintsRollAgainHint()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync();

        Assert.Contains("Esc to exit", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotThrow()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        var exception = await Record.ExceptionAsync(
            () => new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync());

        Assert.Null(exception);
    }

    [Fact]
    public async Task ExecuteAsync_RollsAgainAfterNonEscapeKeyPress()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Enter);  // roll again
        console.Input.PushKey(ConsoleKey.Escape); // exit
        int rollCount = 0;

        await new RollDiceCommand(console, roll: (_, _) => { rollCount++; return 1; }).ExecuteAsync();

        Assert.Equal(2, rollCount);
    }

    [Fact]
    public async Task ExecuteAsync_EscapeExitsAfterFirstRoll()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);
        int rollCount = 0;

        await new RollDiceCommand(console, roll: (_, _) => { rollCount++; return 1; }).ExecuteAsync();

        Assert.Equal(1, rollCount);
    }

    // -------------------------------------------------------------------------
    // Histogram integration
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_OutputContainsFilledAndEmptyBars()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("3"); // 3-sided dice
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 2).ExecuteAsync();

        Assert.Contains("\u2588", console.Output); // filled block
        Assert.Contains("\u2591", console.Output); // light shade
    }

    [Fact]
    public async Task ExecuteAsync_OutputContainsRollCounter()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync();

        Assert.Contains("#1", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_RollCounterIncrementsWithEachRoll()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // accept default of 1 die
        console.Input.PushKey(ConsoleKey.Enter);  // roll again
        console.Input.PushKey(ConsoleKey.Escape); // exit

        await new RollDiceCommand(console, roll: (_, _) => 1).ExecuteAsync();

        Assert.Contains("#2", console.Output);
    }

    // -------------------------------------------------------------------------
    // BuildHistogramData — pure-logic unit tests
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildHistogramData_RowCount_MatchesSides()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 1, 6, 1);

        Assert.Equal(6, rows.Count);
    }

    [Fact]
    public void BuildHistogramData_Faces_AreOneToSides()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 1, 4, 1);

        Assert.Equal([1, 2, 3, 4], rows.Select(r => r.Face).ToArray());
    }

    [Fact]
    public void BuildHistogramData_WithEmptyCounts_AllCountsAreZero()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 1, 6, 3);

        Assert.All(rows, r => Assert.Equal(0, r.Count));
    }

    [Fact]
    public void BuildHistogramData_WithEmptyCounts_AllFillRatiosAreZero()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 1, 6, 3);

        Assert.All(rows, r => Assert.Equal(0.0, r.FillRatio));
    }

    [Fact]
    public void BuildHistogramData_LastRoll_IsMarked()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int> { { 4, 1 } }, 1, 6, 4);

        Assert.True(rows.Single(r => r.Face == 4).IsLastRoll);
    }

    [Fact]
    public void BuildHistogramData_OtherFaces_AreNotMarked()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int> { { 4, 1 } }, 1, 6, 4);

        Assert.All(rows.Where(r => r.Face != 4), r => Assert.False(r.IsLastRoll));
    }

    [Fact]
    public void BuildHistogramData_SoleRolledFace_HasFillRatioOne()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int> { { 3, 5 } }, 1, 6, 3);

        Assert.Equal(1.0, rows.Single(r => r.Face == 3).FillRatio);
    }

    [Fact]
    public void BuildHistogramData_UnrolledFaces_HaveZeroFillRatio()
    {
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int> { { 3, 5 } }, 1, 6, 3);

        Assert.All(rows.Where(r => r.Face != 3), r => Assert.Equal(0.0, r.FillRatio));
    }

    [Fact]
    public void BuildHistogramData_FillRatio_IsProportionalToMaxCount()
    {
        var counts = new Dictionary<int, int> { { 1, 1 }, { 2, 2 } };

        var rows = RollDiceCommand.BuildHistogramData(counts, 1, 3, 2);

        Assert.Equal(0.5, rows[0].FillRatio); // face 1: 1/2
        Assert.Equal(1.0, rows[1].FillRatio); // face 2: 2/2
        Assert.Equal(0.0, rows[2].FillRatio); // face 3: not rolled
    }

    [Fact]
    public void BuildHistogramData_OnlyOneIsLastRoll()
    {
        var rows = RollDiceCommand.BuildHistogramData(
            new Dictionary<int, int> { { 1, 2 }, { 2, 3 }, { 3, 1 } }, 1, 3, 2);

        Assert.Single(rows, r => r.IsLastRoll);
    }

    [Fact]
    public void BuildHistogramData_CountMatchesDictionaryValue()
    {
        var counts = new Dictionary<int, int> { { 5, 7 } };

        var rows = RollDiceCommand.BuildHistogramData(counts, 1, 6, 5);

        Assert.Equal(7, rows.Single(r => r.Face == 5).Count);
    }

    // -------------------------------------------------------------------------
    // Multiple dice
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ExecuteAsync_WithMultipleDice_HeaderShowsDiceNotation()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");  // sides
        console.Input.PushTextWithEnter("2");  // 2 dice
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync();

        Assert.Contains("2d6", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithOneDie_HeaderShowsSidedDiceLabel()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // default 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 3).ExecuteAsync();

        Assert.Contains("6-sided dice", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleDice_RollIsCalledOncePerDie()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");  // sides
        console.Input.PushTextWithEnter("3");  // 3 dice
        console.Input.PushKey(ConsoleKey.Escape);
        int callCount = 0;

        await new RollDiceCommand(console, roll: (_, _) => { callCount++; return 2; }).ExecuteAsync();

        Assert.Equal(3, callCount); // 3 dice × 1 iteration
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleDice_SumAppearsInOutput()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("10"); // sides
        console.Input.PushTextWithEnter("2");  // 2 dice, each returns 4 → sum 8
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 4).ExecuteAsync();

        Assert.Contains("8", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleDice_ShowsIndividualRolls()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");  // sides
        console.Input.PushTextWithEnter("2");  // 2 dice
        console.Input.PushKey(ConsoleKey.Escape);
        int callIndex = 0;
        int[] returns = [3, 5];

        await new RollDiceCommand(console, roll: (_, _) => returns[callIndex++]).ExecuteAsync();

        Assert.Contains("(3, 5)", console.Output);
    }

    [Fact]
    public async Task ExecuteAsync_WithOneDie_DoesNotShowIndividualRolls()
    {
        var console = new TestConsole();
        console.Input.PushTextWithEnter("6");
        console.Input.PushKey(ConsoleKey.Enter); // default 1 die
        console.Input.PushKey(ConsoleKey.Escape);

        await new RollDiceCommand(console, roll: (_, _) => 4).ExecuteAsync();

        Assert.DoesNotContain("(4)", console.Output);
    }

    [Fact]
    public void BuildHistogramData_WithMultipleDice_RowCountMatchesSumRange()
    {
        // 2d6: sums 2..12 = 11 rows
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 2, 12, 7);

        Assert.Equal(11, rows.Count);
    }

    [Fact]
    public void BuildHistogramData_WithMultipleDice_FacesSpanCorrectRange()
    {
        // 2d6: faces should be 2, 3, …, 12
        var rows = RollDiceCommand.BuildHistogramData(new Dictionary<int, int>(), 2, 12, 7);

        Assert.Equal(2, rows[0].Face);
        Assert.Equal(12, rows[^1].Face);
    }
}