using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace BaseLib.Abstracts;

public abstract class CustomEncounterModel : EncounterModel, ICustomModel
{
    protected override bool HasCustomBackground => false;
    public virtual string? CustomBackgroundScenePath => null;
    public virtual string AssetsName => Id.Entry.ToLowerInvariant();
    public virtual bool UseVanillaBackground => false;
    private string CustomBackgroundScenePathFallback => SceneHelper.GetScenePath($"backgrounds/{AssetsName}/{AssetsName}_background");
    public virtual NCombatBackground CreateCustomBackground(ActModel parentAct, Rng rng) {
        if(UseVanillaBackground)
            return NCombatBackground.Create(CreateBackgroundAssetsForCustom(rng));
        return CreateNCombatBackground(CustomBackgroundScenePath);
    }
    public NCombatBackground CreateNCombatBackground(string? path)
    {
        Control control = PreloadManager.Cache.GetScene(path ?? CustomBackgroundScenePathFallback).Instantiate<Control>();
        NCombatBackground? ncombatBackground = control as NCombatBackground;
        if(ncombatBackground != null)
            return ncombatBackground;
        ncombatBackground = new NCombatBackground();
        AddCustomLayer(ncombatBackground,control);
        return ncombatBackground;
    }

    private static void AddCustomLayer(NCombatBackground bg, Control layer) {
        layer.Visible = true;
        bg.AddChildSafely(layer);
    }
    
    public IEnumerable<string> GetAssetPaths(IRunState runState)
    {
        HashSet<string> assetPaths = new HashSet<string>();
        if (this.HasScene) {
            string ScenePath = Traverse.Create(this).Property<string>("ScenePath").Value;
            assetPaths.Add(CustomScenePath ?? ScenePath);
        }
        if (this.ExtraAssetPaths != null)
            assetPaths.UnionWith(this.ExtraAssetPaths);
        foreach ((MonsterModel monsterModel, string _) in (IEnumerable<(MonsterModel, string)>) this.MonstersWithSlots)
            assetPaths.UnionWith(monsterModel.AssetPaths);
        return (IEnumerable<string>) assetPaths;
    }
    private BackgroundAssets CreateBackgroundAssetsForCustom(Rng rng)//CreateBackgroundAssetsForCustom
    {
        return new BackgroundAssets(AssetsName, rng);//TODO create from custom path
    }
    
    public override bool HasScene => false;
    public virtual string? CustomScenePath => null;
    public virtual Control CreateCustomScene()
    {
        string ScenePath = Traverse.Create(this).Property<string>("ScenePath").Value;
        return PreloadManager.Cache.GetScene(CustomScenePath ?? ScenePath).Instantiate<Control>();
    }
    
    [HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateScene))]
    private static class ScenePatch {
        static bool Prefix(EncounterModel __instance, ref Control __result) {
            CustomEncounterModel? model = __instance as CustomEncounterModel;
            if (__instance is not CustomEncounterModel)
                return true;
            __result = model!.CreateCustomScene();
            return false;
        }
    }

    [HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.GetAssetPaths))]
    private static class AssetPathsPatch {
        [HarmonyPrefix]
        static bool Prefix(EncounterModel __instance, ref IEnumerable<string> __result, IRunState runState) {
            CustomEncounterModel? model = __instance as CustomEncounterModel;
            if (__instance is not CustomEncounterModel)
                return true;
            if (model!.UseVanillaBackground)
                return true;
            __result = model.GetAssetPaths(runState);
            return false;
        }
    }
    [HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateBackground))]
    private static class BackgroundPatch {
        [HarmonyPrefix]
        static bool Prefix(EncounterModel __instance, ref NCombatBackground __result, ActModel parentAct, Rng rng) {
            CustomEncounterModel? model = __instance as CustomEncounterModel;
            if (__instance is not CustomEncounterModel)
                return true;
            bool HasCustomBackground = Traverse.Create(model).Property<bool>("HasCustomBackground").Value;
            if(!HasCustomBackground)
                return true;
            __result = model!.CreateCustomBackground(parentAct, rng);
            return false;
        }
    }
}