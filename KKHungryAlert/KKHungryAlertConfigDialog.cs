using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Common;

namespace KKHungryAlert;

public class KKHungryAlertConfigDialog(ICoreClientAPI capi, KKHungryAlertSystem system, KKHungryAlertConfig config) : GuiDialog(capi)
{
    public override string ToggleKeyCombinationCode => null;

    public override void OnGuiOpened()
    {
        SetupDialog();
        base.OnGuiOpened();
    }

    private void SetupDialog()
    {
        // 1. ダイアログのルートBounds (Autosized)
        // CenterMiddleに配置
        ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

        // 2. 背景のBounds (固定サイズ 400x250)
        // ここが重要: dialogBoundsの子要素にする必要がありますが、AddShadedDialogBGは引数のBoundsを使います。
        // 一般的なパターンとして、Contents用のBoundsを作り、それを背景として使います。

        ElementBounds bgBounds = ElementBounds.Fixed(0, 0, 400, 290);

        // bgBoundsをdialogBoundsの子として登録します。
        // これにより、座標計算時にdialogBoundsからの相対座標として処理されます以及親が見つかるようになります。
        dialogBounds.WithChild(bgBounds);

        // 3. コンテンツ用Bounds
        // 各要素は背景の上に配置するため、bgBoundsの子として定義します。

        // Paddingなどを考慮する場合
        // ElementBounds mainBounds = bgBounds.ForkBoundingParent(5, 5, 5, 5); 
        // ではなく、bgBoundsの中に配置していく形にします。

        // 1. Threshold
        ElementBounds thresholdTextBounds = ElementBounds.Fixed(20, 50, 300, 20).WithParent(bgBounds);
        ElementBounds thresholdSliderBounds = ElementBounds.Fixed(20, 80, 360, 20).WithParent(bgBounds);

        // 2. Interval
        ElementBounds intervalTextBounds = ElementBounds.Fixed(20, 115, 300, 20).WithParent(bgBounds);
        ElementBounds intervalSliderBounds = ElementBounds.Fixed(20, 145, 360, 20).WithParent(bgBounds);

        // 3. Volume
        ElementBounds volumeTextBounds = ElementBounds.Fixed(20, 180, 300, 20).WithParent(bgBounds);
        ElementBounds volumeSliderBounds = ElementBounds.Fixed(20, 210, 360, 20).WithParent(bgBounds);

        // Close Button
        ElementBounds closeButtonBounds = ElementBounds.Fixed(0, 250, 100, 25)
            .WithAlignment(EnumDialogArea.CenterFixed)
            .WithParent(bgBounds);


        SingleComposer = capi.Gui.CreateCompo("kkhungryalertconfig", dialogBounds)
            .AddShadedDialogBG(bgBounds) // 背景を追加
            .AddDialogTitleBar("KKHungryAlert Settings", OnTitleBarClose) // タイトルバーは自動配置
            .AddStaticText("Hunger Threshold", CairoFont.WhiteSmallText(), thresholdTextBounds)
            .AddSlider(OnThresholdChanged, thresholdSliderBounds, "thresholdSlider")
            .AddStaticText("Check Interval (Seconds)", CairoFont.WhiteSmallText(), intervalTextBounds)
            .AddSlider(OnIntervalChanged, intervalSliderBounds, "intervalSlider")
            .AddStaticText("Volume", CairoFont.WhiteSmallText(), volumeTextBounds)
            .AddSlider(OnVolumeChanged, volumeSliderBounds, "volumeSlider")
            .AddSmallButton("Close", OnButtonClose, closeButtonBounds)
            .Compose();

        // Set initial values
        SingleComposer.GetSlider("thresholdSlider").SetValues((int)config.HungerThreshold, 0, 1500, 50);
        SingleComposer.GetSlider("intervalSlider").SetValues((int)config.CheckIntervalSeconds, 1, 600, 1);
        SingleComposer.GetSlider("volumeSlider").SetValues((int)(config.SoundVolume * 100), 0, 100, 1);
    }

    private bool OnThresholdChanged(int value)
    {
        system.UpdateConfigFromGui("threshold", value);
        return true;
    }

    private bool OnIntervalChanged(int value)
    {
        system.UpdateConfigFromGui("interval", value);
        return true;
    }

    private bool OnVolumeChanged(int value)
    {
        system.UpdateConfigFromGui("volume", value / 100f);
        return true;
    }

    private void OnTitleBarClose()
    {
        TryClose();
    }

    private bool OnButtonClose()
    {
        TryClose();
        return true;
    }

    // Also save on close to be sure
    public override void OnGuiClosed()
    {
        system.SaveConfig();
        base.OnGuiClosed();
    }
}
