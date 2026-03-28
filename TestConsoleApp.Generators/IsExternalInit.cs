// Required polyfill so that 'record struct' (C# 10+) compiles when targeting netstandard2.0.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
