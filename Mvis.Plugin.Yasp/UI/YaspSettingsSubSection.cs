using M.Resources.Localisation.Mvis;
using M.Resources.Localisation.Mvis.Plugins;
using Mvis.Plugin.Yasp.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.Yasp.UI
{
    public class YaspSettingsSubSection : PluginSettingsSubSection
    {
        public YaspSettingsSubSection(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (YaspConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsSlider<float>
                {
                    LabelText = YaspStrings.Scale,
                    Current = config.GetBindable<float>(YaspSettings.Scale),
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = MvisGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(YaspSettings.EnablePlugin)
                }
            };
        }
    }
}
