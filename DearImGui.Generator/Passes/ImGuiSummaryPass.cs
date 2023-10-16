using System.Collections.Immutable;
using DearGenerator.Passes;

namespace DearImGui.Generator.Passes;

public sealed class ImGuiSummaryPass : ImSummaryPass
{
    protected override ImmutableSortedSet<string> HeaderNames { get; } =
        new[]
            {
                "imgui.h"
            }
            .ToImmutableSortedSet();

    protected override string HeaderUrl { get; } = @$"https://github.com/ocornut/imgui/blob/{File.ReadAllText("../../../../../.git/modules/SubModLoader_DearImGui/modules/imgui/imgui/HEAD").Trim()}/imgui.h";
}