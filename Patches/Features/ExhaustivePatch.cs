using BaseLib.Cards.Variables;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace BaseLib.Patches.Features;

[HarmonyPatch(typeof(CardModel), "GetResultPileType")]
public static class ExhaustivePatch
{
    static void Prefix(CardModel __instance, ref PileType pileType)
    {
        if (GetExhaustive(__instance) == 1 )
        {
            __instance.ExhaustOnNextPlay = true;
        }
    }

    public static int GetExhaustive(this CardModel card)
    {
        var exhaustiveAmount = card.DynamicVars.TryGetValue(ExhaustiveVar.Key, out var val) ? val.IntValue : 0;
        return ExhaustiveVar.ExhaustiveCount(card, exhaustiveAmount);
    }
}