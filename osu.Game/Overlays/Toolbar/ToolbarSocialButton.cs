// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSocialButton : ToolbarOverlayToggleButton
    {
        public ToolbarSocialButton()
        {
            Icon = FontAwesome.Solid.Users;
            TooltipMain = "社交";
            TooltipSub = "其实是排行榜(";
        }

        [BackgroundDependencyLoader(true)]
        private void load(SocialOverlay chat)
        {
            StateContainer = chat;
        }
    }
}
