using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Sts2DebugTools.Sts2DebugToolsCode.Features;

/// <summary>
/// Enables right-clicking an enemy during combat to instantly kill it.
///
/// An invisible <see cref="InputListenerNode"/> is attached to <see cref="NRun"/>
/// for each run.  When a right mouse button press is detected while combat is in
/// progress, the code searches the scene tree for an <see cref="NCreature"/> node
/// whose <c>_isCreatureHovered</c> field is <see langword="true"/>, then kills
/// the underlying primary enemy via <see cref="InstantWinHelper.KillSingle"/>.
/// </summary>
internal static class ClickToKillHandler
{
    private const string ListenerNodeName = "DebugToolsClickToKill";

    /// <summary>Cached handle for the private <c>NCreature._isCreatureHovered</c> field.</summary>
    private static readonly FieldInfo? IsCreatureHoveredField =
        AccessTools.Field(typeof(NCreature), "_isCreatureHovered");

    /// <summary>Cached handle for the <c>NCreature._creature</c> backing field.</summary>
    private static readonly FieldInfo? NCreatureField =
        AccessTools.Field(typeof(NCreature), "_creature");

    private static InputListenerNode? _listener;

    // -----------------------------------------------------------------------
    //  Lifecycle
    // -----------------------------------------------------------------------

    /// <summary>
    /// Attaches the invisible input listener to <see cref="NRun.Instance"/>.
    /// Any previously-attached listener is removed first.
    /// </summary>
    internal static void Attach()
    {
        Detach();

        if (IsCreatureHoveredField == null)
        {
            MainFile.Logger.Warn("[DebugTools] Could not resolve NCreature._isCreatureHovered; right-click-to-kill disabled.");
            return;
        }

        if (NCreatureField == null)
        {
            MainFile.Logger.Warn("[DebugTools] Could not resolve NCreature._creature; right-click-to-kill disabled.");
            return;
        }

        var runRoot = NRun.Instance;
        if (runRoot == null)
        {
            MainFile.Logger.Warn("[DebugTools] NRun.Instance is null; right-click-to-kill cannot be attached.");
            return;
        }

        _listener = new InputListenerNode { Name = ListenerNodeName };
        runRoot.CallDeferred(Node.MethodName.AddChild, _listener);
        MainFile.Logger.Info("[DebugTools] Right-click-to-kill handler attached.");
    }

    /// <summary>Removes the input listener from the scene tree.</summary>
    internal static void Detach()
    {
        if (_listener != null && GodotObject.IsInstanceValid(_listener))
            _listener.QueueFree();

        _listener = null;
    }

    // -----------------------------------------------------------------------
    //  Kill logic
    // -----------------------------------------------------------------------

    /// <summary>
    /// Finds the currently-hovered primary enemy and kills it instantly.
    /// Safe to call at any time; is a no-op outside of active combat or when
    /// no primary enemy is hovered.
    /// </summary>
    internal static void TryKillHoveredEnemy()
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        var runRoot = NRun.Instance;
        if (runRoot == null)
            return;

        foreach (var node in FindAllDescendants(runRoot))
        {
            if (node is not NCreature nc)
                continue;

            if (IsCreatureHoveredField!.GetValue(nc) is not true)
                continue;

            var creature = NCreatureField!.GetValue(nc) as Creature;
            if (creature == null || !creature.IsPrimaryEnemy || !creature.IsAlive)
                continue;

            InstantWinHelper.KillSingle(creature);
            return;
        }
    }

    // -----------------------------------------------------------------------
    //  Scene-tree helpers
    // -----------------------------------------------------------------------

    private static IEnumerable<Node> FindAllDescendants(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            yield return child;
            foreach (var descendant in FindAllDescendants(child))
                yield return descendant;
        }
    }
}

/// <summary>
/// Minimal invisible <see cref="Node"/> that intercepts unhandled right-click
/// events and forwards them to <see cref="ClickToKillHandler.TryKillHoveredEnemy"/>.
/// </summary>
internal partial class InputListenerNode : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            ClickToKillHandler.TryKillHoveredEnemy();
    }
}
