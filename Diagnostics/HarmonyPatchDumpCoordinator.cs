using BaseLib.Config;

namespace BaseLib.Diagnostics;

/// <summary>
///     Manual and first-main-menu Harmony patch dumps using <see cref="BaseLibConfig" />.
/// </summary>
internal static class HarmonyPatchDumpCoordinator
{
    private static int _autoDumpIssuedForSession;

    /// <summary>
    ///     Deferred from <see cref="MegaCrit.Sts2.Core.Nodes.Screens.MainMenu.NMainMenu._Ready" />; at most once per
    ///     process when enabled.
    /// </summary>
    internal static void TryAutoDumpOnFirstMainMenu()
    {
        if (!BaseLibConfig.HarmonyPatchDumpOnFirstMainMenu)
            return;

        if (Interlocked.CompareExchange(ref _autoDumpIssuedForSession, 1, 0) != 0)
            return;

        TryDumpToConfiguredPath(BaseLibConfig.HarmonyPatchDumpOutputPath, "[HarmonyDump][Auto]");
    }

    internal static void TryManualDumpFromSettings()
    {
        TryDumpToConfiguredPath(BaseLibConfig.HarmonyPatchDumpOutputPath, "[HarmonyDump][Manual]");
    }

    private static void TryDumpToConfiguredPath(string rawPath, string logPrefix)
    {
        var resolved = HarmonyPatchDumpWriter.TryResolveFilesystemPath(rawPath);
        if (string.IsNullOrEmpty(resolved))
        {
            BaseLibMain.Logger.Warn(
                $"{logPrefix} Output path is empty or invalid. Set a path in BaseLib mod config (or use Browse).");
            return;
        }

        if (!HarmonyPatchDumpWriter.TryWrite(resolved, out var err))
        {
            BaseLibMain.Logger.Warn($"{logPrefix} Failed to write dump: {err}");
            return;
        }

        BaseLibMain.Logger.Info($"{logPrefix} Wrote Harmony patch dump to: {resolved}");
    }
}