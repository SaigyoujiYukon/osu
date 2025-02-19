// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindUp : ModTimeRamp
    {
        public override string Name => "递增";
        public override string Acronym => "WU";
        public override string Description => "你能坚持住吗?";
        public override IconUsage? Icon => FontAwesome.Solid.ChevronCircleUp;
        public override double ScoreMultiplier => 1.0;

        [SettingSource("初始速度", "歌曲的初始速度")]
        public override BindableNumber<double> InitialRate { get; } = new BindableDouble
        {
            MinValue = 0.5,
            MaxValue = 1.99,
            Default = 1,
            Value = 1,
            Precision = 0.01,
        };

        [SettingSource("最终速度", "歌曲的最终速度")]
        public override BindableNumber<double> FinalRate { get; } = new BindableDouble
        {
            MinValue = 0.51,
            MaxValue = 2,
            Default = 1.5,
            Value = 1.5,
            Precision = 0.01,
        };

        [SettingSource("启用变调", "是否要更随速度调整音调")]
        public override BindableBool AdjustPitch { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModWindDown)).ToArray();

        public ModWindUp()
        {
            InitialRate.BindValueChanged(val =>
            {
                if (val.NewValue >= FinalRate.Value)
                    FinalRate.Value = val.NewValue + FinalRate.Precision;
            });

            FinalRate.BindValueChanged(val =>
            {
                if (val.NewValue <= InitialRate.Value)
                    InitialRate.Value = val.NewValue - InitialRate.Precision;
            });
        }
    }
}
