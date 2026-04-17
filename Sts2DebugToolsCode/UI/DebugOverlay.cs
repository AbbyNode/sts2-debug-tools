using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using Sts2DebugTools.Sts2DebugToolsCode.Features;

namespace Sts2DebugTools.Sts2DebugToolsCode.UI;

/// <summary>
/// Floating debug panel injected into the run scene.
///
/// <list type="bullet">
///   <item><description>
///     <b>Win Battle</b> button – visible and active only while combat is in
///     progress.  Calls <see cref="InstantWinHelper.Execute"/> to instantly
///     kill all primary enemies.
///   </description></item>
/// </list>
///
/// Map teleport is handled separately by <see cref="Features.MapDebugTravelPatch"/>,
/// which enables the game's built-in debug travel on the map screen so the
/// player can click any room directly.
///
/// One instance is created per run via <see cref="CreateAndAttach"/> and
/// destroyed when a new run starts (or via <see cref="Detach"/>).
/// </summary>
internal static class DebugOverlay
{
    private const string OverlayNodeName = "DebugToolsOverlay";
    private const int OverlayCanvasLayer = 100;

    private static CanvasLayer? _canvasLayer;
    private static Control? _root;
    private static Button? _winBattleButton;

    // -----------------------------------------------------------------------
    //  Public lifecycle API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a new overlay and attaches it to <see cref="NRun.Instance"/>.
    /// Any previously-attached overlay is removed first.
    /// </summary>
    internal static void CreateAndAttach()
    {
        Detach();

        var runRoot = NRun.Instance;
        if (runRoot == null)
        {
            MainFile.Logger.Warn("[DebugTools] NRun.Instance is null; overlay cannot be shown yet.");
            return;
        }

        // ------------------------------------------------------------------
        // CanvasLayer ensures the overlay renders above all game UI.
        // ------------------------------------------------------------------
        _canvasLayer = new CanvasLayer
        {
            Name = OverlayNodeName,
            Layer = OverlayCanvasLayer,
        };

        // ------------------------------------------------------------------
        // Root control positioned in the top-left area.
        // ------------------------------------------------------------------
        _root = new Control
        {
            Position = new Vector2(10f, 200f),
        };
        _canvasLayer.AddChild(_root);

        // ------------------------------------------------------------------
        // Win Battle button – only shown during combat.
        // ------------------------------------------------------------------
        _winBattleButton = new Button
        {
            Text = "Win Battle",
            CustomMinimumSize = new Vector2(180f, 44f),
            Visible = false,
        };
        _winBattleButton.AddThemeFontSizeOverride("font_size", 20);
        _winBattleButton.Pressed += OnWinBattlePressed;
        _root.AddChild(_winBattleButton);

        // Subscribe to combat events so the button tracks the game state.
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
        CombatManager.Instance.CombatEnded += OnCombatEnded;

        runRoot.CallDeferred(Node.MethodName.AddChild, _canvasLayer);
        MainFile.Logger.Info("[DebugTools] Debug overlay attached to NRun.");
    }

    /// <summary>Removes the overlay from the scene tree and unsubscribes events.</summary>
    internal static void Detach()
    {
        if (_canvasLayer != null && GodotObject.IsInstanceValid(_canvasLayer))
        {
            CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
            CombatManager.Instance.CombatEnded -= OnCombatEnded;
            _canvasLayer.QueueFree();
        }

        _canvasLayer = null;
        _root = null;
        _winBattleButton = null;
    }

    // -----------------------------------------------------------------------
    //  Combat event handlers – update button visibility
    // -----------------------------------------------------------------------

    private static void OnCombatSetUp(CombatState _)
    {
        SetButtonVisible(_winBattleButton, true);
    }

    private static void OnCombatEnded(CombatRoom _)
    {
        SetButtonVisible(_winBattleButton, false);
    }

    // -----------------------------------------------------------------------
    //  Button press handlers
    // -----------------------------------------------------------------------

    private static void OnWinBattlePressed()
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            MainFile.Logger.Warn("[DebugTools] Win Battle pressed but not in combat.");
            return;
        }

        MainFile.Logger.Info("[DebugTools] Win Battle pressed – executing instant win.");
        InstantWinHelper.Execute();
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static void SetButtonVisible(Button? button, bool visible)
    {
        if (button != null && GodotObject.IsInstanceValid(button))
            button.Visible = visible;
    }
}
