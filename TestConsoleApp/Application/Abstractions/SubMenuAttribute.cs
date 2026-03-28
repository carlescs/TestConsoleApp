namespace TestConsoleApp.Application.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SubMenuAttribute(string name) : Attribute
{
    public string Name => name;
}
