﻿using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CppSharp;

namespace im.NET.Generator;

public abstract class ConsoleGenerator
{
    protected ConsoleGenerator(string moduleName, string? directory = null)
    {
        ModuleName = moduleName;
        ModulePath = Path.Combine(directory ?? Environment.CurrentDirectory, Path.ChangeExtension(ModuleName, "cs"));
        ModuleText = File.Exists(ModulePath) ? File.ReadAllText(ModulePath) : string.Empty;
    }

    /// <summary>
    ///     The set of namespaces used.
    /// </summary>
    public abstract ImmutableSortedSet<string> Namespaces { get; }

    /// <summary>
    ///     The set of classes to be renamed.
    /// </summary>
    public abstract ImmutableSortedSet<KeyValuePair<string, string>> Classes { get; }

    /// <summary>
    ///     The set of aliases to be renamed.
    /// </summary>
    public abstract ImmutableSortedSet<Type> Aliases { get; }

    private string ModuleName { get; }

    private string ModulePath { get; }

    private string ModuleText { get; }

    public static void Run(ILibrary library, ConsoleGenerator generator)
    {
        if (Debugger.IsAttached) // cleanup garbage
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Clear();
        }

        ConsoleDriver.Run(library);

        generator.Process();

        Console.WriteLine("Generation finished.");
    }

    private void Process()
    {
        var text = File.ReadAllText(ModulePath);

        Process(ref text);

        Write(text);
    }

    protected virtual void Process(ref string text)
    {
        // apply module-wide changes stuff first

        ProcessNamespaces(ref text, Namespaces);
        ProcessAliases(ref text, Aliases);
        ProcessClasses(ref text, Classes);

        // apply fine-grained stuff

        ProcessVectors(ref text);
        ProcessPointers(ref text);
        ProcessSummaries(ref text);
        ProcessVisibility(ref text);
    }

    private static void ProcessAliases(ref string input, ImmutableSortedSet<Type> aliases)
    {
        foreach (var item in aliases)
        {
            var name = item.Name;

            // remove using alias

            input = Regex.Replace(input,
                $@"^using __{name} = .*;\r?$\n",
                string.Empty,
                RegexOptions.Multiline
            );

            // remove double underscore prefix

            input = input.Replace($"__{name}", name);
        }
    }

    private static void ProcessClasses(ref string input, ImmutableSortedSet<KeyValuePair<string, string>> classes)
    {
        // rename classes and references to their internals

        foreach (var (key, val) in classes)
        {
            input = input.Replace($"class {key}", $"class {val}");
            input = input.Replace($"{key}()",     $"{val}()");
            input = input.Replace($"{key}._",     $"{val}._");
        }
    }

    private static void ProcessNamespaces(ref string input, ImmutableSortedSet<string> namespaces)
    {
        // simplify fully qualified names

        foreach (var item in namespaces.Reverse())
        {
            input = input.Replace($"global::{item}.", string.Empty);
        }
    }

    private static void ProcessPointers(ref string input)
    {
        // hide pointers to internal classes

        input = input.Replace(
            "public IntPtr __Instance { get; protected set; }",
            "internal IntPtr __Instance { get; set; }"
        );
    }

    private static void ProcessSummaries(ref string input)
    {
        input = input.Replace("// <summary>", "/// <summary>");

        // add few inherit doc to reduce CS1591 warnings count

        input = Regex.Replace(
            input,
            @"^(\s+)(public void Dispose\(\))",
            @"$1/// <inheritdoc />$1$2",
            RegexOptions.Multiline
        );

        input = Regex.Replace(
            input,
            @"^(\s+)(~\w+\(\))",
            @"$1/// <inheritdoc />$1$2",
            RegexOptions.Multiline
        );
    }

    private static void ProcessVectors(ref string input)
    {
        // type maps aren't enough to pass vectors directly

        input = Regex.Replace(input,
            @"new ImVec(\d)\.__Internal\(\)",
            @"new Vector$1()",
            RegexOptions.Multiline
        );

        // make vectors as obsolete so it's crystal clear

        input = Regex.Replace(input,
            @"public (unsafe partial struct ImVec\d)",
            @"[Obsolete(null, true)] internal $1",
            RegexOptions.Multiline
        );

        // setting value type in type maps doesn't work

        input = Regex.Replace(input,
            @"__element is null \? new .*Vector\d.*;",
            @"__element;",
            RegexOptions.Multiline
        );
    }

    private static void ProcessVisibility(ref string input)
    {
        // hide internal structs and vectors

        input = Regex.Replace(input,
            @"public ((?:unsafe )?partial struct (?:__Internal|ImVec\d))",
            @"internal $1",
            RegexOptions.Multiline
        );

        // hide protected members to remove more CS1591

        input = Regex.Replace(input,
            @"(internal\s+)*protected",
            "private protected",
            RegexOptions.Multiline
        );
    }

    private void Write(string text)
    {
        File.WriteAllText(ModulePath, text);

        if (ModuleText == string.Empty)
        {
            return;
        }

        if (ModuleText == text)
        {
            return;
        }

        // create a backup history so we can diff them
        // imgui-backup.cs -> imgui-backup-date.cs
        // imgui.cs -> imgui-backup.cs

        var backupDirectory = Path.GetDirectoryName(ModulePath) ?? string.Empty;
        var backupExtension = Path.GetExtension(ModulePath);
        var backupPath = Path.Combine(backupDirectory, Path.ChangeExtension($"{ModuleName}-backup", backupExtension));

        if (File.Exists(backupPath))
        {
            var backupTime = File.GetLastWriteTimeUtc(backupPath);
            var backupName = backupTime.ToString("O").Replace(':', '-').Replace('.', '-');
            var backupFile = Path.GetFileNameWithoutExtension(backupPath);
            var backupDest = Path.Combine(backupDirectory, Path.ChangeExtension($"{backupFile}-{backupName}", backupExtension));

            File.Copy(backupPath, backupDest, true);
        }

        File.WriteAllText(backupPath, ModuleText);
    }

    protected static ImmutableSortedSet<string> GetDefaultNamespaces()
    {
        return new SortedSet<string>
            {
                "imgui.NET",
                "System",
                "System.Collections.Concurrent",
                "System.Numerics",
                "System.Runtime.CompilerServices",
                "System.Runtime.InteropServices",
                "System.Security",
                "System.Text"
            }
            .ToImmutableSortedSet();
    }

    protected static ImmutableSortedSet<Type> GetDefaultAliases()
    {
        return new SortedSet<Type>(TypeNameComparer.Instance)
            {
                typeof(CallingConvention),
                typeof(IntPtr)
            }
            .ToImmutableSortedSet(TypeNameComparer.Instance);
    }
}