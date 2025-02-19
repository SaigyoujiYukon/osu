// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using M.DBus;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class LinuxSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Linux集成";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config, DBusManager dBusManager)
        {
            SettingsCheckbox trayCheckbox;

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<GamemodeActivateCondition>
                {
                    LabelText = "Gamemode启用条件",
                    TooltipText = "依赖libgamemode",
                    Current = config.GetBindable<GamemodeActivateCondition>(MSetting.Gamemode)
                },
                trayCheckbox = new SettingsCheckbox
                {
                    LabelText = "启用DBus系统托盘",
                    Current = config.GetBindable<bool>(MSetting.EnableTray)
                },
                new SettingsCheckbox
                {
                    LabelText = "允许通过DBus发送系统通知",
                    Current = config.GetBindable<bool>(MSetting.EnableSystemNotifications)
                },
                new SettingsTextBox
                {
                    LabelText = "托盘图标名称",
                    Current = config.GetBindable<string>(MSetting.TrayIconName)
                },
            };

            trayCheckbox.WarningText = "由于未知原因, 启用再禁用托盘功能可能不会使托盘图标消失。\n具体原因正在调查中。";
        }
    }
}
