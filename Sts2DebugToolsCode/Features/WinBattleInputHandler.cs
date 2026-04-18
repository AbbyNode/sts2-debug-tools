using Godot;
using MegaCrit.Sts2.Core.Combat;

namespace Sts2DebugTools.Sts2DebugToolsCode.Features;

/// <summary>
/// Listens for the <see cref="Key.F5"/> key during active combat and calls
/// <see cref="InstantWinHelper.Execute"/> to instantly win the battle.
///
/// The input listener is attached to the <see cref="SceneTree"/> root viewport
/// rather than <see cref="MegaCrit.Sts2.Core.Nodes.NRun"/>, so it works even
/// when <c>NRun.Instance</c> is <see langword="null"/>.
/// </summary>
internal static class WinBattleInputHandler
{
    private const string ListenerNodeName = "DebugToolsWinBattle";

    private static WinBattleListenerNode? _listener;

    /// <summary>
    /// Attaches the key listener to the scene-tree root.
    /// Safe to call multiple times; any previous listener is removed first.
    /// </summary>
    internal static void Attach()
    {
        Detach();

        var root = ((SceneTree)Engine.GetMainLoop()).Root;
        if (root == null)
        {
            MainFile.Logger.Warn("[DebugTools] SceneTree root is null; Win-Battle key handler cannot be attached.");
            return;
        }

        _listener = new WinBattleListenerNode { Name = ListenerNodeName };
        root.CallDeferred(Node.MethodName.AddChild, _listener);
        MainFile.Logger.Info("[DebugTools] Win-Battle key handler attached (press F5 during combat).");
    }

    /// <summary>Removes the key listener from the scene tree.</summary>
    internal static void Detach()
    {
        if (_listener != null && GodotObject.IsInstanceValid(_listener))
            _listener.QueueFree();

        _listener = null;
    }
}

/// <summary>
/// Minimal invisible <see cref="Node"/> that intercepts unhandled <see cref="Key.F5"/>
/// key presses and triggers <see cref="InstantWinHelper.Execute"/> when combat is active.
/// </summary>
internal partial class WinBattleListenerNode : Node
{
    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Keycode: Key.F5 }
            && CombatManager.Instance.IsInProgress)
        {
            InstantWinHelper.Execute();
        }
    }
}
