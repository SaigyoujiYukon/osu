﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Comments
{
    public class CommentsHeader : CompositeDrawable
    {
        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        private readonly Box background;

        public CommentsHeader()
        {
            RelativeSizeAxes = Axes.X;
            Height = 40;

            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 50 },
                    Children = new Drawable[]
                    {
                        new OverlaySortTabControl<CommentsSortCriteria>
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Current = Sort
                        },
                        new ShowDeletedButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Checked = { BindTarget = ShowDeleted }
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            background.Colour = colourProvider.Background4;
        }

        private class ShowDeletedButton : HeaderButton
        {
            public readonly BindableBool Checked = new BindableBool();

            private readonly SpriteIcon checkboxIcon;
            private Sample sampleChecked;
            private Sample sampleUnchecked;

            public ShowDeletedButton()
            {
                Add(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Children = new Drawable[]
                    {
                        checkboxIcon = new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(10),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: 17),
                            Text = @"显示已删除的评论"
                        }
                    },
                });
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                sampleChecked = audio.Samples.Get(@"UI/check-on");
                sampleUnchecked = audio.Samples.Get(@"UI/check-off");
            }

            protected override void LoadComplete()
            {
                Checked.BindValueChanged(isChecked => checkboxIcon.Icon = isChecked.NewValue ? FontAwesome.Solid.CheckSquare : FontAwesome.Regular.Square, true);
                base.LoadComplete();
            }

            protected override bool OnClick(ClickEvent e)
            {
                Checked.Value = !Checked.Value;

                if (Checked.Value)
                    sampleChecked?.Play();
                else
                    sampleUnchecked?.Play();

                return true;
            }
        }
    }

    public enum CommentsSortCriteria
    {
        [System.ComponentModel.Description(@"最新")]
        New,

        [System.ComponentModel.Description(@"最旧")]
        Old,

        [System.ComponentModel.Description(@"热门")]
        Top
    }
}
