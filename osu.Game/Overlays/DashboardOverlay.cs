// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays.Dashboard;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Users;

namespace osu.Game.Overlays
{
    public class DashboardOverlay : FullscreenOverlay
    {
        private readonly Bindable<User> localUser = new Bindable<User>();

        [Resolved]
        private IAPIProvider api { get; set; }

        private CancellationTokenSource cancellationToken;

        private readonly Box background;
        private readonly DashboardOverlayHeader header;
        private readonly Container<DashboardDisplay> content;
        private readonly LoadingLayer loading;

        public DashboardOverlay()
            : base(OverlayColourScheme.Purple)
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            header = new DashboardOverlayHeader
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Depth = -float.MaxValue
                            },
                            content = new Container<DashboardDisplay>
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            }
                        }
                    }
                },
                loading = new LoadingLayer(content),
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = ColourProvider.Background5;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            localUser.BindTo(api.LocalUser);
            localUser.BindValueChanged(_ => onTabChanged());
            header.Current.BindValueChanged(_ => onTabChanged());
        }

        protected override void PopIn()
        {
            base.PopIn();
            header.Current.TriggerChange();
        }

        private void onTabChanged()
        {
            loading.Show();
            cancellationToken?.Cancel();

            // We may want to use OnlineViewContainer after https://github.com/ppy/osu/pull/8044 merge
            if (!api.IsLoggedIn)
            {
                content.Clear();
                return;
            }

            switch (header.Current.Value)
            {
                default:
                    loadDisplay(null);
                    return;

                case HomeOverlayTabs.Friends:
                    loadDisplay(new FriendDisplay());
                    return;
            }
        }

        private void loadDisplay(DashboardDisplay display)
        {
            if (display == null)
            {
                content.Clear();
                loading.Hide();
                return;
            }

            LoadComponentAsync(display, loaded =>
            {
                content.Clear();

                content.Add(loaded);
                loaded.Fetch();

                loading.Hide();
            }, (cancellationToken = new CancellationTokenSource()).Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}