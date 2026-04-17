using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
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
///   <item><description>
///     <b>Teleport to Room</b> button – visible and active only when a run
///     is active and no combat is in progress.  Opens
///     <see cref="MapTeleportDialog"/> so the player can jump to any room on
///     the current act's map.
///   </description></item>
/// </list>
///
/// One instance is created per run via <see cref="CreateAndAttach"/> and
/// destroyed when a new run starts (or via <see cref="Detach"/>).
/// </summary>
internal static class DebugOverlay
{
    private const string OverlayNodeName = "DebugToolsOverlay";

    private static Control? _root;
    private static Button? _winBattleButton;
    private static Button? _teleportButton;

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
        // Root control – anchored to the top-left corner of the screen.
        // ------------------------------------------------------------------
        _root = new Control
        {
            Name = OverlayNodeName,
            Position = new Vector2(10f, 200f),
            ZIndex = 100,
        };

        // ------------------------------------------------------------------
        // Vertical layout for the two buttons.
        // ------------------------------------------------------------------
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        _root.AddChild(vbox);

        _winBattleButton = new Button
        {
            Text = "Win Battle",
            CustomMinimumSize = new Vector2(180f, 44f),
            Visible = false,
        };
        _winBattleButton.AddThemeFontSizeOverride("font_size", 20);
        _winBattleButton.Pressed += OnWinBattlePressed;
        vbox.AddChild(_winBattleButton);

        _teleportButton = new Button
        {
            Text = "Teleport to Room",
            CustomMinimumSize = new Vector2(180f, 44f),
            Visible = true,
        };
        _teleportButton.AddThemeFontSizeOverride("font_size", 20);
        _teleportButton.Pressed += OnTeleportPressed;
        vbox.AddChild(_teleportButton);

        // Subscribe to combat events so the buttons track the game state.
        CombatManager.Instance.CombatSetUp += OnCombatSetUp;
        CombatManager.Instance.CombatEnded += OnCombatEnded;

        runRoot.CallDeferred(Node.MethodName.AddChild, _root);
        MainFile.Logger.Info("[DebugTools] Debug overlay attached to NRun.");
    }

    /// <summary>Removes the overlay from the scene tree and unsubscribes events.</summary>
    internal static void Detach()
    {
        if (_root != null && GodotObject.IsInstanceValid(_root))
        {
            CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
            CombatManager.Instance.CombatEnded -= OnCombatEnded;
            _root.QueueFree();
        }

        _root = null;
        _winBattleButton = null;
        _teleportButton = null;
    }

    // -----------------------------------------------------------------------
    //  Combat event handlers – update button visibility
    // -----------------------------------------------------------------------

    private static void OnCombatSetUp(CombatState _)
    {
        SetButtonVisible(_winBattleButton, true);
        SetButtonVisible(_teleportButton, false);
    }

    private static void OnCombatEnded(CombatRoom _)
    {
        SetButtonVisible(_winBattleButton, false);
        SetButtonVisible(_teleportButton, true);
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

    private static void OnTeleportPressed()
    {
        if (RunManager.Instance.DebugOnlyGetState() == null)
        {
            MainFile.Logger.Warn("[DebugTools] Teleport pressed but no active run.");
            return;
        }

        if (CombatManager.Instance.IsInProgress)
        {
            MainFile.Logger.Warn("[DebugTools] Teleport pressed during combat – ignored.");
            return;
        }

        if (_root == null || !GodotObject.IsInstanceValid(_root))
        {
            MainFile.Logger.Warn("[DebugTools] Teleport pressed but overlay root is invalid.");
            return;
        }

        MapTeleportDialog.Show(_root);
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
