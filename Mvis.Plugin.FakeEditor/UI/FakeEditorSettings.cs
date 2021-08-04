using M.Resources.Localisation.Mvis;
using Mvis.Plugin.FakeEditor.Config;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Config;

namespace Mvis.Plugin.FakeEditor.UI
{
    public class FakeEditorSettings : PluginSettingsSubSection
    {
        public FakeEditorSettings(MvisPlugin plugin)
            : base(plugin)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (FakeEditorConfigManager)ConfigManager;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = MvisGenericStrings.EnablePlugin,
                    Current = config.GetBindable<bool>(FakeEditorSetting.EnableFakeEditor)
                },
            };
        }
    }
}
