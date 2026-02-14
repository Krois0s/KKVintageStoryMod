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

    private KKHungryAlertConfigDialog configDialog;

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        capi = api;

        LoadConfig();

        // Register command to change settings on the fly
        // Usage: .kkha threshold [value] | .kkha interval [value] | .kkha volume [value] | .kkha config
        api.ChatCommands.Create("kkha")
            .WithDescription("KKHungryAlert Configuration")
            .WithAlias("kkhungryalert")
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(args => OpenConfigDialog())
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
            .EndSubCommand()
            .BeginSubCommand("config")
                .HandleWith(args => OpenConfigDialog())
            .EndSubCommand();

        StartListener();
    }

    private TextCommandResult OpenConfigDialog()
    {
        configDialog ??= new KKHungryAlertConfigDialog(capi, this, config);
        configDialog.TryOpen();
        return TextCommandResult.Success();
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
                config.SatietyThreshold = value;
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
        SaveConfig();
        return TextCommandResult.Success();
    }

    public void UpdateConfigFromGui(string key, float value)
    {
        switch (key)
        {
            case "threshold":
                config.SatietyThreshold = value;
                break;
            case "interval":
                config.CheckIntervalSeconds = value;
                StartListener();
                break;
            case "volume":
                config.SoundVolume = value;
                break;
        }
    }

    public void SaveConfig()
    {
        capi.StoreModConfig(config, "KKHungryAlert.json");
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

        if (saturation < config.SatietyThreshold)
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
        if (configDialog != null)
        {
            configDialog.TryClose();
            configDialog.Dispose();
        }
        base.Dispose();
    }
}
