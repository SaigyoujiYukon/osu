// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Extensions;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsPlayer : RoomSubmittingPlayer
    {
        public Action Exited;

        protected override UserActivity InitialActivity => new UserActivity.InPlaylistGame(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        public PlaylistsPlayer(Room room, PlaylistItem playlistItem, PlayerConfiguration configuration = null)
            : base(room, playlistItem, configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<RulesetInfo> ruleset)
        {
            // Sanity checks to ensure that PlaylistsPlayer matches the settings for the current PlaylistItem
            if (!Beatmap.Value.BeatmapInfo.MatchesOnlineID(PlaylistItem.Beatmap.Value))
                throw new InvalidOperationException("当前谱面与游玩列表不匹配");

            if (!ruleset.Value.MatchesOnlineID(PlaylistItem.Ruleset.Value))
                throw new InvalidOperationException("当前游戏模式与游玩列表不匹配");

            if (!PlaylistItem.RequiredMods.All(m => Mods.Value.Any(m.Equals)))
                throw new InvalidOperationException("当前Mods与游玩列表所需要的不匹配");
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            Exited?.Invoke();

            return false;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(Room.RoomID.Value != null);
            return new PlaylistsResultsScreen(score, Room.RoomID.Value.Value, PlaylistItem, true);
        }

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            Score.ScoreInfo.TotalScore = (int)Math.Round(ScoreProcessor.GetStandardisedScore());
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Exited = null;
        }
    }
}
