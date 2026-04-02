using TestConsoleApp.Application.Abstractions;

namespace TestConsoleApp.Tests.Application.Abstractions;

public sealed class SubMenuAttributeTests
{
    [Fact]
    public void Path_ReturnsSingleSegment_WhenOneSegmentProvided()
    {
        var attr = new SubMenuAttribute("Tools");

        Assert.Equal(["Tools"], attr.Path);
    }

    [Fact]
    public void Path_ReturnsMultipleSegments_WhenMultipleProvided()
    {
        var attr = new SubMenuAttribute("Tools", "Advanced");

        Assert.Equal(["Tools", "Advanced"], attr.Path);
    }

    [Fact]
    public void Path_IsEmpty_WhenNoSegmentsProvided()
    {
        var attr = new SubMenuAttribute();

        Assert.Empty(attr.Path);
    }

    [Fact]
    public void AttributeTargets_ClassOnly()
    {
        var usage = (AttributeUsageAttribute)typeof(SubMenuAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.Equal(AttributeTargets.Class, usage.ValidOn);
    }

    [Fact]
    public void AttributeUsage_DoesNotAllowMultiple()
    {
        var usage = (AttributeUsageAttribute)typeof(SubMenuAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.AllowMultiple);
    }

    [Fact]
    public void AttributeUsage_IsNotInherited()
    {
        var usage = (AttributeUsageAttribute)typeof(SubMenuAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)[0];

        Assert.False(usage.Inherited);
    }
}
