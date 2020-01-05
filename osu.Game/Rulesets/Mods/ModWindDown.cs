// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindDown : ModTimeRamp
    {
        public override string Name => "Wind Down";
        public override string Acronym => "WD";
        public override string Description => "越~来~~越~~~慢~~~~";
        public override IconUsage Icon => FontAwesome.Solid.ChevronCircleDown;
        public override double ScoreMultiplier => 1.0;

        [SettingSource("Final rate", "The speed increase to ramp towards")]
        public override BindableNumber<double> FinalRate { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 0.99,
            Default = 0.75,
            Value = 0.75,
            Precision = 0.01,
        };

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModWindUp)).ToArray();
    }
}
