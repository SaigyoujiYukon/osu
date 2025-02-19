// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays.Movers;
using osu.Game.Rulesets.Osu.UI;
using osuTK;
using static osu.Game.Rulesets.Osu.Replays.Movers.MoverUtilExtensions;

namespace osu.Game.Rulesets.Osu.Replays
{
    public class OsuDanceAutoGenerator : OsuAutoGeneratorBase
    {
        public static Mover GetMover(OsuDanceMover mover) =>
            mover switch
            {
                OsuDanceMover.HalfCircle => new HalfCircleMover(),
                OsuDanceMover.Flower => new FlowerMover(),
                OsuDanceMover.Momentum => new MomentumMover(),
                OsuDanceMover.Pippi => new PippiMover(),
                OsuDanceMover.Aggresive => new AggressiveMover(),
                OsuDanceMover.AxisAligned => new AxisAlignedMover(),
                OsuDanceMover.Bezier => new BezierMover(),
                OsuDanceMover.Linear => new LinearMover(),
                _ => new MomentumMover()
            };

        public new OsuBeatmap Beatmap => (OsuBeatmap)base.Beatmap;
        private readonly bool[] objectsDuring;
        private readonly Mover mover;
        private readonly float spinRadiusStart;
        private readonly float spinRadiusEnd;
        private readonly bool sliderDance;
        private readonly bool pippiSpinner;
        private readonly bool pippiStream;
        private readonly bool skipShortSliders;
        private bool isStream;
        private readonly OsuRulesetConfigManager config;
        private readonly double frameDelay;
        private int buttonIndex;
        protected readonly bool IsPippi;

