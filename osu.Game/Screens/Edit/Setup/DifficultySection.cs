// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Setup
{
    internal class DifficultySection : SetupSection
    {
        private LabelledSliderBar<float> circleSizeSlider;
        private LabelledSliderBar<float> healthDrainSlider;
        private LabelledSliderBar<float> approachRateSlider;
        private LabelledSliderBar<float> overallDifficultySlider;

        public override LocalisableString Title => "难度设置";

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                circleSizeSlider = new LabelledSliderBar<float>
                {
                    Label = "物件大小(CS)",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "这将决定所有物件的大小, 数值越大, 物件越小",
                    Current = new BindableFloat(Beatmap.Difficulty.CircleSize)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                healthDrainSlider = new LabelledSliderBar<float>
                {
                    Label = "掉血速度(HP)",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "这将决定掉血的速度, 数值越大, 掉血越快",
                    Current = new BindableFloat(Beatmap.Difficulty.DrainRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                approachRateSlider = new LabelledSliderBar<float>
                {
                    Label = "缩圈速度(AR)",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "这将决定缩圈速度, 数值越大, 缩圈越快",
                    Current = new BindableFloat(Beatmap.Difficulty.ApproachRate)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
                overallDifficultySlider = new LabelledSliderBar<float>
                {
                    Label = "整体难度(OD)",
                    FixedLabelWidth = LABEL_WIDTH,
                    Description = "这将决定该谱面的整体难度(准确率要求, 转盘难度等), 数值越大, 难度越高",
                    Current = new BindableFloat(Beatmap.Difficulty.OverallDifficulty)
                    {
                        Default = BeatmapDifficulty.DEFAULT_DIFFICULTY,
                        MinValue = 0,
                        MaxValue = 10,
                        Precision = 0.1f,
                    }
                },
            };

            foreach (var item in Children.OfType<LabelledSliderBar<float>>())
                item.Current.ValueChanged += onValueChanged;
        }

        private void onValueChanged(ValueChangedEvent<float> args)
        {
            // for now, update these on commit rather than making BeatmapMetadata bindables.
            // after switching database engines we can reconsider if switching to bindables is a good direction.
            Beatmap.Difficulty.CircleSize = circleSizeSlider.Current.Value;
            Beatmap.Difficulty.DrainRate = healthDrainSlider.Current.Value;
            Beatmap.Difficulty.ApproachRate = approachRateSlider.Current.Value;
            Beatmap.Difficulty.OverallDifficulty = overallDifficultySlider.Current.Value;

            Beatmap.UpdateAllHitObjects();
        }
    }
}
