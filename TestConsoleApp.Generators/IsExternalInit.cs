// Required polyfill so that 'record struct' (C# 10+) compiles when targeting netstandard2.0.
namespace TestConsoleApp.Generators
{
    internal static class IsExternalInit { }
}
