// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Types;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class MvisAudioSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "音频";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, MvisPluginManager pluginManager)
        {
            AudioControlPluginDropDown dropdown;
            Children = new Drawable[]
            {
                new SettingsSlider<double>
                {
                    LabelText = "播放速度",
                    Current = config.GetBindable<double>(MSetting.MvisMusicSpeed),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                    TransferValueOnCommit = true
                },
                new SettingsCheckbox
                {
                    LabelText = "调整音调",
                    Current = config.GetBindable<bool>(MSetting.MvisAdjustMusicWithFreq),
                    TooltipText = "暂不支持调整故事版的音调"
                },
                new SettingsCheckbox
                {
                    LabelText = "夜核节拍器",
                    Current = config.GetBindable<bool>(MSetting.MvisEnableNightcoreBeat),
                    TooltipText = "动次打次动次打次"
                },
                dropdown = new AudioControlPluginDropDown
                {
                    LabelText = "音乐控制插件"
                }
            };

            var plugins = pluginManager.GetAllAudioControlPlugin();

            var currentAudioControlPlugin = config.Get<string>(MSetting.MvisCurrentAudioProvider);

            foreach (var pl in plugins)
            {
                var type = pl.GetType();

                if (currentAudioControlPlugin == type.Name + "@" + type.Namespace)
                {
                    dropdown.Current.Value = pl;
                }
            }

            dropdown.Items = plugins;
            dropdown.Current.BindValueChanged(v =>
            {
                if (v.NewValue == null)
                {
                    config.SetValue(MSetting.MvisCurrentAudioProvider, string.Empty);
                    return;
                }

                var pl = (MvisPlugin)v.NewValue;
                var type = pl.GetType();

                config.SetValue(MSetting.MvisCurrentAudioProvider, type.Name + "@" + type.Namespace);
            });
        }

        private class AudioControlPluginDropDown : SettingsDropdown<IProvideAudioControlPlugin>
        {
            protected override OsuDropdown<IProvideAudioControlPlugin> CreateDropdown()
                => new PluginDropDownControl();

            private class PluginDropDownControl : DropdownControl
            {
                protected override LocalisableString GenerateItemText(IProvideAudioControlPlugin item)
                {
                    return ((MvisPlugin)item).Name;
                }
            }
        }
    }
}
