// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.BeatmapListing
{
    public enum SearchCategory
    {
        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusAny))]
        Any,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusLeaderboard))]
        [Description("拥有排行榜的谱面")]
        Leaderboard,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusRanked))]
        Ranked,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusQualified))]
        Qualified,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusLoved))]
        Loved,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusFavourites))]
        Favourites,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusPending))]
        [Description("Pending & WIP")]
        Pending,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusGraveyard))]
        Graveyard,

        [LocalisableDescription(typeof(BeatmapsStrings), nameof(BeatmapsStrings.StatusMine))]
        [Description("我制作的谱面")]
        Mine,
    }
}
