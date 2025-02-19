﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Configuration;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaRuleset : DrawableScrollingRuleset<ManiaHitObject>
    {
        /// <summary>
        /// The minimum time range. This occurs at a <see cref="relativeTimeRange"/> of 40.
        /// </summary>
        public const double MIN_TIME_RANGE = 290;

        /// <summary>
        /// The maximum time range. This occurs at a <see cref="relativeTimeRange"/> of 1.
        /// </summary>
        public const double MAX_TIME_RANGE = 11485;

        protected new ManiaPlayfield Playfield => (ManiaPlayfield)base.Playfield;

        public new ManiaBeatmap Beatmap => (ManiaBeatmap)base.Beatmap;

        public IEnumerable<BarLine> BarLines;

        protected override bool RelativeScaleBeatLengths => true;

        protected new ManiaRulesetConfigManager Config => (ManiaRulesetConfigManager)base.Config;

        public ScrollVisualisationMethod ScrollMethod
        {
            get => scrollMethod;
            set
            {
                if (IsLoaded)
                    throw new InvalidOperationException($"Can't alter {nameof(ScrollMethod)} after ruleset is already loaded");

                scrollMethod = value;
            }
        }

        private ScrollVisualisationMethod scrollMethod = ScrollVisualisationMethod.Sequential;

        protected override ScrollVisualisationMethod VisualisationMethod => scrollMethod;

        private readonly Bindable<ManiaScrollingDirection> configDirection = new Bindable<ManiaScrollingDirection>();
        private readonly BindableDouble configTimeRange = new BindableDouble();

        // Stores the current speed adjustment active in gameplay.
        private readonly Track speedAdjustmentTrack = new TrackVirtual(0);

        public DrawableManiaRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
            BarLines = new BarLineGenerator<BarLine>(Beatmap).BarLines;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var mod in Mods.OfType<IApplicableToTrack>())
                mod.ApplyToTrack(speedAdjustmentTrack);

            bool isForCurrentRuleset = Beatmap.BeatmapInfo.Ruleset.Equals(Ruleset.RulesetInfo);

            foreach (var p in ControlPoints)
            {
                // Mania doesn't care about global velocity
                p.Velocity = 1;
                p.BaseBeatLength *= Beatmap.Difficulty.SliderMultiplier;

                // For non-mania beatmap, speed changes should only happen through timing points
                if (!isForCurrentRuleset)
                    p.EffectPoint = new EffectControlPoint();
            }

            BarLines.ForEach(Playfield.Add);

            Config.BindWith(ManiaRulesetSetting.ScrollDirection, configDirection);
            configDirection.BindValueChanged(direction => Direction.Value = (ScrollingDirection)direction.NewValue, true);

            Config.BindWith(ManiaRulesetSetting.ScrollTime, configTimeRange);
            TimeRange.MinValue = configTimeRange.MinValue;
            TimeRange.MaxValue = configTimeRange.MaxValue;
        }

        protected override void AdjustScrollSpeed(int amount)
        {
            this.TransformTo(nameof(relativeTimeRange), relativeTimeRange + amount, 200, Easing.OutQuint);
        }

        private double relativeTimeRange
        {
            get => MAX_TIME_RANGE / configTimeRange.Value;
            set => configTimeRange.Value = MAX_TIME_RANGE / value;
        }

        protected override void Update()
        {
            base.Update();

            updateTimeRange();
        }

        private void updateTimeRange() => TimeRange.Value = configTimeRange.Value * speedAdjustmentTrack.AggregateTempo.Value * speedAdjustmentTrack.AggregateFrequency.Value;

        public override PlayfieldAdjustmentContainer CreatePlayfieldAdjustmentContainer() => new ManiaPlayfieldAdjustmentContainer();

        protected override Playfield CreatePlayfield() => new ManiaPlayfield(Beatmap.Stages);

        public override int Variant => (int)(Beatmap.Stages.Count == 1 ? PlayfieldType.Single : PlayfieldType.Dual) + Beatmap.TotalColumns;

        protected override PassThroughInputManager CreateInputManager() => new ManiaInputManager(Ruleset.RulesetInfo, Variant);

        public override DrawableHitObject<ManiaHitObject> CreateDrawableRepresentation(ManiaHitObject h) => null;

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => new ManiaFramedReplayInputHandler(replay);

        protected override ReplayRecorder CreateReplayRecorder(Score score) => new ManiaReplayRecorder(score);
    }
}