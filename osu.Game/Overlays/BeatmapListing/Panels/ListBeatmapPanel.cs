﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class ListBeatmapPanel : BeatmapPanel
    {
        private const float transition_duration = 120;
        private const float horizontal_padding = 10;
        private const float vertical_padding = 5;
        private const float height = 70;

        private FillFlowContainer statusContainer, titleContainer, artistContainer;
        protected BeatmapPanelDownloadButton DownloadButton;
        private PlayButton playButton;
        private Box progressBar;

        protected override bool FadePlayButton => false;

        protected override PlayButton PlayButton => playButton;
        protected override Box PreviewBar => progressBar;

        public ListBeatmapPanel(APIBeatmapSet beatmap)
            : base(beatmap)
        {
            RelativeSizeAxes = Axes.X;
            Height = height;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Content.CornerRadius = 5;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.25f), Color4.Black.Opacity(0.75f)),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding, Left = horizontal_padding, Right = vertical_padding },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            LayoutEasing = Easing.OutQuint,
                            LayoutDuration = transition_duration,
                            Spacing = new Vector2(10, 0),
                            Children = new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                playButton = new PlayButton(SetInfo)
                                                {
                                                    Origin = Anchor.CentreLeft,
                                                    Anchor = Anchor.CentreLeft,
                                                    Size = new Vector2(height / 3),
                                                    FillMode = FillMode.Fit,
                                                    Margin = new MarginPadding { Right = 10 },
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Direction = FillDirection.Vertical,
                                                    Children = new Drawable[]
                                                    {
                                                        titleContainer = new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Direction = FillDirection.Horizontal,
                                                            Children = new[]
                                                            {
                                                                new OsuSpriteText
                                                                {
                                                                    Text = new RomanisableString(SetInfo.TitleUnicode, SetInfo.Title),
                                                                    Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true)
                                                                },
                                                            }
                                                        },
                                                        artistContainer = new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.Both,
                                                            Direction = FillDirection.Horizontal,
                                                            Children = new[]
                                                            {
                                                                new OsuSpriteText
                                                                {
                                                                    Text = new RomanisableString(SetInfo.ArtistUnicode, SetInfo.Artist),
                                                                    Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true)
                                                                },
                                                            },
                                                        },
                                                    }
                                                },
                                            }
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Children = new Drawable[]
                                            {
                                                statusContainer = new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.Both,
                                                    Margin = new MarginPadding { Vertical = vertical_padding, Horizontal = 5 },
                                                    Spacing = new Vector2(5),
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    Height = 20,
                                                    Margin = new MarginPadding { Top = vertical_padding, Bottom = vertical_padding },
                                                    Spacing = new Vector2(3),
                                                    Children = GetDifficultyIcons(colours),
                                                },
                                            },
                                        },
                                    },
                                },
                            }
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Child = DownloadButton = new BeatmapPanelDownloadButton(SetInfo)
                                    {
                                        Size = new Vector2(height - vertical_padding * 3),
                                        Margin = new MarginPadding { Left = vertical_padding * 2, Right = vertical_padding },
                                    },
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new Statistic(FontAwesome.Solid.PlayCircle, SetInfo.PlayCount),
                                        new Statistic(FontAwesome.Solid.Heart, SetInfo.FavouriteCount),
                                        new LinkFlowContainer(s =>
                                        {
                                            s.Shadow = false;
                                            s.Font = OsuFont.GetFont(size: 15);
                                        })
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            AutoSizeAxes = Axes.Both,
                                        }.With(d =>
                                        {
                                            d.AutoSizeAxes = Axes.Both;
                                            d.AddText("谱师: ");
                                            d.AddUserLink(SetInfo.Author);
                                        }),
                                        new OsuSpriteText
                                        {
                                            Text = SetInfo.Source,
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            Font = OsuFont.GetFont(size: 14),
                                            Alpha = string.IsNullOrEmpty(SetInfo.Source) ? 0f : 1f,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                progressBar = new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    BypassAutoSizeAxes = Axes.Y,
                    Size = new Vector2(0, 3),
                    Alpha = 0,
                    Colour = colours.Yellow,
                },
            });

            if (SetInfo.HasExplicitContent)
            {
                titleContainer.Add(new ExplicitContentBeatmapPill
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10f, Top = 2f },
                });
            }

            if (SetInfo.TrackId != null)
            {
                artistContainer.Add(new FeaturedArtistBeatmapPill
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10f, Top = 2f },
                });
            }

            if (SetInfo.HasVideo)
            {
                statusContainer.Add(new IconPill(FontAwesome.Solid.Film) { IconSize = new Vector2(20) });
            }

            if (SetInfo.HasStoryboard)
            {
                statusContainer.Add(new IconPill(FontAwesome.Solid.Image) { IconSize = new Vector2(20) });
            }

            statusContainer.Add(new BeatmapSetOnlineStatusPill
            {
                AutoSizeAxes = Axes.Both,
                TextSize = 12,
                TextPadding = new MarginPadding { Horizontal = 10, Vertical = 4 },
                Status = SetInfo.Status,
            });
        }
    }
}