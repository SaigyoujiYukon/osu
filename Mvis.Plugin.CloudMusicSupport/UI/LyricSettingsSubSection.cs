using Mvis.Plugin.CloudMusicSupport.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.CloudMusicSupport.UI
{
    public class LyricSettingsSubSection : PluginSettingsSubSection
    {
        public LyricSettingsSubSection(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (LyricConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "启用本插件",
                    Current = config.GetBindable<bool>(LyricSettings.EnablePlugin)
                },
                new SettingsCheckbox
                {
                    LabelText = "自动保存歌词到本地",
                    Current = config.GetBindable<bool>(LyricSettings.SaveLrcWhenFetchFinish),
                    TooltipText = "保存后歌词在离线时也能用，但如果保存的歌词有问题，您需要手动删除对应文件并重新获取"
                },
                new SettingsSlider<double>
                {
                    LabelText = "全局歌词偏移(毫秒)",
                    Current = config.GetBindable<double>(LyricSettings.LyricOffset),
                    TooltipText = "如果当前歌曲歌词太早或太晚，拖动这里的滑条试试"
                },
                new SettingsSlider<float>
                {
                    LabelText = "歌词淡入时间",
                    TooltipText = "调整歌词淡入动画要花多长时间",
                    Current = config.GetBindable<float>(LyricSettings.LyricFadeInDuration)
                },
                new SettingsSlider<float>
                {
                    LabelText = "歌词淡出时间",
                    TooltipText = "调整歌词淡出动画要花多长时间",
                    Current = config.GetBindable<float>(LyricSettings.LyricFadeOutDuration)
                },
            };
        }
    }
}
