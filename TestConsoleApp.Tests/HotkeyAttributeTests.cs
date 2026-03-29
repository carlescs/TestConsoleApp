using System.Reflection;
using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Tests;

public sealed class HotkeyAttributeTests
{
    [Fact]
    public void Key_ReturnsCharPassedToConstructor()
    {
        var attr = new HotkeyAttribute('H');

        Assert.Equal('H', attr.Key);
    }

    [Fact]
    public void Modifiers_DefaultsToNone_WhenNotProvided()
    {
        var attr = new HotkeyAttribute('H');

        Assert.Equal(default, attr.Modifiers);
    }

    [Fact]
    public void Modifiers_ReturnsControlModifier_WhenProvided()
    {
        var attr = new HotkeyAttribute('H', ConsoleModifiers.Control);

        Assert.Equal(ConsoleModifiers.Control, attr.Modifiers);
    }

    [Fact]
    public void Modifiers_ReturnsAltModifier_WhenProvided()
    {
        var attr = new HotkeyAttribute('H', ConsoleModifiers.Alt);

        Assert.Equal(ConsoleModifiers.Alt, attr.Modifiers);
    }

    [Theory]
    [InlineData('A')]
    [InlineData('Z')]
    [InlineData('1')]
    public void Key_CanBeAnyChar(char key)
    {
        var attr = new HotkeyAttribute(key);

        Assert.Equal(key, attr.Key);
    }

    [Fact]
    public void AttributeTargets_ClassOnly()
    {
        var usage = (AttributeUsageAttribute)typeof(HotkeyAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
    }

    [Fact]
    public void AttributeUsage_DoesNotAllowMultiple()
    {
        var usage = (AttributeUsageAttribute)typeof(HotkeyAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.AllowMultiple);
    }

    [Fact]
    public void AttributeUsage_IsNotInherited()
    {
        var usage = (AttributeUsageAttribute)typeof(HotkeyAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.Inherited);
    }
}
