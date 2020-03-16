// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Chat;
using osuTK.Graphics;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Display a chat channel in an insolated region.
    /// </summary>
    public class StandAloneChatDisplay : CompositeDrawable
    {
        public readonly Bindable<Channel> Channel = new Bindable<Channel>();

        private readonly FocusedTextBox textbox;

        protected ChannelManager ChannelManager;

        private DrawableChannel drawableChannel;

        private readonly bool postingTextbox;

        private const float textbox_height = 30;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="postingTextbox">Whether a textbox for posting new messages should be displayed.</param>
        public StandAloneChatDisplay(bool postingTextbox = false)
        {
            this.postingTextbox = postingTextbox;
            CornerRadius = 10;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.Black,
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both
                },
            };

            if (postingTextbox)
            {
                AddInternal(textbox = new FocusedTextBox
                {
                    RelativeSizeAxes = Axes.X,
                    Height = textbox_height,
                    PlaceholderText = "在这里输入你要发送的消息",
                    OnCommit = postMessage,
                    ReleaseFocusOnCommit = false,
                    HoldFocus = true,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                });
            }

            Channel.BindValueChanged(channelChanged);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChannelManager manager)
        {
            if (ChannelManager == null)
                ChannelManager = manager;
        }

        private void postMessage(TextBox sender, bool newtext)
        {
            var text = textbox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text[0] == '/')
                ChannelManager?.PostCommand(text.Substring(1), Channel.Value);
            else
                ChannelManager?.PostMessage(text, target: Channel.Value);

            textbox.Text = string.Empty;
        }

        protected virtual ChatLine CreateMessage(Message message) => new StandAloneMessage(message);

        private void channelChanged(ValueChangedEvent<Channel> e)
        {
            drawableChannel?.Expire();

            if (e.NewValue == null) return;

            AddInternal(drawableChannel = new StandAloneDrawableChannel(e.NewValue)
            {
                CreateChatLineAction = CreateMessage,
                Padding = new MarginPadding { Bottom = postingTextbox ? textbox_height : 0 }
            });
        }

        protected class StandAloneDrawableChannel : DrawableChannel
        {
            public Func<Message, ChatLine> CreateChatLineAction;

            protected override ChatLine CreateChatLine(Message m) => CreateChatLineAction(m);

            protected override DaySeparator CreateDaySeparator(DateTimeOffset time) => new CustomDaySeparator(time);

            public StandAloneDrawableChannel(Channel channel)
                : base(channel)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                ChatLineFlow.Padding = new MarginPadding { Horizontal = 0 };
            }

            private class CustomDaySeparator : DaySeparator
            {
                public CustomDaySeparator(DateTimeOffset time)
                    : base(time)
                {
                }

                [BackgroundDependencyLoader]
                private void load(OsuColour colours)
                {
                    Colour = colours.Yellow;
                    TextSize = 14;
                    LineHeight = 1;
                    Padding = new MarginPadding { Horizontal = 10 };
                    Margin = new MarginPadding { Vertical = 5 };
                }
            }
        }

        protected class StandAloneMessage : ChatLine
        {
            protected override float TextSize => 15;

            protected override float HorizontalPadding => 10;
            protected override float MessagePadding => 120;
            protected override float TimestampPadding => 50;

            public StandAloneMessage(Message message)
                : base(message)
            {
            }
        }
    }
}
