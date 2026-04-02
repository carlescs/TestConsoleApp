using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Tests.Application.Abstractions;

public sealed class CommandDescriptionAttributeTests
{
    [Fact]
    public void Description_ReturnsTextPassedToConstructor()
    {
        var attr = new CommandDescriptionAttribute("Does something useful.");

        Assert.Equal("Does something useful.", attr.Description);
    }

    [Theory]
    [InlineData("Short.")]
    [InlineData("A longer description with punctuation, commas, and more text.")]
    public void Description_CanBeAnyNonEmptyString(string text)
    {
        var attr = new CommandDescriptionAttribute(text);

        Assert.Equal(text, attr.Description);
    }

    [Fact]
    public void AttributeTargets_ClassOnly()
    {
        var usage = (AttributeUsageAttribute)typeof(CommandDescriptionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
    }

    [Fact]
    public void AttributeUsage_DoesNotAllowMultiple()
    {
        var usage = (AttributeUsageAttribute)typeof(CommandDescriptionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.AllowMultiple);
    }

    [Fact]
    public void AttributeUsage_IsNotInherited()
    {
        var usage = (AttributeUsageAttribute)typeof(CommandDescriptionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.Inherited);
    }
}
