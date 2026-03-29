using NSubstitute;
using TestConsoleApp.Application.Abstractions;
using TestConsoleApp.Presentation.Menus;

namespace TestConsoleApp.Tests;

public sealed class HelpRendererTests
{
    // -------------------------------------------------------------------------
    // Empty / no commands
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildSections_WithNoCommands_ReturnsEmptyList()
    {
        var sections = HelpRenderer.BuildSections([]);

        Assert.Empty(sections);
    }

    [Fact]
    public void BuildSections_EmptySubMenu_ProducesNoSection()
    {
        var subMenu = new SubMenuCommand("Empty", []);

        var sections = HelpRenderer.BuildSections([subMenu]);

        Assert.Empty(sections);
    }

    // -------------------------------------------------------------------------
    // Heading
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildSections_DefaultHeading_IsMainMenu()
    {
        var cmd = Command("X");

        var sections = HelpRenderer.BuildSections([cmd]);

        Assert.Equal("Main Menu", sections[0].Heading);
    }

    [Fact]
    public void BuildSections_CustomHeading_IsUsed()
    {
        var cmd = Command("X");

        var sections = HelpRenderer.BuildSections([cmd], "My Section");

        Assert.Equal("My Section", sections[0].Heading);
    }

    [Fact]
    public void BuildSections_SubMenuHeading_IsSubMenuTitle()
    {
        var leaf = Command("Leaf");
        var subMenu = new SubMenuCommand("Tools", [leaf]);

        var sections = HelpRenderer.BuildSections([subMenu]);

        Assert.Equal("Tools", sections[0].Heading);
    }

    // -------------------------------------------------------------------------
    // Section count and ordering
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildSections_SingleLeafCommand_ReturnsSingleSection()
    {
        var sections = HelpRenderer.BuildSections([Command("A")]);

        Assert.Single(sections);
    }

    [Fact]
    public void BuildSections_MultipleLeafCommands_ReturnsSingleSection()
    {
        var sections = HelpRenderer.BuildSections([Command("A"), Command("B"), Command("C")]);

        Assert.Single(sections);
    }

    [Fact]
    public void BuildSections_OnlySubMenuCommands_ReturnsSectionsPerSubMenu()
    {
        var sub1 = new SubMenuCommand("Alpha", [Command("A1")]);
        var sub2 = new SubMenuCommand("Beta", [Command("B1")]);

        var sections = HelpRenderer.BuildSections([sub1, sub2]);

        Assert.Equal(2, sections.Count);
    }

    [Fact]
    public void BuildSections_RootAndSubMenu_ReturnsTwoSections()
    {
        var sections = HelpRenderer.BuildSections([
            Command("Root"),
            new SubMenuCommand("Tools", [Command("Tool")])
        ]);

        Assert.Equal(2, sections.Count);
    }

    [Fact]
    public void BuildSections_RootSectionAppearsBefore_SubMenuSection()
    {
        var sections = HelpRenderer.BuildSections([
            Command("Root"),
            new SubMenuCommand("Tools", [Command("Tool")])
        ]);

        Assert.Equal("Main Menu", sections[0].Heading);
        Assert.Equal("Tools", sections[1].Heading);
    }

    // -------------------------------------------------------------------------
    // Entry count and content
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildSections_EntryCount_MatchesLeafCommandCount()
    {
        var sections = HelpRenderer.BuildSections([Command("A"), Command("B")]);

        Assert.Equal(2, sections[0].Entries.Count);
    }

    [Fact]
    public void BuildSections_SubMenuEntries_ExcludesNestedSubMenuNodes()
    {
        // Only the leaf is an entry — the inner SubMenuCommand is not
        var inner = new SubMenuCommand("Inner", [Command("Deep")]);
        var outer = new SubMenuCommand("Outer", [Command("Shallow"), inner]);

        var sections = HelpRenderer.BuildSections([outer]);

        Assert.Equal(2, sections.Count);
        Assert.Equal(1, sections[0].Entries.Count); // "Shallow" under "Outer"
        Assert.Equal(1, sections[1].Entries.Count); // "Deep" under "Inner"
    }

    [Fact]
    public void BuildSections_EntryTitle_MatchesCommandTitle()
    {
        var cmd = Command("My Command");

        var sections = HelpRenderer.BuildSections([cmd]);

        Assert.Equal("My Command", sections[0].Entries[0].Title);
    }

    [Fact]
    public void BuildSections_EntryHotkey_IsEmpty_WhenCommandHasNoRegisteredHotkey()
    {
        // NSubstitute mock has no type registration in CommandRegistry
        var sections = HelpRenderer.BuildSections([Command("X")]);

        Assert.Equal(string.Empty, sections[0].Entries[0].Hotkey);
    }

    [Fact]
    public void BuildSections_EntryDescription_IsNull_WhenCommandHasNoDescriptionAttribute()
    {
        // NSubstitute mock carries no [CommandDescription] attribute
        var sections = HelpRenderer.BuildSections([Command("X")]);

        Assert.Null(sections[0].Entries[0].Description);
    }

    // -------------------------------------------------------------------------
    // Nested SubMenuCommand
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildSections_NestedSubMenu_CreatesSectionForInnerLevel()
    {
        var leaf = Command("Deep");
        var inner = new SubMenuCommand("Inner", [leaf]);
        var outer = new SubMenuCommand("Outer", [inner]);

        var sections = HelpRenderer.BuildSections([outer]);

        Assert.Single(sections);
        Assert.Equal("Inner", sections[0].Heading);
    }

    [Fact]
    public void BuildSections_ThreeLevelNesting_ProducesOneSection()
    {
        var leaf = Command("Leaf");
        var l3 = new SubMenuCommand("L3", [leaf]);
        var l2 = new SubMenuCommand("L2", [l3]);
        var l1 = new SubMenuCommand("L1", [l2]);

        var sections = HelpRenderer.BuildSections([l1]);

        Assert.Single(sections);
        Assert.Equal("L3", sections[0].Heading);
    }

    [Fact]
    public void BuildSections_PreservesInsertionOrder_AcrossSections()
    {
        var sections = HelpRenderer.BuildSections([
            Command("Root1"),
            Command("Root2"),
            new SubMenuCommand("Sub", [Command("SubCmd")])
        ]);

        Assert.Equal("Root1", sections[0].Entries[0].Title);
        Assert.Equal("Root2", sections[0].Entries[1].Title);
        Assert.Equal("SubCmd", sections[1].Entries[0].Title);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static IMenuCommand Command(string title)
    {
        var cmd = Substitute.For<IMenuCommand>();
        cmd.Title.Returns(title);
        return cmd;
    }
}
