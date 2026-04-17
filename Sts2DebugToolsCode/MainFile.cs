using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;
using Sts2DebugTools.Sts2DebugToolsCode.UI;

namespace Sts2DebugTools.Sts2DebugToolsCode;

/// <summary>
/// Entry point for the STS2 Debug Tools mod.
///
/// Registers a <see cref="RunManager.RunStarted"/> listener that attaches
/// the <see cref="DebugOverlay"/> to the run scene each time a new run
/// begins.  The overlay provides:
/// <list type="bullet">
///   <item><description>
///     <b>Win Battle</b> – instantly wins the current combat.
///   </description></item>
///   <item><description>
///     <b>Teleport to Room</b> – opens a dialog to jump to any room on the
///     current act's map.
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
        RunManager.Instance.RunStarted += OnRunStarted;

        Logger.Info("STS2 Debug Tools mod initialized.");
    }

    private static void OnRunStarted(RunState _)
    {
        DebugOverlay.CreateAndAttach();
    }
}
