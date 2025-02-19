// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class UserSortTabControl : OverlaySortTabControl<UserSortCriteria>
    {
    }

    public enum UserSortCriteria
    {
        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.LastVisit))]
        [Description(@"最近活跃")]
        LastVisit,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Rank))]
        Rank,

        [LocalisableDescription(typeof(SortStrings), nameof(SortStrings.Username))]
        Username
    }
}
