// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChangelogButton : ToolbarOverlayToggleButtonRightSide
    {
        public ToolbarChangelogButton()
        {
            SetIcon(FontAwesome.Solid.Bullhorn);
            TooltipMain = "更新日志";
            TooltipSub = "在这里查看更新日志";
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChangelogOverlay changelog)
        {
            StateContainer = changelog;
        }
    }
}
