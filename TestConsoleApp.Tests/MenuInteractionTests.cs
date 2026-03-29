using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests;

public sealed class MenuInteractionTests
{
    // -------------------------------------------------------------------------
    // HotkeyMatches
    // -------------------------------------------------------------------------

    [Fact]
    public void HotkeyMatches_ReturnsTrue_WhenCharAndModifiersMatch()
    {
        var ki = new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false);

        Assert.True(MenuInteraction.HotkeyMatches(ki, 'H', default));
    }

    [Fact]
    public void HotkeyMatches_IsCaseInsensitive_WhenNoModifiers()
    {
        var ki = new ConsoleKeyInfo('H', ConsoleKey.H, false, false, false);

        Assert.True(MenuInteraction.HotkeyMatches(ki, 'h', default));
    }

    [Fact]
    public void HotkeyMatches_ReturnsFalse_WhenCharDiffers()
    {
        var ki = new ConsoleKeyInfo('x', ConsoleKey.X, false, false, false);

        Assert.False(MenuInteraction.HotkeyMatches(ki, 'H', default));
    }

    [Fact]
    public void HotkeyMatches_ReturnsFalse_WhenModifierDiffers()
    {
        var ki = new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false);

        Assert.False(MenuInteraction.HotkeyMatches(ki, 'H', ConsoleModifiers.Control));
    }

    [Fact]
    public void HotkeyMatches_WithCtrl_UsesConsoleKeyNotKeyChar()
    {
        // Ctrl+H produces KeyChar = '\b' (backspace), but ConsoleKey = H
        var ki = new ConsoleKeyInfo('\x08', ConsoleKey.H, false, false, true);

        Assert.True(MenuInteraction.HotkeyMatches(ki, 'H', ConsoleModifiers.Control));
    }

    [Fact]
    public void HotkeyMatches_WithAlt_UsesConsoleKeyNotKeyChar()
    {
        var ki = new ConsoleKeyInfo('\0', ConsoleKey.H, false, true, false);

        Assert.True(MenuInteraction.HotkeyMatches(ki, 'H', ConsoleModifiers.Alt));
    }

    [Fact]
    public void HotkeyMatches_IgnoresShift_WhenComparingModifiers()
    {
        // Shift is stripped from both sides, so Shift+H should match a no-modifier hotkey
        var ki = new ConsoleKeyInfo('H', ConsoleKey.H, true, false, false);

        Assert.True(MenuInteraction.HotkeyMatches(ki, 'H', default));
    }

    [Fact]
    public void HotkeyMatches_ReturnsFalse_WhenAltRequiredButNotPressed()
    {
        var ki = new ConsoleKeyInfo('h', ConsoleKey.H, false, false, false);

        Assert.False(MenuInteraction.HotkeyMatches(ki, 'H', ConsoleModifiers.Alt));
    }

    // -------------------------------------------------------------------------
    // BuildBadgeContent
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(ConsoleModifiers.None, 'H', "H")]
    [InlineData(ConsoleModifiers.Control, 'H', "^H")]
    [InlineData(ConsoleModifiers.Alt, 'H', "~H")]
    [InlineData(ConsoleModifiers.Control | ConsoleModifiers.Alt, 'H', "^~H")]
    [InlineData(ConsoleModifiers.None, 'h', "H")]  // lowercase key is uppercased
    public void BuildBadgeContent_ReturnsExpectedString(ConsoleModifiers modifiers, char key, string expected) => Assert.Equal(expected, MenuInteraction.BuildBadgeContent(modifiers, key));

    // -------------------------------------------------------------------------
    // BuildBadgeMarkup
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildBadgeMarkup_NoModifier_ContainsBadgeAndPadding()
    {
        // content = "H" (1 char), padding = 8 - 1 - 2 = 5 spaces
        var result = MenuInteraction.BuildBadgeMarkup('H', default);

        Assert.Equal("[[[bold cyan]H[/]]]     ", result);
    }

    [Fact]
    public void BuildBadgeMarkup_WithCtrl_ContainsCaretPrefix()
    {
        // content = "^H" (2 chars), padding = 8 - 2 - 2 = 4 spaces
        var result = MenuInteraction.BuildBadgeMarkup('H', ConsoleModifiers.Control);

        Assert.Equal("[[[bold cyan]^H[/]]]    ", result);
    }

    [Fact]
    public void BuildBadgeMarkup_WithCtrlAndAlt_ContainsBothPrefixes()
    {
        // content = "^~H" (3 chars), padding = 8 - 3 - 2 = 3 spaces
        var result = MenuInteraction.BuildBadgeMarkup('H', ConsoleModifiers.Control | ConsoleModifiers.Alt);

        Assert.Equal("[[[bold cyan]^~H[/]]]   ", result);
    }
}