        public OsuDanceAutoGenerator(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(beatmap, mods)
        {
            config = OsuRulesetConfigManager.Instance;
            mover = GetMover(config.Get<OsuDanceMover>(OsuRulesetSetting.DanceMover));
            IsPippi = mover is PippiMover;

            frameDelay = 1000.0 / config.Get<float>(OsuRulesetSetting.ReplayFramerate);
            spinRadiusStart = config.Get<float>(OsuRulesetSetting.SpinnerRadiusStart);
            spinRadiusEnd = config.Get<float>(OsuRulesetSetting.SpinnerRadiusEnd);
            sliderDance = config.Get<bool>(OsuRulesetSetting.SliderDance);
            pippiSpinner = config.Get<bool>(OsuRulesetSetting.PippiSpinner) || IsPippi;
            pippiStream = config.Get<bool>(OsuRulesetSetting.PippiStream);
            mover = GetMover(config.Get<OsuDanceMover>(OsuRulesetSetting.DanceMover));
            skipShortSliders = config.Get<bool>(OsuRulesetSetting.SkipShortSlider);

            mover.Beatmap = Beatmap;
            mover.TimeAffectingMods = mods.OfType<IApplicableToRate>().ToList();

            var objectsDuring = new bool[Beatmap.HitObjects.Count];

            for (int i = 0; i < Beatmap.HitObjects.Count - 1; ++i)
            {
                var e = Beatmap.HitObjects[i].GetEndTime();
                objectsDuring[i] = false;

                for (int j = i + 1; j < Beatmap.HitObjects.Count; ++j)
                {
                    if (Beatmap.HitObjects[j].StartTime + 1 > e) continue;

                    objectsDuring[i] = true;
                    break;
                }
            }

            this.objectsDuring = objectsDuring;
            mover.ObjectsDuring = objectsDuring;
        }

        private OsuAction getAction(OsuHitObject h, OsuHitObject last)
        {
            double timeDifference = ApplyModsToTimeDelta(last.StartTime, h.StartTime);

            if (timeDifference > 0 && timeDifference < 266)
                buttonIndex++;
            else
                buttonIndex = 0;

            return buttonIndex % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton;
        }

        private void addHitObjectClickFrames(OsuHitObject h, int index)
        {
            OsuHitObject last = Beatmap.HitObjects[Math.Max(0, index - 1)];
            OsuAction action = getAction(h, last);
            Vector2 startPosition = h.StackedPosition;
            Vector2 difference = startPosition - SPINNER_CENTRE;
            float radius = difference.Length;
            float angle = radius == 0 ? 0 : MathF.Atan2(difference.Y, difference.X);
            Vector2 pos;

            switch (h)
            {
                case Slider slider:
                    AddFrameToReplay(new OsuReplayFrame(slider.StartTime, slider.StackedPosition, action));

                    if (objectsDuring[index]) break;

                    var points = slider.NestedHitObjects.SkipWhile(p => p is SliderRepeat).Cast<OsuHitObject>()
                                       .OrderBy(p => p.StartTime)
                                       .ToList();

                    var mid = slider.StackedEndPosition;

                    if (skipShortSliders && slider.RepeatCount == 1 || Math.Abs(Vector2.Distance(startPosition, mid)) <= h.Radius * 1.6)
                    {
                        mid = slider.Path.PositionAt(0.5);

                        if (Vector2.Distance(startPosition, mid) <= h.Radius * 1.6)
                        {
                            AddFrameToReplay(new OsuReplayFrame(slider.EndTime, mid, action));
                            mover.LastPos = mid;
                            return;
                        }
                    }

                    if (sliderDance && points.Count > 2)
                    {
                        for (int i = 0; i < points.Count - 1; i++)
                        {
                            var point = points[i];
                            var next = points[i + 1];
                            var duration = next.StartTime - point.StartTime;

                            for (double j = GetFrameDelay(point.StartTime); j < duration; j += GetFrameDelay(point.StartTime + j))
                            {
                                var scaleFactor = j / duration;
                                pos = point.StackedPosition + (next.StackedPosition - point.StackedPosition) * (float)scaleFactor;

                                addPippiFrame(new OsuReplayFrame(point.StartTime + j, pos, action), IsPippi ? -1 : 0);
                                mover.LastPos = pos;
                            }
                        }
                    }
                    else
                    {
                        for (double j = GetFrameDelay(slider.StartTime); j < slider.Duration; j += GetFrameDelay(slider.StartTime + j))
                        {
                            pos = slider.StackedPositionAt(j / slider.Duration);
                            addPippiFrame(new OsuReplayFrame(h.StartTime + j, pos, action), IsPippi ? -1 : 0);
                            mover.LastPos = pos;
                        }
                    }

                    break;

                case Spinner spinner:
                    if (spinner.SpinsRequired == 0) return;

                    double radiusStart = spinner.SpinsRequired > 3 ? spinRadiusStart : spinRadiusEnd;
                    double rEndTime = spinner.StartTime + spinner.Duration * 0.7;
                    double previousFrame = h.StartTime;

                    for (double nextFrame = h.StartTime + GetFrameDelay(h.StartTime); nextFrame < spinner.EndTime; nextFrame += ApplyModsToRate(nextFrame, frameDelay))
                    {
                        var t = ApplyModsToTimeDelta(previousFrame, nextFrame) * -1;
                        angle += (float)t / 20;
                        var r = nextFrame > rEndTime ? spinRadiusEnd : Interpolation.ValueAt(nextFrame, radiusStart, spinRadiusEnd, spinner.StartTime, rEndTime, Easing.In);
                        pos = SPINNER_CENTRE + CirclePosition(angle, r);
                        addPippiFrame(new OsuReplayFrame((int)nextFrame, pos, action), pippiSpinner ? (float)r : 0);

                        previousFrame = nextFrame;
                        mover.LastPos = pos;
                    }

                    break;

                default:
                    isStream = pippiStream && IsStream(last, h, (OsuHitObject)GetNextObject(index)) && !(mover is PippiMover);
                    addPippiFrame(new OsuReplayFrame(h.StartTime, mover.Update(h.StartTime), action), isStream ? -1 : 0);
                    break;
            }
        }

        public override Replay Generate()
        {
            OsuHitObject h = Beatmap.HitObjects[0];
            AddFrameToReplay(new OsuReplayFrame(-10000, h.StackedPosition));

            Vector2 baseSize = OsuPlayfield.BASE_SIZE;

            float xf = baseSize.X / 0.8f * (4f / 3f);
            float x0 = (baseSize.X - xf) / 2f;
            float x1 = xf + x0;

            float yf = baseSize.Y / 0.8f;
            float y0 = (baseSize.Y - yf) / 2f;
            float y1 = yf + y0;
            mover.OnObjChange();

            for (int i = 0; i < Beatmap.HitObjects.Count - 1; i++)
            {
                h = Beatmap.HitObjects[i];
                addHitObjectClickFrames(h, i);

                mover.ObjectIndex = i;
                mover.OnObjChange();

                for (double time = (objectsDuring[i] ? h.StartTime : h.GetEndTime()) + frameDelay; time < mover.End.StartTime; time += frameDelay)
                {
                    var currentPosition = ApplyPippiOffset(mover.Update(time), time, isStream ? -1 : 0);

                    if (config.Get<bool>(OsuRulesetSetting.BorderBounce))
                    {
                        if (currentPosition.X < x0) currentPosition.X = x0 - (currentPosition.X - x0);
                        if (currentPosition.Y < y0) currentPosition.Y = y0 - (currentPosition.Y - y0);

                        if (currentPosition.X > x1)
                        {
                            float x = currentPosition.X - x0;
                            int m = (int)(x / xf);
                            x %= xf;
                            x = m % 2 == 0 ? x : xf - x;
                            currentPosition.X = x + x0;
                        }

                        if (currentPosition.Y > y1)
                        {
                            float y = currentPosition.Y - y0;
                            float m = (int)(y / yf);
                            y %= yf;
                            y = m % 2 == 0 ? y : yf - y;
                            currentPosition.Y = y + y0;
                        }
                    }

                    AddFrameToReplay(new OsuReplayFrame(time, currentPosition));
                }
            }

            addHitObjectClickFrames(Beatmap.HitObjects[^1], Beatmap.HitObjects.Count - 1);

            AddFrameToReplay(new OsuReplayFrame(Beatmap.HitObjects[^1].GetEndTime(), Beatmap.HitObjects[^1].StackedEndPosition));

            return Replay;
        }

        protected override HitObject GetNextObject(int currentIndex)
        {
            var next = Beatmap.HitObjects[Math.Min(Beatmap.HitObjects.Count - 1, currentIndex + 1)];

            while (next is Spinner { SpinsRequired: 0 } && currentIndex < Beatmap.HitObjects.Count - 1)
            {
                next = Beatmap.HitObjects[++currentIndex];
            }

            return next;
        }

        private void addPippiFrame(OsuReplayFrame frame, float radius)
        {
            frame.Position = ApplyPippiOffset(frame.Position, frame.Time, radius);
            AddFrameToReplay(frame);
        }
    }
}
