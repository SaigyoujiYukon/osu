// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Game.Database;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Handles the storage and retrieval of Beatmaps/BeatmapSets to the database backing
    /// </summary>
    public class BeatmapStore : MutableDatabaseBackedStoreWithFileIncludes<BeatmapSetInfo, BeatmapSetFileInfo>
    {
        public event Action<BeatmapInfo> BeatmapHidden;
        public event Action<BeatmapInfo> BeatmapRestored;

        public BeatmapStore(IDatabaseContextFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Hide a <see cref="BeatmapInfo"/> in the database.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to hide.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Hide(BeatmapInfo beatmapInfo)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref beatmapInfo, Beatmaps);

                if (beatmapInfo.Hidden) return false;

                beatmapInfo.Hidden = true;
            }

            BeatmapHidden?.Invoke(beatmapInfo);
            return true;
        }

        /// <summary>
        /// Restore a previously hidden <see cref="BeatmapInfo"/>.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to restore.</param>
        /// <returns>Whether the beatmap's <see cref="BeatmapInfo.Hidden"/> was changed.</returns>
        public bool Restore(BeatmapInfo beatmapInfo)
        {
            using (ContextFactory.GetForWrite())
            {
                Refresh(ref beatmapInfo, Beatmaps);

                if (!beatmapInfo.Hidden) return false;

                beatmapInfo.Hidden = false;
            }

            BeatmapRestored?.Invoke(beatmapInfo);
            return true;
        }

        protected override IQueryable<BeatmapSetInfo> AddIncludesForDeletion(IQueryable<BeatmapSetInfo> query) =>
            base.AddIncludesForDeletion(query)
                .Include(s => s.Metadata)
                .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata);

        protected override IQueryable<BeatmapSetInfo> AddIncludesForConsumption(IQueryable<BeatmapSetInfo> query) =>
            base.AddIncludesForConsumption(query)
                .Include(s => s.Metadata)
                .Include(s => s.Beatmaps).ThenInclude(s => s.Ruleset)
                .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata);

        protected override void Purge(List<BeatmapSetInfo> items, OsuDbContext context)
        {
            // metadata is M-N so we can't rely on cascades
            context.BeatmapMetadata.RemoveRange(items.Select(s => s.Metadata));
            context.BeatmapMetadata.RemoveRange(items.SelectMany(s => s.Beatmaps.Select(b => b.Metadata).Where(m => m != null)));

            // todo: we can probably make cascades work here with a FK in BeatmapDifficulty. just make to make it work correctly.
            context.BeatmapDifficulty.RemoveRange(items.SelectMany(s => s.Beatmaps.Select(b => b.BaseDifficulty)));

            base.Purge(items, context);
        }

        public IQueryable<BeatmapSetInfo> BeatmapSetsOverview => ContextFactory.Get().BeatmapSetInfo
                                                                               .Include(s => s.Metadata)
                                                                               .Include(s => s.Beatmaps)
                                                                               .AsNoTracking();

        public IQueryable<BeatmapSetInfo> BeatmapSetsWithoutRuleset => ContextFactory.Get().BeatmapSetInfo
                                                                                     .Include(s => s.Metadata)
                                                                                     .Include(s => s.Files).ThenInclude(f => f.FileInfo)
                                                                                     .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                                                                                     .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata)
                                                                                     .AsNoTracking();

        public IQueryable<BeatmapSetInfo> BeatmapSetsWithoutFiles => ContextFactory.Get().BeatmapSetInfo
                                                                                   .Include(s => s.Metadata)
                                                                                   .Include(s => s.Beatmaps).ThenInclude(s => s.Ruleset)
                                                                                   .Include(s => s.Beatmaps).ThenInclude(b => b.BaseDifficulty)
                                                                                   .Include(s => s.Beatmaps).ThenInclude(b => b.Metadata)
                                                                                   .AsNoTracking();

        public IQueryable<BeatmapInfo> Beatmaps =>
            ContextFactory.Get().BeatmapInfo
                          .Include(b => b.BeatmapSet).ThenInclude(s => s.Metadata)
                          .Include(b => b.BeatmapSet).ThenInclude(s => s.Files).ThenInclude(f => f.FileInfo)
                          .Include(b => b.Metadata)
                          .Include(b => b.Ruleset)
                          .Include(b => b.BaseDifficulty);

        public IQueryable<BeatmapInfo> BeatmapsMinimal =>
            ContextFactory.Get().BeatmapInfo;
    }
}
