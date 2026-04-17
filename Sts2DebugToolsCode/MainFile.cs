using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;
using Sts2DebugTools.Sts2DebugToolsCode.Features;

namespace Sts2DebugTools.Sts2DebugToolsCode;

/// <summary>
/// Entry point for the STS2 Debug Tools mod.
///
/// Applies Harmony patches and registers a <see cref="RunManager.RunStarted"/>
/// listener that attaches the right-click-to-kill handler to the run scene each
/// time a new run begins.  The mod provides:
/// <list type="bullet">
///   <item><description>
///     <b>Right-click to kill</b> – right-click any primary enemy during combat
///     to instantly kill it (handled by <see cref="ClickToKillHandler"/>).
///   </description></item>
///   <item><description>
///     <b>Map Teleport</b> – click any room on the map to teleport there
///     (enabled via <see cref="MapDebugTravelPatch"/>).
///   </description></item>
/// </list>
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Sts2DebugTools";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var harmony = new Harmony(ModId);
        harmony.PatchAll(typeof(MainFile).Assembly);

        RunManager.Instance.RunStarted += OnRunStarted;

        Logger.Info("STS2 Debug Tools mod initialized.");
    }

    private static void OnRunStarted(RunState _)
    {
        ClickToKillHandler.Attach();
    }
}
