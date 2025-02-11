// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Security.Principal;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Security
{
    /// <summary>
    /// Checks if the game is running with elevated privileges (as admin in Windows, root in Unix) and displays a warning notification if so.
    /// </summary>
    public class ElevatedPrivilegesChecker : Component
    {
        [Resolved]
        private NotificationOverlay notifications { get; set; }

        private bool elevated;

        [BackgroundDependencyLoader]
        private void load()
        {
            elevated = checkElevated();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (elevated)
                notifications.Post(new ElevatedPrivilegesNotification());
        }

        private bool checkElevated()
        {
            try
            {
                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Windows:
                        if (!OperatingSystem.IsWindows()) return false;

                        var windowsIdentity = WindowsIdentity.GetCurrent();
                        var windowsPrincipal = new WindowsPrincipal(windowsIdentity);

                        return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

                    case RuntimeInfo.Platform.macOS:
                    case RuntimeInfo.Platform.Linux:
                        return Mono.Unix.Native.Syscall.geteuid() == 0;
                }
            }
            catch
            {
            }

            return false;
        }

        private class ElevatedPrivilegesNotification : SimpleNotification
        {
            public override bool IsImportant => true;

            public ElevatedPrivilegesNotification()
            {
                Text = $"使用{(RuntimeInfo.IsUnix ? "root" : "administrator")}用户运行osu!并不会改进性能， "
                       + "并且可能会破坏集成或导致安全问题。 "
                       + "请使用普通用户运行游戏。";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, NotificationOverlay notificationOverlay)
            {
                Icon = FontAwesome.Solid.ShieldAlt;
                IconBackgound.Colour = colours.YellowDark;
            }
        }
    }
}
