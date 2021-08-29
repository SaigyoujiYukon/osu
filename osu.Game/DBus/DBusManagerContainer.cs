using System;
using JetBrains.Annotations;
using M.DBus;
using M.DBus.Services;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Users;
using Tmds.DBus;

namespace osu.Game.DBus
{
    public class DBusManagerContainer : Component
    {
        public readonly DBusManager DBusManager = new DBusManager(false);
        private readonly Bindable<bool> controlSource;

        public DBusManagerContainer(bool autoStart = false, Bindable<bool> controlSource = null)
        {
            if (autoStart && controlSource != null)
            {
                this.controlSource = controlSource;
            }
            else if (controlSource == null && autoStart)
            {
                throw new InvalidOperationException("设置了自动启动但是控制源是null?");
            }
        }

        private BeatmapInfoDBusService beatmapService;
        private AudioInfoDBusService audioservice;
        private UserInfoDBusService userInfoService;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [CanBeNull]
        [Resolved(canBeNull: true)]
        private NotificationOverlay notificationOverlay { get; set; }

        private readonly Bindable<UserActivity> bindableActivity = new Bindable<UserActivity>();

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api, MConfigManager config)
        {
            DBusManager.RegisterNewObjects(new IDBusObject[]
            {
                beatmapService = new BeatmapInfoDBusService(),
                audioservice = new AudioInfoDBusService(),
                userInfoService = new UserInfoDBusService(),
                new Greet(GetType().Namespace + '.' + GetType().Name)
                {
                    AllowPost = config.GetBindable<bool>(MSetting.DBusAllowPost),
                    OnMessageRecive = s => Schedule(() =>
                    {
                        notificationOverlay?.Post(new SimpleNotification
                        {
                            Text = "收到一条来自DBus的消息: \n" + s
                        });
                    })
                }
            });

            api.LocalUser.BindValueChanged(onUserChanged, true);
            beatmap.BindValueChanged(onBeatmapChanged, true);
            ruleset.BindValueChanged(v => userInfoService.SetProperty("current_ruleset", v.NewValue?.Name ?? "???"), true);
            bindableActivity.BindValueChanged(v => userInfoService.SetProperty("activity", v.NewValue?.Status ?? "空闲"), true);
        }

        private void onUserChanged(ValueChangedEvent<User> v)
        {
            bindableActivity.UnbindBindings();
            bindableActivity.BindTo(v.NewValue.Activity);
            userInfoService.User = v.NewValue;
        }

        private void onBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            beatmapService.Beatmap = v.NewValue;
            audioservice.Beatmap = v.NewValue;
        }

        protected override void LoadComplete()
        {
            controlSource?.BindValueChanged(onControlSourceChanged, true);
            base.LoadComplete();
        }

        private void onControlSourceChanged(ValueChangedEvent<bool> v)
        {
            if (v.NewValue)
                DBusManager.Connect();
            else
                DBusManager.Disconnect();
        }

        protected override void Dispose(bool isDisposing)
        {
            DBusManager.Dispose();
            base.Dispose(isDisposing);
        }
    }
}
