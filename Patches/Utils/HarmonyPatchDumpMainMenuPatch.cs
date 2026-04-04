using BaseLib.Diagnostics;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace BaseLib.Patches.Utils;

/// <summary>
///     After main menu is ready, optionally dump Harmony patch info once per session (deferred).
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class HarmonyPatchDumpMainMenuPatch
{
    /// <summary>
    ///     After main menu is ready, optionally dump Harmony patch info once per session (deferred).
    /// </summary>
    public static void Postfix()
    {
        Callable.From(HarmonyPatchDumpCoordinator.TryAutoDumpOnFirstMainMenu).CallDeferred();
    }
}