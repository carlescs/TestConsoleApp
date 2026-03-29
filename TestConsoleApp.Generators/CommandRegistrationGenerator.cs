using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace TestConsoleApp.Generators;

/// <summary>
/// An incremental Roslyn source generator that discovers all concrete, public,
/// parameterless-constructor <c>IMenuCommand</c> implementations in the compilation,
/// builds a nested menu tree from their <c>SubMenuAttribute</c> paths, and emits a
/// <c>CommandRegistrationInitializer</c> class whose <c>[ModuleInitializer]</c> method
/// registers every command with <c>CommandRegistry</c> at application startup.
/// </summary>
[Generator]
public sealed class CommandRegistrationGenerator : IIncrementalGenerator
{
    private const string InterfaceName = "IMenuCommand";
    private const string InterfaceNamespace = "TestConsoleApp.Application.Abstractions";
    private const string SubMenuAttributeName = "SubMenuAttribute";
    private const string HotkeyAttributeName = "HotkeyAttribute";
    private const string SubMenuCommandNamespace = "TestConsoleApp.Presentation.Menus";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandModels = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) => node is ClassDeclarationSyntax { BaseList: not null },
                static (ctx, _) => GetCommandModel(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!.Value)
            .Collect();

        context.RegisterSourceOutput(commandModels, static (ctx, models) => Emit(ctx, models));
    }

    /// <summary>
    /// Tries to extract a <see cref="CommandModel"/> from the given syntax node.
    /// </summary>
    /// <param name="context">The generator syntax context providing semantic information.</param>
    /// <returns>
    /// A <see cref="CommandModel"/> when the node is a registerable command class;
    /// otherwise <see langword="null"/>.
    /// </returns>
    private static CommandModel? GetCommandModel(GeneratorSyntaxContext context)
    {
        if (context.SemanticModel.GetDeclaredSymbol(context.Node) is not INamedTypeSymbol symbol || symbol.IsAbstract || symbol.IsGenericType)
            return null;

        bool implementsInterface = symbol.AllInterfaces.Any(i =>
            i.Name == InterfaceName &&
            i.ContainingNamespace.ToDisplayString() == InterfaceNamespace);

        if (!implementsInterface)
            return null;

        bool hasParameterlessConstructor = symbol.InstanceConstructors.Any(contructor =>
            (contructor.Parameters.IsEmpty || contructor.Parameters.All(p => p.HasExplicitDefaultValue)) &&
            contructor.DeclaredAccessibility == Accessibility.Public);

        if (!hasParameterlessConstructor)
            return null;

        var subMenuAttr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == SubMenuAttributeName &&
            a.AttributeClass.ContainingNamespace.ToDisplayString() == InterfaceNamespace);

        var subMenuPath = ImmutableArray<string>.Empty;
        if (subMenuAttr is { ConstructorArguments.Length: > 0 })
        {
            var arg = subMenuAttr.ConstructorArguments[0];
            if (arg.Kind == TypedConstantKind.Array)
            {
                var builder = ImmutableArray.CreateBuilder<string>();
                foreach (var v in arg.Values)
                {
                    if (v.Value is string { Length: > 0 } s)
                        builder.Add(s);
                }
                subMenuPath = builder.ToImmutable();
            }
        }

        var hotkeyAttr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == HotkeyAttributeName &&
            a.AttributeClass.ContainingNamespace.ToDisplayString() == InterfaceNamespace);

        (char Key, ConsoleModifiers Modifiers)? hotkey = null;
        if (hotkeyAttr is not { ConstructorArguments.Length: > 0 })
        {
            return new CommandModel(
                symbol.Name,
                symbol.ContainingNamespace.ToDisplayString(),
                subMenuPath,
                hotkey);
        }

        var keyArg = hotkeyAttr.ConstructorArguments[0];
        if (keyArg is not { Kind: TypedConstantKind.Primitive, Value: char c })
        {
            return new CommandModel(
                symbol.Name,
                symbol.ContainingNamespace.ToDisplayString(),
                subMenuPath,
                hotkey);
        }

        var mods = (ConsoleModifiers)0;
        if (hotkeyAttr.ConstructorArguments.Length > 1)
        {
            var modArg = hotkeyAttr.ConstructorArguments[1];
            if (modArg is { Kind: TypedConstantKind.Enum, Value: not null })
                mods = (ConsoleModifiers)Convert.ToInt32(modArg.Value);
        }
        hotkey = (c, mods);

        return new CommandModel(
            symbol.Name,
            symbol.ContainingNamespace.ToDisplayString(),
            subMenuPath,
            hotkey);
    }

    /// <summary>Emits the registration source file from the collected command models.</summary>
    /// <param name="context">The source production context used to add the generated file.</param>
    /// <param name="models">All command models collected from the current compilation.</param>
    private static void Emit(SourceProductionContext context, ImmutableArray<CommandModel> models)
    {
        if (models.IsEmpty)
            return;

        bool hasGrouped = models.Any(m => !m.SubMenuPath.IsEmpty);
        var modelsWithHotkeys = models.Where(m => m.Hotkey.HasValue).OrderBy(m => m.TypeName).ToList();
        var hotkeyTypeNames = new HashSet<string>(modelsWithHotkeys.Select(m => m.TypeName));

        var root = new MenuTreeNode();
        foreach (var model in models)
        {
            var node = root;
            foreach (string? segment in model.SubMenuPath)
            {
                if (!node.Children.TryGetValue(segment, out var child))
                {
                    child = new MenuTreeNode();
                    node.Children[segment] = child;
                }
                node = child;
            }
            node.CommandTypeNames.Add(model.TypeName);
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine($"using {InterfaceNamespace};");

        if (modelsWithHotkeys.Count > 0)
            sb.AppendLine("using System;"); // ConsoleModifiers

        if (hasGrouped)
            sb.AppendLine($"using {SubMenuCommandNamespace};");

        foreach (string? ns in models
            .Select(m => m.Namespace)
            .Where(ns => ns != InterfaceNamespace)
            .Distinct()
            .OrderBy(ns => ns))
            sb.AppendLine($"using {ns};");

        sb.AppendLine();
        sb.AppendLine("namespace TestConsoleApp.Generated;");
        sb.AppendLine();
        sb.AppendLine("internal static class CommandRegistrationInitializer");
        sb.AppendLine("{");
        sb.AppendLine("    [ModuleInitializer]");
        sb.AppendLine("    internal static void RegisterAll()");
        sb.AppendLine("    {");

        // Variable declarations for commands that have hotkeys (needed to pass instances)
        foreach (var model in modelsWithHotkeys)
            sb.AppendLine($"        var {GetVarName(model.TypeName)} = new {model.TypeName}();");

        if (modelsWithHotkeys.Count > 0)
            sb.AppendLine();

        // Register calls – use variables where available
        foreach (string? typeName in root.CommandTypeNames.OrderBy(t => t))
        {
            string expr = hotkeyTypeNames.Contains(typeName) ? GetVarName(typeName) : $"new {typeName}()";
            sb.AppendLine($"        CommandRegistry.Register({expr});");
        }

        foreach (var kvp in root.Children)
            sb.AppendLine($"        CommandRegistry.Register({EmitSubMenuNode(kvp.Key, kvp.Value, hotkeyTypeNames)});");

        // RegisterHotkey calls
        if (modelsWithHotkeys.Count > 0)
            sb.AppendLine();

        foreach (var model in modelsWithHotkeys)
        {
            (char key, ConsoleModifiers mods) = model.Hotkey!.Value;
            sb.AppendLine($"        CommandRegistry.RegisterHotkey('{key}', {FormatModifiers(mods)}, {GetVarName(model.TypeName)});");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("CommandRegistrationInitializer.g.cs", sb.ToString());
    }

    /// <summary>
    /// Recursively renders a <c>new SubMenuCommand(...)</c> expression for a tree node
    /// and all of its descendants.
    /// </summary>
    /// <param name="name">The display name of this submenu level.</param>
    /// <param name="node">The tree node containing child commands and nested submenus.</param>
    /// <param name="hotkeyTypeNames">A set of type names that have associated hotkeys.</param>
    /// <returns>A C# expression string for constructing the submenu.</returns>
    private static string EmitSubMenuNode(string name, MenuTreeNode node, HashSet<string> hotkeyTypeNames)
    {
        var items = new List<string>();
        foreach (string? typeName in node.CommandTypeNames.OrderBy(t => t))
        {
            string expr = hotkeyTypeNames.Contains(typeName) ? GetVarName(typeName) : $"new {typeName}()";
            items.Add(expr);
        }
        foreach (var kvp in node.Children)
            items.Add(EmitSubMenuNode(kvp.Key, kvp.Value, hotkeyTypeNames));
        return $"new SubMenuCommand(\"{EscapeString(name)}\", [{string.Join(", ", items)}])";
    }

    /// <summary>Derives a camelCase local variable name from a type name (e.g. FooCommand → fooCommand).</summary>
    private static string GetVarName(string typeName) =>
        char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);

    /// <summary>Formats a <see cref="System.ConsoleModifiers"/> value as a C# expression.</summary>
    private static string FormatModifiers(ConsoleModifiers modifiers)
    {
        if ((int)modifiers == 0) return "(ConsoleModifiers)0";
        var parts = new List<string>();
        if ((modifiers & ConsoleModifiers.Alt) != 0) parts.Add("ConsoleModifiers.Alt");
        if ((modifiers & ConsoleModifiers.Shift) != 0) parts.Add("ConsoleModifiers.Shift");
        if ((modifiers & ConsoleModifiers.Control) != 0) parts.Add("ConsoleModifiers.Control");
        return string.Join(" | ", parts);
    }

    /// <summary>Escapes backslashes and double quotes for use inside a C# string literal.</summary>
    /// <param name="value">The raw string value to escape.</param>
    /// <returns>The escaped string.</returns>
    private static string EscapeString(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>Holds the data extracted from a single command class declaration.</summary>
    private readonly record struct CommandModel(string TypeName, string Namespace, ImmutableArray<string> SubMenuPath, (char Key, ConsoleModifiers Modifiers)? Hotkey);

    /// <summary>
    /// An internal tree node used to accumulate the nested submenu structure
    /// before the source file is emitted.
    /// </summary>
    private sealed class MenuTreeNode
    {
        /// <summary>Gets the type names of commands registered directly at this node.</summary>
        public List<string> CommandTypeNames { get; } = [];

        /// <summary>Gets the named child nodes representing deeper menu levels.</summary>
        public SortedDictionary<string, MenuTreeNode> Children { get; } = new(StringComparer.Ordinal);
    }
}
