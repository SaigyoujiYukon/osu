﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Utils;

namespace osu.Game.Graphics
{
    public class DrawableDate : OsuSpriteText, IHasTooltip
    {
        private DateTimeOffset date;

        public DateTimeOffset Date
        {
            get => date;
            set
            {
                if (date == value)
                    return;

                date = value.ToLocalTime();

                if (LoadState >= LoadState.Ready)
                    updateTime();
            }
        }

        public DrawableDate(DateTimeOffset date)
        {
            Font = OsuFont.GetFont(weight: FontWeight.Regular, italics: true);
            Date = date;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            updateTime();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.Add(updateTimeWithReschedule);
        }

        private void updateTimeWithReschedule()
        {
            updateTime();

            var diffToNow = DateTimeOffset.Now.Subtract(Date);

            double timeUntilNextUpdate = 1000;

            if (Math.Abs(diffToNow.TotalSeconds) > 120)
            {
                timeUntilNextUpdate *= 60;

                if (Math.Abs(diffToNow.TotalMinutes) > 120)
                {
                    timeUntilNextUpdate *= 60;

                    if (Math.Abs(diffToNow.TotalHours) > 48)
                        timeUntilNextUpdate *= 24;
                }
            }

            Scheduler.AddDelayed(updateTimeWithReschedule, timeUntilNextUpdate);
        }

        protected virtual string Format() => HumanizerUtils.Humanize(Date);

        private void updateTime() => Text = Format();

        public virtual string TooltipText => string.Format($"{Date:yyyy MMMM d h:mm tt \"UTC\"z}");
    }
}
