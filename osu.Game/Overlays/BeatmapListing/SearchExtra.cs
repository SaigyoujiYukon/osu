﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchExtra
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ExtraVideo))]
        [Description("有视频")]
        Video,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.ExtraStoryboard))]
        [Description("有故事版")]
        Storyboard
    }
}
