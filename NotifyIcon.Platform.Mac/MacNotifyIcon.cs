#if !NET5_0_WINDOWS && !NET6_0_WINDOWS && !NETSTANDARD1_0 && !NET35 && !NET40 && !NET45 && !NETSTANDARD1_1
#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using MonoMac.CoreGraphics;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
using CoreGraphics;
#endif
#if NET5_0_OR_GREATER || NET6_0_MACOS10_14
using System.Runtime.Versioning;
#endif
using System.IO;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Windows
{
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("macos")]
#endif
    internal sealed class MacNotifyIcon : NotifyIcon
    {
        NSStatusItem? statusItem;

        public MacNotifyIcon() : base()
        {
#if MONO_MAC
            var length = -1D;
#else
            var length = NSStatusItemLength.Variable;
#endif
            NSRunLoop.Main.InvokeOnMainThread(() =>
            {
                statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(length);
                statusItem.DoubleClick += StatusItem_DoubleClick;
            });
        }

        void StatusItem_DoubleClick(object? sender, EventArgs e)
           => DoubleClick?.Invoke(sender, e);

        static NSImage Convert(object? value)
        {
            if (value is NSImage image)
            {
                return image;
            }
            else
            {
                if (value is byte[] byteArray)
                {
                    return new NSImage(NSData.FromArray(byteArray));
                }
                if (value is Stream stream)
                {
                    return NSImage.FromStream(stream);
                }
                else if (value is string fileName)
                {
                    return new(fileName);
                }
                else if (value is FileInfo fileInfo)
                {
                    return new(fileInfo.FullName);
                }
#if MONO_MAC
                else if (value is IntPtr intPtr)
                {
                    return new(intPtr);
                }
#endif
                else if (value is NSPasteboard pasteboard)
                {
                    return new(pasteboard);
                }
                else if (value is NSCoder coder)
                {
                    return new(coder);
                }
#if MONO_MAC
                else if (value is NSObjectFlag t)
                {
                    return new(t);
                }
#endif
                else if (value is NSUrl url)
                {
                    return new(url);
                }
                else if (value is CGSize aSize)
                {
                    return new(aSize);
                }
                else if (value is NSData data)
                {
                    return new(data);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

#if NET5_0_OR_GREATER || NET6_0_MACOS10_14
        [UnsupportedOSPlatform("macos10.10")]
#endif
        public override object? Icon
        {
            set => statusItem!.Image = Convert(value);
        }

#if NET5_0_OR_GREATER || NET6_0_MACOS10_14
        [UnsupportedOSPlatform("macos10.10")]
#endif
        public override string? Text
        {
            get => statusItem!.ToolTip;
            set => statusItem!.ToolTip = value;
        }

        public override bool Visible
#if MONO_MAC
        {
            get => statusItem!.View.NeedsDisplay;
            set => statusItem!.View.NeedsDisplay = value;
        }
#else
        {
            get => statusItem!.Visible;
            set => statusItem!.Visible = value;
        }
#endif

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
                    if (statusItem != null)
                    {
                        statusItem.DoubleClick -= StatusItem_DoubleClick;
                        statusItem.Dispose();
                        statusItem = null;
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        readonly Dictionary<ContextMenuStrip.MenuItem, NSMenuItem> menuItems = new();

        NSMenuItem Convert(ContextMenuStrip.MenuItem menuItem)
        {
            if (menuItems.ContainsKey(menuItem)) return menuItems[menuItem];
            NSMenuItem menuItem1 = new();
            menuItem.PropertyChanged += MenuItem_PropertyChanged;
            menuItem1.Activated += (s, e) => OnContextMenuItemClick(menuItem, s, e);
            MenuItem_PropertyChanged(menuItem1, new(nameof(ContextMenuStrip.MenuItem.Text)));
            MenuItem_PropertyChanged(menuItem1, new(nameof(ContextMenuStrip.MenuItem.Icon)));
            MenuItem_PropertyChanged(menuItem1, new(nameof(ContextMenuStrip.MenuItem.Visible)));
            menuItems.Add(menuItem, menuItem1);
            return menuItem1;
            void MenuItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(ContextMenuStrip.MenuItem.Text):
                        menuItem1.Title = menuItem.Text ?? string.Empty;
                        break;
                    case nameof(ContextMenuStrip.MenuItem.Icon):
                        menuItem1.Image = menuItem.Icon != null ? Convert(menuItem.Icon) : null;
                        break;
                    case nameof(ContextMenuStrip.MenuItem.Visible):
                        if (menuItem1.View != null)
                        {
                            menuItem1.View.NeedsDisplay = menuItem.Visible;
                        }
                        break;
                }
            }
        }

        protected override void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (statusItem!.Menu == null)
            {
                statusItem.Menu = new();
            }
            var newStartingIndex = e.NewStartingIndex < 0 ? 0 : e.NewStartingIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Add();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Remove();
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    Remove();
                    Add();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    newStartingIndex = 0;
                    statusItem.Menu.RemoveAllItems();
                    foreach (var item in menuItems.Values)
                    {
                        item.Dispose();
                    }
                    menuItems.Clear();
                    Add();
                    break;
            }
            void Add()
            {
                if (e.NewItems == null) return;
                foreach (var item in e.NewItems)
                {
                    if (item is ContextMenuStrip.MenuItem menuItem)
                    {
                        var newItem = Convert(menuItem);
                        statusItem.Menu.InsertItem(newItem, newStartingIndex++);
                    }
                }
            }
            void Remove()
            {
                if (e.OldItems == null) return;
                foreach (var item in e.OldItems)
                {
                    if (item is ContextMenuStrip.MenuItem menuItem)
                    {
                        if (menuItems.ContainsKey(menuItem))
                        {
                            var removeItem = menuItems[menuItem];
                            statusItem.Menu.RemoveItem(removeItem);
                            removeItem.Dispose();
                        }
                    }
                }
            }
        }
    }
}
#endif