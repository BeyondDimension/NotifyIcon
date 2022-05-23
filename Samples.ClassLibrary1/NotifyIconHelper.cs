using ReactiveUI;
using Samples.Properties;
using System;
using System.Linq;
using System.Windows;

namespace Samples
{
    /// <summary>
    ///
    /// </summary>
    public static class NotifyIconHelper
    {
        static readonly Random random = new();
        static readonly ToolTipIcon[] toolTipIcons = (ToolTipIcon[])Enum.GetValues(typeof(ToolTipIcon));

        static object Icon => OperatingSystem2.IsMacOS() ? Resources.Icon_16 : Resources.Icon;

        /// <summary>
        ///
        /// </summary>
        /// <param name="notifyIcon"></param>
        /// <param name="exit"></param>
        public static void Init(NotifyIcon notifyIcon, Action? exit)
        {
            notifyIcon.Text = "My Notify Icon";
            notifyIcon.Icon = Icon;
            var count = 8;
            var typeUNUserNotificationCenter = Type.GetType("UserNotifications.UNUserNotificationCenter, Xamarin.Mac");
            var menuItems = Enumerable.Range(1, count)
                .Select(x =>
                {
                    var menuItem = new ContextMenuStrip.MenuItem
                    {
                        Text = OperatingSystem2.IsWindows() && x == count ? "Test Balloon Tip" : $"Menu Item {x}",
                    };
                    var index = 1;
                    menuItem.Command = ReactiveCommand.Create(() =>
                    {
                        var text = $"Menu Item {x}({index++})";
                        if (x == count && (OperatingSystem2.IsWindows() || typeUNUserNotificationCenter != null))
                        {
                            notifyIcon.ShowBalloonTip("My Title", text,
                                toolTipIcons[random.Next(toolTipIcons.Length)]);
                        }
                        else
                        {
                            menuItem.Text = text;
                        }
                    });
                    //menuItem.Click += ...;
                    if (index % 2 == 0)
                    {
                        menuItem.Icon = Icon;
                    }
                    return menuItem;
                });
            foreach (var item in menuItems)
            {
                notifyIcon.ContextMenuStrip.Items.Add(item);
            }
            notifyIcon.ContextMenuStrip.Items.Add(new()
            {
                Text = "Exit",
                Command = ReactiveCommand.Create(() => exit?.Invoke()),
            });
        }
    }
}