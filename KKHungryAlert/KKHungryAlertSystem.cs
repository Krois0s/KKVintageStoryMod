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
    private KKHungryAlertConfig config;

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        capi = api;

        LoadConfig();

        // Register command to change settings on the fly
        // Usage: .kkha threshold [value] | .kkha interval [value] | .kkha volume [value]
        api.ChatCommands.Create("kkha")
            .WithDescription("KKHungryAlert Configuration")
            .WithAlias("kkhungryalert")
            .RequiresPrivilege(Privilege.chat)
            .BeginSubCommand("threshold")
                .WithArgs(api.ChatCommands.Parsers.Float("value"))
                .HandleWith(args => UpdateConfig("threshold", (float)args.Parsers[0].GetValue()))
            .EndSubCommand()
            .BeginSubCommand("interval")
                .WithArgs(api.ChatCommands.Parsers.Float("value"))
                .HandleWith(args => UpdateConfig("interval", (float)args.Parsers[0].GetValue()))
            .EndSubCommand()
            .BeginSubCommand("volume")
                .WithArgs(api.ChatCommands.Parsers.Float("value"))
                .HandleWith(args => UpdateConfig("volume", (float)args.Parsers[0].GetValue()))
            .EndSubCommand();

        StartListener();
    }

    private void LoadConfig()
    {
        try
        {
            config = capi.LoadModConfig<KKHungryAlertConfig>("KKHungryAlert.json");
            if (config == null)
            {
                config = new KKHungryAlertConfig();
                capi.StoreModConfig(config, "KKHungryAlert.json");
            }
        }
        catch
        {
            config = new KKHungryAlertConfig();
            capi.StoreModConfig(config, "KKHungryAlert.json");
        }
    }

    private TextCommandResult UpdateConfig(string key, float value)
    {
        switch (key)
        {
            case "threshold":
                config.HungerThreshold = value;
                capi.ShowChatMessage($"[KKHungryAlert] Threshold set to {value}");
                break;
            case "interval":
                config.CheckIntervalSeconds = value;
                capi.ShowChatMessage($"[KKHungryAlert] Interval set to {value}s");
                StartListener();
                break;
            case "volume":
                config.SoundVolume = value;
                capi.ShowChatMessage($"[KKHungryAlert] Volume set to {value}");
                break;
        }
        capi.StoreModConfig(config, "KKHungryAlert.json");
        return TextCommandResult.Success();
    }

    private void StartListener()
    {
        if (listenerId != 0) capi.Event.UnregisterGameTickListener(listenerId);
        listenerId = capi.Event.RegisterGameTickListener(CheckHunger, (int)(config.CheckIntervalSeconds * 1000));
    }

    private void CheckHunger(float dt)
    {
        if (capi.World.Player == null || capi.World.Player.Entity == null) return;

        var hungerTree = capi.World.Player.Entity.WatchedAttributes.GetTreeAttribute("hunger");
        if (hungerTree == null) return;

        float saturation = hungerTree.GetFloat("currentsaturation");

        if (saturation < config.HungerThreshold)
        {
            // Play sound with configured volume
            capi.World.PlaySoundAt(new AssetLocation("kkhungryalert:sounds/stomach"), capi.World.Player.Entity, null, true, 16, config.SoundVolume);
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
