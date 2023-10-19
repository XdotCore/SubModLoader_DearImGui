using System.Numerics;
using JetBrains.Annotations;

namespace DearImGui;

/// <summary>
///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2633/"/>
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
partial struct ImDrawData
{
    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2640"/>
    /// </summary>
    public Vector2 DisplayPos => __Instance.DisplayPos;

    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2641."/>
    /// </summary>
    public Vector2 DisplaySize => __Instance.DisplaySize;

    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2642."/>
    /// </summary>
    public Vector2 FramebufferScale => __Instance.FramebufferScale;

    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2639"/>
    /// </summary>
    public unsafe int GetCmdLists([System.Diagnostics.CodeAnalysis.NotNull] ref ImDrawList[]? lists)
    {
        var count = CmdListsCount;

        if (lists is null || lists.Length < count)
        {
            Array.Resize(ref lists, count);
        }

        var input = (IntPtr*)__Instance.CmdLists.Data;

        for (var i = 0; i < count; i++)
        {
            lists[i] = ImDrawList.__CreateInstance(input[i]);
        }

        return count;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Valid)}: {Valid}, {nameof(TotalIdxCount)}: {TotalIdxCount}, {nameof(TotalVtxCount)}: {TotalVtxCount}, {nameof(CmdListsCount)}: {CmdListsCount}";
    }
}