// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK.Graphics;

namespace osu.Game.Overlays.Chat
{
    public class DrawableChannel : Container
    {
        public readonly Channel Channel;
        protected FillFlowContainer ChatLineFlow;
        private ChannelScrollContainer scroll;

        private bool scrollbarVisible = true;

        public bool ScrollbarVisible
        {
            set
            {
                if (scrollbarVisible == value) return;

                scrollbarVisible = value;
                if (scroll != null)
                    scroll.ScrollbarVisible = value;
            }
        }

        [Resolved]
        private OsuColour colours { get; set; }

        public DrawableChannel(Channel channel)
        {
            Channel = channel;
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Child = scroll = new ChannelScrollContainer
                {
                    ScrollbarVisible = scrollbarVisible,
                    RelativeSizeAxes = Axes.Both,
                    // Some chat lines have effects that slightly protrude to the bottom,
                    // which we do not want to mask away, hence the padding.
                    Padding = new MarginPadding { Bottom = 5 },
                    Child = ChatLineFlow = new FillFlowContainer
                    {
                        Padding = new MarginPadding { Left = 20, Right = 20 },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                    }
                },
            };

            newMessagesArrived(Channel.Messages);

            Channel.NewMessagesArrived += newMessagesArrived;
            Channel.MessageRemoved += messageRemoved;
            Channel.PendingMessageResolved += pendingMessageResolved;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            Channel.NewMessagesArrived -= newMessagesArrived;
            Channel.MessageRemoved -= messageRemoved;
            Channel.PendingMessageResolved -= pendingMessageResolved;
        }

        protected virtual ChatLine CreateChatLine(Message m) => new ChatLine(m);

        protected virtual DaySeparator CreateDaySeparator(DateTimeOffset time) => new DaySeparator(time)
        {
            Margin = new MarginPadding { Vertical = 10 },
            Colour = colours.ChatBlue.Lighten(0.7f),
        };

        private void newMessagesArrived(IEnumerable<Message> newMessages) => Schedule(() =>
        {
            if (newMessages.Min(m => m.Id) < chatLines.Max(c => c.Message.Id))
            {
                // there is a case (on initial population) that we may receive past messages and need to reorder.
                // easiest way is to just combine messages and recreate drawables (less worrying about day separators etc.)
                newMessages = newMessages.Concat(chatLines.Select(c => c.Message)).OrderBy(m => m.Id).ToList();
                ChatLineFlow.Clear();
            }

            // Add up to last Channel.MAX_HISTORY messages
            var displayMessages = newMessages.Skip(Math.Max(0, newMessages.Count() - Channel.MAX_HISTORY));

            Message lastMessage = chatLines.LastOrDefault()?.Message;

            foreach (var message in displayMessages)
            {
                if (lastMessage == null || lastMessage.Timestamp.ToLocalTime().Date != message.Timestamp.ToLocalTime().Date)
                    ChatLineFlow.Add(CreateDaySeparator(message.Timestamp));

                ChatLineFlow.Add(CreateChatLine(message));
                lastMessage = message;
            }

            var staleMessages = chatLines.Where(c => c.LifetimeEnd == double.MaxValue).ToArray();
            int count = staleMessages.Length - Channel.MAX_HISTORY;

            if (count > 0)
            {
                void expireAndAdjustScroll(Drawable d)
                {
                    scroll.OffsetScrollPosition(-d.DrawHeight);
                    d.Expire();
                }

                for (int i = 0; i < count; i++)
                    expireAndAdjustScroll(staleMessages[i]);

                // remove all adjacent day separators after stale message removal
                for (int i = 0; i < ChatLineFlow.Count - 1; i++)
                {
                    if (!(ChatLineFlow[i] is DaySeparator)) break;
                    if (!(ChatLineFlow[i + 1] is DaySeparator)) break;

                    expireAndAdjustScroll(ChatLineFlow[i]);
                }
            }

            // due to the scroll adjusts from old messages removal above, a scroll-to-end must be enforced,
            // to avoid making the container think the user has scrolled back up and unwantedly disable auto-scrolling.
            if (newMessages.Any(m => m is LocalMessage))
                scroll.ScrollToEnd();
        });

        private void pendingMessageResolved(Message existing, Message updated) => Schedule(() =>
        {
            var found = chatLines.LastOrDefault(c => c.Message == existing);

            if (found != null)
            {
                Trace.Assert(updated.Id.HasValue, "An updated message was returned with no ID.");

                ChatLineFlow.Remove(found);
                found.Message = updated;
                ChatLineFlow.Add(found);
            }
        });

        private void messageRemoved(Message removed) => Schedule(() =>
        {
            chatLines.FirstOrDefault(c => c.Message == removed)?.FadeColour(Color4.Red, 400).FadeOut(600).Expire();
        });

        private IEnumerable<ChatLine> chatLines => ChatLineFlow.Children.OfType<ChatLine>();

        public class DaySeparator : Container
        {
            public float TextSize
            {
                get => text.Font.Size;
                set => text.Font = text.Font.With(size: value);
            }

            private float lineHeight = 2;

            public float LineHeight
            {
                get => lineHeight;
                set => lineHeight = leftBox.Height = rightBox.Height = value;
            }

            private readonly SpriteText text;
            private readonly Box leftBox;
            private readonly Box rightBox;

            public DaySeparator(DateTimeOffset time)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize), },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            leftBox = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                Height = lineHeight,
                            },
                            text = new OsuSpriteText
                            {
                                Margin = new MarginPadding { Horizontal = 10 },
                                Text = time.ToLocalTime().ToString("yyyy MM dd"),
                            },
                            rightBox = new Box
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                Height = lineHeight,
                            },
                        }
                    }
                };
            }
        }

        /// <summary>
        /// An <see cref="OsuScrollContainer"/> with functionality to automatically scroll whenever the maximum scrollable distance increases.
        /// </summary>
        private class ChannelScrollContainer : UserTrackingScrollContainer
        {
            /// <summary>
            /// The chat will be automatically scrolled to end if and only if
            /// the distance between the current scroll position and the end of the scroll
            /// is less than this value.
            /// </summary>
            private const float auto_scroll_leniency = 10f;

            private float? lastExtent;

            protected override void OnUserScroll(float value, bool animated = true, double? distanceDecay = default)
            {
                base.OnUserScroll(value, animated, distanceDecay);
                lastExtent = null;
            }

            protected override void Update()
            {
                base.Update();

                // If the user has scrolled to the bottom of the container, we should resume tracking new content.
                if (UserScrolling && IsScrolledToEnd(auto_scroll_leniency))
                    CancelUserScroll();

                // If the user hasn't overridden our behaviour and there has been new content added to the container, we should update our scroll position to track it.
                bool requiresScrollUpdate = !UserScrolling && (lastExtent == null || Precision.AlmostBigger(ScrollableExtent, lastExtent.Value));

                if (requiresScrollUpdate)
                {
                    // Schedule required to allow FillFlow to be the correct size.
                    Schedule(() =>
                    {
                        if (!UserScrolling)
                        {
                            if (Current < ScrollableExtent)
                                ScrollToEnd();
                            lastExtent = ScrollableExtent;
                        }
                    });
                }
            }
        }
    }
}
