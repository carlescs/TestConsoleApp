// Required polyfill so that 'record struct' (C# 10+) compiles when targeting netstandard2.0.
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices;

// ReSharper disable once UnusedType.Global
internal static class IsExternalInit { }
