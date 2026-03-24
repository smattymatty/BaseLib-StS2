using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Patches.Features;

[HarmonyPatch(typeof(PowerModel), "AddDumbVariablesToDescription")]
public class TemporaryPowerModelPatch
{
    [HarmonyPostfix]
    static void Postfix(PowerModel __instance, LocString description)
    {
        if (__instance is not CustomTemporaryPowerModel customTemporaryPowerModel)
            return;
        description.Add("Title", customTemporaryPowerModel.InternallyAppliedPower.Title);
    }
}