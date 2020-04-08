﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Utils;
using osu.Game.Screens.Menu;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using IntroSequence = osu.Game.Configuration.IntroSequence;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osu.Framework.Platform;

namespace osu.Game.Screens
{
    public class Loader : StartupScreen
    {
        private bool showDisclaimer;

        public Loader()
        {
            ValidForResume = false;
        }

        private OsuScreen loadableScreen;
        private ShaderPrecompiler precompiler;

        private IntroSequence introSequence;
        private Container logoContainer;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;

        protected virtual OsuScreen CreateLoadableScreen()
        {
            if (showDisclaimer)
                return new Disclaimer(getIntroSequence());

            return getIntroSequence();
        }

        private IntroScreen getIntroSequence()
        {
            if (introSequence == IntroSequence.Random)
                introSequence = (IntroSequence)RNG.Next(0, (int)IntroSequence.Random);

            switch (introSequence)
            {
                case IntroSequence.Circles:
                    return new IntroCircles();

                case IntroSequence.CirclesCN:
                    return new IntroCirclesCN();

                case IntroSequence.TrianglesCN:
                    return new IntroTrianglesCN();

                default:
                    return new IntroTriangles();
            }
        }

        protected virtual ShaderPrecompiler CreateShaderPrecompiler() => new ShaderPrecompiler();

        private TextureStore textures;
        private LoaderStorage LoaderStorage;

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            LoadComponentAsync(precompiler = CreateShaderPrecompiler(), AddInternal);
            LoadComponentAsync(loadableScreen = CreateLoadableScreen());

            LoadComponentAsync(spinner = new LoadingSpinner(true, true)
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Margin = new MarginPadding(40),
            }, _ =>
            {
                AddInternal(spinner);
                spinnerShow = Scheduler.AddDelayed(spinner.Show, 200);
            });

            LoadComponentAsync(logoContainer = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    new Sprite
                    {
                        Size = new Vector2(400),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Texture = textures.Get("avatarlogo"),
                    }
                }
            }, _ => 
            {
                AddInternal(logoContainer);
                Scheduler.AddDelayed(logoContainer.Show, 0);
            });

            checkIfLoaded();
        }

        private void checkIfLoaded()
        {
            if (loadableScreen.LoadState != LoadState.Ready || !precompiler.FinishedCompiling)
            {
                Schedule(checkIfLoaded);
                return;
            }

            spinnerShow?.Cancel();

            if (spinner.State.Value == Visibility.Visible)
            {
                spinner.Hide();
                logoContainer.Delay(1000).FadeOut(LoadingSpinner.TRANSITION_DURATION, Easing.OutQuint);
                Scheduler.AddDelayed(() => this.Push(loadableScreen), LoadingSpinner.TRANSITION_DURATION + 1000);
            }
            else
            {
                logoContainer.FadeTo(1).Then().Delay(1000).FadeOut(LoadingSpinner.TRANSITION_DURATION, Easing.OutQuint);
                Scheduler.AddDelayed(() => this.Push(loadableScreen), LoadingSpinner.TRANSITION_DURATION + 1000);
            }
        }

        private DependencyContainer dependencies;
        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuConfigManager config, TextureStore textures, Storage storage)
        {
            this.textures = textures;
            dependencies.CacheAs(LoaderStorage = new LoaderStorage(storage));
            textures.AddStore(new TextureLoaderStore(LoaderStorage));
            showDisclaimer = game.IsDeployedBuild;
            introSequence = config.Get<IntroSequence>(OsuSetting.IntroSequence);
        }

        /// <summary>
        /// Compiles a set of shaders before continuing. Attempts to draw some frames between compilation by limiting to one compile per draw frame.
        /// </summary>
        public class ShaderPrecompiler : Drawable
        {
            private readonly List<IShader> loadTargets = new List<IShader>();

            public bool FinishedCompiling { get; private set; }

            [BackgroundDependencyLoader]
            private void load(ShaderManager manager)
            {
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE_ROUNDED));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.BLUR));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(@"CursorTrail", FragmentShaderDescriptor.TEXTURE));

                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE_ROUNDED));
                loadTargets.Add(manager.Load(VertexShaderDescriptor.TEXTURE_3, FragmentShaderDescriptor.TEXTURE));
            }

            protected virtual bool AllLoaded => loadTargets.All(s => s.IsLoaded);

            protected override void Update()
            {
                base.Update();

                // if our target is null we are done.
                if (AllLoaded)
                {
                    FinishedCompiling = true;
                    Expire();
                }
            }
        }
    }
}
