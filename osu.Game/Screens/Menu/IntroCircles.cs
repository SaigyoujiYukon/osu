﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Game.Screens.Backgrounds;

namespace osu.Game.Screens.Menu
{
    public class IntroCircles : IntroScreen
    {
        protected override string BeatmapHash => "3c8b1fcc9434dbb29e2fb613d3b9eada9d7bb6c125ceb32396c3b53437280c83";

        protected override string BeatmapFile => "circles.osz";

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenPureColor();

        private const double delay_step_one = 2300;
        private const double delay_step_two = 600;

        private Sample welcome;
        private readonly bool useTranslate;

        public IntroCircles([CanBeNull] Func<MainMenu> createNextScreen = null, bool useTranslate = false)
            : base(createNextScreen)
        {
            this.useTranslate = useTranslate;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            if (MenuVoice.Value)
                welcome = audio.Samples.Get(@"Intro/welcome");
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                welcome?.Play();

                Scheduler.AddDelayed(delegate
                {
                    StartTrack();

                    PrepareMenuLoad();

                    Scheduler.AddDelayed(LoadMenu, delay_step_one);
                }, delay_step_two);

                logo.ScaleTo(1);
                logo.FadeIn();
                logo.PlayIntro(useTranslate);
            }
        }

        public override void OnSuspending(IScreen next)
        {
            this.FadeOut(300);
            base.OnSuspending(next);
        }
    }
}
