using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Sts2DebugTools.Sts2DebugToolsCode.Features;

/// <summary>
/// Instantly wins the current combat by zeroing the HP of every primary
/// enemy, then prompting <see cref="CombatManager"/> to evaluate the win
/// condition so it can perform its normal end-of-combat flow.
/// </summary>
internal static class InstantWinHelper
{
    /// <summary>Cached handle for the private <c>Creature._currentHp</c> backing field.</summary>
    private static readonly FieldInfo? CurrentHpField =
        AccessTools.Field(typeof(Creature), "_currentHp");

    /// <summary>
    /// Sets every living primary enemy's HP to 0 and triggers the combat
    /// win condition check.  Safe to call when not in combat (no-op).
    /// </summary>
    internal static void Execute()
    {
        if (CombatManager.Instance?.IsInProgress != true)
        {
            MainFile.Logger.Warn("[DebugTools] InstantWin called but no combat is in progress.");
            return;
        }

        if (CurrentHpField == null)
        {
            MainFile.Logger.Error("[DebugTools] Could not resolve Creature._currentHp via reflection; instant win aborted.");
            return;
        }

        var state = CombatManager.Instance?.DebugOnlyGetState();
        if (state == null)
        {
            MainFile.Logger.Warn("[DebugTools] CombatState is null; instant win aborted.");
            return;
        }

        int killed = 0;
        foreach (var enemy in state.Enemies.Where(e => e.IsPrimaryEnemy && e.IsAlive).ToList())
        {
            try
            {
                CurrentHpField.SetValue(enemy, 0);
                enemy.InvokeDiedEvent();
                killed++;
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"[DebugTools] Failed to kill enemy '{enemy}': {ex.Message}");
            }
        }

        MainFile.Logger.Info(
            $"[DebugTools] Instant win: zeroed HP of {killed} primary {(killed == 1 ? "enemy" : "enemies")}.");

        // Fire-and-forget: ask CombatManager to evaluate the win condition.
        // IsEnding will now return true (no alive primary enemies), causing
        // EndCombatInternal to run and complete the battle normally.
        _ = CombatManager.Instance?.CheckWinCondition();
    }

    /// <summary>
    /// Sets a single enemy's HP to 0, triggers its death event, then asks
    /// <see cref="CombatManager"/> to evaluate the win condition.
    /// </summary>
    internal static void KillSingle(Creature enemy)
    {
        if (CurrentHpField == null)
        {
            MainFile.Logger.Error("[DebugTools] Could not resolve Creature._currentHp; single-kill aborted.");
            return;
        }

        try
        {
            CurrentHpField.SetValue(enemy, 0);
            enemy.InvokeDiedEvent();
            MainFile.Logger.Info($"[DebugTools] Right-click kill: '{enemy}' defeated.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[DebugTools] Failed to kill enemy '{enemy}': {ex.Message}");
            return;
        }

        // Check win condition in case this was the last primary enemy.
        _ = CombatManager.Instance?.CheckWinCondition();
    }
}
