// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Game.Configuration
{
    public class MConfigManager : IniConfigManager<MSetting>
    {
        protected override string Filename => "mf.ini";

        public MConfigManager(Storage storage)
            : base(storage)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();

            //Other Settings
            SetDefault(MSetting.UseSayobot, true);

            //UI Settings
            SetDefault(MSetting.OptUI, true);
            SetDefault(MSetting.TrianglesEnabled, true);
            SetDefault(MSetting.SongSelectBgBlur, 0.2f, 0f, 1f);
            SetDefault(MSetting.AlwaysHideTextIndicator, false);

            //Intro Settings
            SetDefault(MSetting.IntroLoadDirectToSongSelect, false);

            //Gameplay Settings
            SetDefault(MSetting.SamplePlaybackGain, 1f, 0f, 20f);

            //MvisSettings
            SetDefault(MSetting.MvisContentAlpha, 1f, 0f, 1f);
            SetDefault(MSetting.MvisBgBlur, 0.2f, 0f, 1f);
            SetDefault(MSetting.MvisStoryboardProxy, false);
            SetDefault(MSetting.MvisIdleBgDim, 0.8f, 0f, 1f);
            SetDefault(MSetting.MvisEnableBgTriangles, true);
            SetDefault(MSetting.MvisAdjustMusicWithFreq, true);
            SetDefault(MSetting.MvisMusicSpeed, 1.0, 0.1, 2.0);
            SetDefault(MSetting.MvisEnableNightcoreBeat, false);
            SetDefault(MSetting.MvisPlayFromCollection, false);
            SetDefault(MSetting.MvisInterfaceRed, value: 0, 0, 255f);
            SetDefault(MSetting.MvisInterfaceGreen, value: 119f, 0, 255f);
            SetDefault(MSetting.MvisInterfaceBlue, value: 255f, 0, 255f);
            SetDefault(MSetting.MvisCurrentAudioProvider, "osu.Game.Screens.Mvis.Plugins+OsuMusicControllerWrapper");

            //实验性功能
            SetDefault(MSetting.CustomWindowIconPath, "");
            SetDefault(MSetting.UseCustomGreetingPicture, false);
            SetDefault(MSetting.FadeOutWindowWhenExiting, false);
            SetDefault(MSetting.FadeInWindowWhenEntering, false);
            SetDefault(MSetting.UseSystemCursor, false);
            SetDefault(MSetting.PreferredFont, "Torus");

            //已分离
            SetDefault(MSetting.MvisEnableStoryboard, true);
            SetDefault(MSetting.MvisEnableFakeEditor, false);

            //Mvis Settings(Upstream; 已分离)
            SetDefault(MSetting.MvisEnableRulesetPanel, true);
            SetDefault(MSetting.MvisParticleAmount, 350, 0, 350);
            SetDefault(MSetting.MvisShowParticles, true);
            SetDefault(MSetting.MvisBarType, MvisBarType.Rounded);
            SetDefault(MSetting.MvisVisualizerAmount, 3, 1, 5);
            SetDefault(MSetting.MvisBarWidth, 3.0, 1, 20);
            SetDefault(MSetting.MvisBarsPerVisual, 120, 1, 200);
            SetDefault(MSetting.MvisRotation, 0, 0, 359);
            SetDefault(MSetting.MvisUseCustomColour, false);
            SetDefault(MSetting.MvisRed, 0, 0, 255);
            SetDefault(MSetting.MvisGreen, 0, 0, 255);
            SetDefault(MSetting.MvisBlue, 0, 0, 255);
            SetDefault(MSetting.MvisUseOsuLogoVisualisation, false);
        }
    }

    public enum MSetting
    {
        OptUI,
        TrianglesEnabled,
        UseSayobot,
        MvisBgBlur,
        MvisEnableStoryboard,
        MvisStoryboardProxy,
        MvisIdleBgDim,
        MvisContentAlpha,
        MvisEnableBgTriangles,
        MvisMusicSpeed,
        MvisAdjustMusicWithFreq,
        MvisEnableNightcoreBeat,
        MvisPlayFromCollection,
        MvisInterfaceRed,
        MvisInterfaceGreen,
        MvisInterfaceBlue,
        MvisEnableFakeEditor,
        SamplePlaybackGain,
        SongSelectBgBlur,
        IntroLoadDirectToSongSelect,
        CustomWindowIconPath,
        UseCustomGreetingPicture,
        FadeOutWindowWhenExiting,
        FadeInWindowWhenEntering,
        UseSystemCursor,
        PreferredFont,
        AlwaysHideTextIndicator,
        MvisCurrentAudioProvider,

        //已分离(MvisPanel)
        MvisEnableRulesetPanel,
        MvisParticleAmount,
        MvisShowParticles,
        MvisVisualizerAmount,
        MvisBarWidth,
        MvisBarsPerVisual,
        MvisBarType,
        MvisRotation,
        MvisUseCustomColour,
        MvisRed,
        MvisGreen,
        MvisBlue,
        MvisUseOsuLogoVisualisation,
    }

    //已分离(MvisPanel)
    public enum MvisBarType
    {
        [Description("基本")]
        Basic,

        [Description("圆角")]
        Rounded,

        [Description("打砖块")]
        Fall
    }
}
