using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Mvis.Plugins;
using osu.Game.Screens.Mvis.Plugins.Types;

namespace Mvis.Plugin.Example.Sidebar
{
    public class ExampleSidebarPage : PluginSidebarPage
    {
        private OsuSpriteText text;

        public ExampleSidebarPage(MvisPlugin plugin)
            : base(plugin, 0.3f)
        {
        }

        public override IPluginFunctionProvider GetFunctionEntry() => null;

        protected override void InitContent(MvisPlugin plugin)
        {
            var pl = (ExamplePlugin)plugin;

            Add(text = new OsuSpriteText
            {
                Text = "Hello World!",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });

            pl.BindableString.BindValueChanged(v => text.Text = v.NewValue, true);
            base.InitContent(plugin);
        }
    }
}
