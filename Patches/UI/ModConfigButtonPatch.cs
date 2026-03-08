using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using BaseLib.Config;
using BaseLib.Config.UI;
using BaseLib.Utils;

namespace BaseLib.Patches.UI;

[HarmonyPatch(typeof(NModInfoContainer), nameof(NModInfoContainer._Ready))]
public static class ModConfigButtonPatch
{
    public static readonly SpireField<NModInfoContainer, NButton> ConfigButton = new((node)=>
    {
        var button = CreateButton(node); 
        return button;
    });

    private static NButton CreateButton(NModInfoContainer node)
    {
        var button = new NButton(); //For future maybe make a custom scene.
        
        button.Name = "ConfigButton";
        
        node.AddChild(button);
        button.Owner = node;
        
        button.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomRight);
        button.Position += new Vector2(-200, -80); // Adjust position
        
        var label = new MegaLabel();
        label.Name = "Label";
        button.AddChild(label);
        label.Text = "Config";

        button.Connect(NClickableControl.SignalName.Released, Callable.From<NButton>(btn => {
            OnConfigPressed(node);
        }));
        
        button.Hide();
        
        return button;
    }
    
    [HarmonyPostfix]
    public static void PrepButton(NModInfoContainer __instance)
    {
        ConfigButton.Get(__instance);
    }

    private static void OnConfigPressed(NModInfoContainer container)
    {
        var mod = ModConfigFillPatch.CurrentMod;
        if (mod == null) return;
        
        var configPanel = ModConfigRegistry.Get(mod.pckName);
        if (configPanel != null)
        {
            // Find NModdingScreen
            Node parent = container.GetParent();
            while (parent != null && !(parent is NModdingScreen))
            {
                parent = parent.GetParent();
            }

            if (parent is NModdingScreen screen)
            {
                // Instantiate our manual config control
                var configControl = new ModConfigControl(mod.manifest?.name ?? mod.pckName, configPanel, screen);
                // We add it to the screen's parent and hide the screen
                screen.GetParent().AddChild(configControl);
                screen.Visible = false;
            }
        }
    }
}

[HarmonyPatch(typeof(NModInfoContainer), nameof(NModInfoContainer.Fill))]
public static class ModConfigFillPatch
{
    public static Mod? CurrentMod { get; private set; }

    public static void Postfix(NModInfoContainer __instance, Mod mod)
    {
        CurrentMod = mod;
        var configButton = ModConfigButtonPatch.ConfigButton.Get(__instance);
        if (configButton != null)
        {
            if (mod.wasLoaded && ModConfigRegistry.Get(mod.pckName) != null)
            {
                configButton.Show();
            }
            else
            {
                configButton.Hide();
            }
        }
    }
}
