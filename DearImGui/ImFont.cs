using JetBrains.Annotations;

namespace DearImGui;

/// <summary>
///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2850"/>
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
partial class ImFont
{
    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2882"/>
    /// </summary>
    public string DebugName => ConfigData?.Name ?? "<unknown>";

    /// <summary>
    ///     <see href="https://github.com/ocornut/imgui/blob/9aae45eb4a05a5a1f96be1ef37eb503a12ceb889/imgui.h#L2881"/>
    /// </summary>
    public bool IsLoaded => ContainerAtlas != null;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(FontSize)}: {FontSize}, {nameof(Scale)}: {Scale}";
    }
}