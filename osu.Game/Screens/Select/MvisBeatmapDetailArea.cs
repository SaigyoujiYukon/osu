using osu.Game.Overlays.Settings;
using osu.Framework.Graphics;
using System;

namespace osu.Game.Screens.Select
{
    public class MvisBeatmapDetailArea : BeatmapDetailArea
    {
        public Action OnPressedAction;
        protected override BeatmapDetailAreaTabItem[] CreateTabItems() => new BeatmapDetailAreaTabItem[]
        {
            new VoidTabItem(),
        };

        public MvisBeatmapDetailArea()
        {
            Add(
                new SettingsButton()
                {
                    Text = "选择该谱面",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Action = () => OnPressedAction?.Invoke()
                }
            );
        }

        private class VoidTabItem : BeatmapDetailAreaTabItem
        {
            public override string Name => "选择该谱面";
        }
    }
}