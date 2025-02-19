// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A drawable which can be skinned via an <see cref="ISkinSource"/>.
    /// </summary>
    public class SkinnableDrawable : SkinReloadableDrawable
    {
        /// <summary>
        /// The displayed component.
        /// </summary>
        public Drawable Drawable { get; private set; }

        /// <summary>
        /// Whether the drawable component should be centered in available space.
        /// Defaults to true.
        /// </summary>
        public bool CentreComponent = true;

        public Anchor ChildAnchor = Anchor.TopLeft;
        public Anchor ChildOrigin = Anchor.TopLeft;
        public bool OverrideChildAnchor;

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        private readonly ISkinComponent component;

        private readonly ConfineMode confineMode;

        /// <summary>
        /// Create a new skinnable drawable.
        /// </summary>
        /// <param name="component">The namespace-complete resource name for this skinnable element.</param>
        /// <param name="defaultImplementation">A function to create the default skin implementation of this element.</param>
        /// <param name="confineMode">How (if at all) the <see cref="Drawable"/> should be resize to fit within our own bounds.</param>
        public SkinnableDrawable(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : this(component, confineMode)
        {
            createDefault = defaultImplementation;
        }

        protected SkinnableDrawable(ISkinComponent component, ConfineMode confineMode = ConfineMode.NoScaling)
        {
            this.component = component;
            this.confineMode = confineMode;

            RelativeSizeAxes = Axes.Both;
        }

        /// <summary>
        /// Seeks to the 0-th frame if the content of this <see cref="SkinnableDrawable"/> is an <see cref="IFramedAnimation"/>.
        /// </summary>
        public void ResetAnimation() => (Drawable as IFramedAnimation)?.GotoFrame(0);

        private readonly Func<ISkinComponent, Drawable> createDefault;

        private readonly Cached scaling = new Cached();

        private bool isDefault;

        protected virtual Drawable CreateDefault(ISkinComponent component) => createDefault?.Invoke(component) ?? Empty();

        /// <summary>
        /// Whether to apply size restrictions (specified via <see cref="confineMode"/>) to the default implementation.
        /// </summary>
        protected virtual bool ApplySizeRestrictionsToDefault => false;

        protected override void SkinChanged(ISkinSource skin)
        {
            Drawable = skin.GetDrawableComponent(component);

            isDefault = false;

            if (Drawable == null)
            {
                Drawable = CreateDefault(component);
                isDefault = true;
            }

            if (Drawable != null)
            {
                scaling.Invalidate();

                if (OverrideChildAnchor)
                {
                    ChildAnchor = CentreComponent ? Anchor.Centre : ChildAnchor;
                    ChildOrigin = CentreComponent ? Anchor.Centre : ChildOrigin;

                    Drawable.Anchor = ChildAnchor;
                    Drawable.Origin = ChildOrigin;
                }

                if (CentreComponent)
                {
                    Drawable.Origin = Anchor.Centre;
                    Drawable.Anchor = Anchor.Centre;
                }

                InternalChild = Drawable;
            }
            else
                ClearInternal();
        }

        protected override void Update()
        {
            base.Update();

            if (!scaling.IsValid)
            {
                try
                {
                    if (Drawable == null || (isDefault && !ApplySizeRestrictionsToDefault)) return;

                    switch (confineMode)
                    {
                        case ConfineMode.ScaleToFit:
                            Drawable.RelativeSizeAxes = Axes.Both;
                            Drawable.Size = Vector2.One;
                            Drawable.Scale = Vector2.One;
                            Drawable.FillMode = FillMode.Fit;
                            break;

                        case ConfineMode.ScaleToFill:
                            Drawable.RelativeSizeAxes = Axes.Both;
                            Drawable.Size = Vector2.One;
                            Drawable.Scale = Vector2.One;
                            Drawable.FillMode = FillMode.Fill;
                            break;
                    }
                }
                finally
                {
                    scaling.Validate();
                }
            }
        }
    }

    public enum ConfineMode
    {
        /// <summary>
        /// Don't apply any scaling. This allows the user element to be of any size, exceeding specified bounds.
        /// </summary>
        NoScaling,
        ScaleToFit,
        ScaleToFill
    }
}
