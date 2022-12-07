﻿using System.Collections.Immutable;
using System.Text.RegularExpressions;
using im.NET.Generator;

namespace implot.NET.Generator;

internal sealed class ConsoleGeneratorImPlot : ConsoleGenerator
{
    public ConsoleGeneratorImPlot(string moduleName, string? directory = null)
        : base(moduleName, directory)
    {
        Namespaces = GetDefaultNamespaces().Add("implot.NET");

        Classes = new SortedSet<KeyValuePair<string, string>>
            {
                new("implot", "ImPlot")
            }
            .ToImmutableSortedSet();

        Aliases = GetDefaultAliases();
    }

    public override ImmutableSortedSet<string> Namespaces { get; }

    public override ImmutableSortedSet<KeyValuePair<string, string>> Classes { get; }

    public override ImmutableSortedSet<Type> Aliases { get; }

    protected override void Process(ref string input)
    {
        base.Process(ref input);

        // fix enumeration syntax for negative values

        input = Regex.Replace(input,
            @"(?<!(?:static|///\s*<summary>).*)(Colormap.*)(-1)",
            "$1(ImPlotColormap)($2)",
            RegexOptions.Multiline
        );

        // fix this wrong reference to internal pointer

        input = input.Replace(
            "ImPlotPoint.__Internal ImPlotGetter",
            "ImPlotPoint ImPlotGetter"
        );

        // stride in generic function still have wrong default value of T

        input = Regex.Replace(input,
            @"(public\s+static\s+\w+\s+Plot.*?\s)(ref\s)?(\w+)(\*?\s)(values|xs)(.*\s+stride\s+=\s+sizeof\()T(\)\))",
            @"$1$2$3$4$5$6$3$7",
            RegexOptions.Multiline
        );
    }
}