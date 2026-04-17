using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace Sts2DebugTools.Sts2DebugToolsCode.Features;

/// <summary>
/// Harmony patch that enables <see cref="NMapScreen.SetDebugTravelEnabled"/>
/// every time the map screen opens, allowing the player to click any room on
/// the map to teleport there directly instead of being restricted to normal
/// travel rules.
/// </summary>
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
internal static class MapDebugTravelPatch
{
    private static void Postfix(NMapScreen __instance)
    {
        __instance.SetDebugTravelEnabled(true);
        MainFile.Logger.Info("[DebugTools] Debug travel enabled on map screen.");
    }
}
