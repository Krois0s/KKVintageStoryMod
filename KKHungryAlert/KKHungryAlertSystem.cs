using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace KKHungryAlert;

public class KKHungryAlertSystem : ModSystem
{
    private ICoreClientAPI capi;
    private long listenerId;

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        capi = api;
        // Check every 10 seconds (10000 ms)
        listenerId = api.Event.RegisterGameTickListener(CheckHunger, 10000);
    }

    private void CheckHunger(float dt)
    {
        if (capi.World.Player == null || capi.World.Player.Entity == null) return;

        var hungerTree = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("hunger");
        if (hungerTree == null) return;

        float saturation = hungerTree.GetFloat("currentsaturation");

        // Max saturation is typically 1500. Alert if below 300 (20%)
        if (saturation < 300)
        {
            // Play a sound to indicate hunger. 
            // Using "kkhungryalert:sounds/stomach"
            capi.World.PlaySoundAt(new AssetLocation("kkhungryalert:sounds/stomach"), capi.World.Player.Entity, null, true, 16, 1);
            
            // Optional: Show a message for debugging clarity
            // capi.ShowChatMessage("Your stomach growls...");
        }
    }

    public override void Dispose()
    {
        if (capi != null && listenerId != 0)
        {
            capi.Event.UnregisterGameTickListener(listenerId);
        }
        base.Dispose();
    }
}
