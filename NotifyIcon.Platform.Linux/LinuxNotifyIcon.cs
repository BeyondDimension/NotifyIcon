#if !XAMARIN_MAC && !__MACOS__ && !NET6_0_MACOS10_14 && !NET_WINDOWS && !NET5_0_WINDOWS && !NET6_0_WINDOWS && !NETSTANDARD1_0 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_1
using Gdk;
using Gtk;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace System.Windows
{
#pragma warning disable CS0612 // 类型或成员已过时
#if NET5_0_OR_GREATER2
    [SupportedOSPlatform("linux")]
#endif
    internal sealed class LinuxNotifyIcon : NotifyIcon
    {
        readonly StatusIcon statusIcon;
        Menu? popupMenu;

        public LinuxNotifyIcon() : base()
        {
            statusIcon = new();
            statusIcon.Activate += StatusIcon_Activate;
            statusIcon.PopupMenu += StatusIcon_PopupMenu;
        }

        void StatusIcon_Activate(object? sender, EventArgs e) => Click?.Invoke(sender, e);

        void StatusIcon_PopupMenu(object? sender, PopupMenuArgs _)
        {
            popupMenu?.Dispose();

            if (!ContextMenuStrip.Items.Any()) return;

            popupMenu = new();

            popupMenu.Deactivated += PopupMenu_Deactivated;
            popupMenu.PoppedUp += PopupMenu_PoppedUp;

            foreach (var item in ContextMenuStrip.Items)
            {
                MenuItem menuItem;
                if (item.Icon != null)
                {
                    var icon = Convert(item.Icon);
                    menuItem = new ImageMenuItem(item.Text)
                    {
                        Image = new Image(icon, IconSize.Menu),
                    };
                }
                else
                {
                    menuItem = new MenuItem(item.Text);
                }
                menuItem.Activated += (s, e) => OnContextMenuItemClick(item, s, e);
                popupMenu.Add(menuItem);
            }

            popupMenu.ShowAll();
            popupMenu.Popup();

            RightClick?.Invoke(sender, MouseEventArgs.Empty);
        }

        private void PopupMenu_PoppedUp(object? sender, EventArgs e)
            => OnContextMenuPopup(sender, e);

        private void PopupMenu_Deactivated(object? sender, EventArgs e)
        {
            if (popupMenu != null)
            {
                popupMenu.Deactivated -= PopupMenu_Deactivated;
                popupMenu.PoppedUp -= PopupMenu_PoppedUp;
            }
            OnContextMenuCollapse(sender, e);
        }

        static GLib.IIcon Convert(object? value)
        {
            if (value is GLib.IIcon icon)
            {
                return icon;
            }
            else
            {
                Pixbuf pixbuf;
                if (value is byte[] byteArray)
                {
                    pixbuf = new(byteArray);
                }
                else if (value is Stream stream)
                {
                    pixbuf = new(stream);
                }
                else if (value is string fileName)
                {
                    pixbuf = new(fileName);
                }
                else if (value is FileInfo fileInfo)
                {
                    pixbuf = new(fileInfo.FullName);
                }
                else if (value is IntPtr intPtr)
                {
                    pixbuf = new(intPtr);
                }
                else if (value is string[] strArray)
                {
                    pixbuf = new(strArray);
                }
                else if (value is GLib.IAsyncResult asyncResult)
                {
                    pixbuf = new(asyncResult);
                }
                else
                {
                    throw new NotSupportedException();
                }
                return pixbuf;
            }
        }

        public override object? Icon
        {
            set => statusIcon.Icon = Convert(value);
        }

        public override string? Text
        {
            get => statusIcon.TooltipText;
            set => statusIcon.TooltipText = value;
        }

        public override bool Visible
        {
            get => statusIcon.Visible;
            set => statusIcon.Visible = value;
        }

        public override event EventHandler? Click;

        public override event EventHandler? DoubleClick;

        public override event EventHandler<MouseEventArgs>? RightClick;

        bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    popupMenu?.Dispose();
                    statusIcon.Activate -= StatusIcon_Activate;
                    statusIcon.PopupMenu -= StatusIcon_PopupMenu;
                    statusIcon.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
    }
#pragma warning restore CS0612 // 类型或成员已过时
}
#endif