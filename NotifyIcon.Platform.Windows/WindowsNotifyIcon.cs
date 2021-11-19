#if !XAMARIN_MAC && !__MACOS__ && !NET6_0_MACOS10_14 && !NETSTANDARD1_0 && !NETSTANDARD1_1
#if DRAWING || NETFRAMEWORK
using System.Drawing;
#endif
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Windows
{
#if NET5_0_OR_GREATER2
    [SupportedOSPlatform("windows")]
#endif
    internal sealed class WindowsNotifyIcon : NotifyIcon
    {
        readonly int _uID;
        readonly NativeWindow _window;
        static int _nextUID;
        bool _iconAdded;
        IntPtr popupMenu;
        static readonly uint WM_TASKBARCREATED = RegisterWindowMessage("TaskbarCreated");

        /// <summary>
        /// Represents the current icon data.
        /// </summary>
        readonly NOTIFYICONDATA iconData;

        public WindowsNotifyIcon() : base()
        {
            _uID = ++_nextUID;
            _window = new(HandleWndProc);
            iconData = new NOTIFYICONDATA()
            {
                hWnd = _window.Handle,
                uID = _uID,
                uFlags = NIF.TIP | NIF.MESSAGE,
                uCallbackMessage = (int)CustomWindowsMessage.WM_TRAYMOUSE,
                hIcon = IntPtr.Zero,
                uTimeoutOrVersion = 0,
                szTip = string.Empty,
            };
        }

        IntPtr _iconHandle;
        Icon? _icon;
        Bitmap? _iconBitmap;

        void SetIcon(Icon value)
        {
            if (_iconBitmap != null)
            {
                _iconBitmap.Dispose();
                _iconBitmap = null;
            }
            if (_icon != null)
            {
                _icon.Dispose();
            }
            _icon = value;
            _iconHandle = value.Handle;
        }

        void SetIcon(Bitmap value)
        {
            if (_icon != null)
            {
                _icon.Dispose();
                _icon = null;
            }
            if (_iconBitmap != null)
            {
                _iconBitmap.Dispose();
            }
            _iconBitmap = value;
            _iconHandle = value.GetHicon();
        }

        public override object? Icon
        {
            set
            {
                if (value is IntPtr intPtr)
                {
                    _iconHandle = intPtr;
                }
#if DRAWING || NETFRAMEWORK
                else if (value is Icon icon)
                {
                    SetIcon(icon);
                }
                else if (value is Bitmap bitmap)
                {
                    SetIcon(bitmap);
                }
#endif
                else
                {
#if DRAWING || NETFRAMEWORK
                    if (value is byte[] byteArray)
                    {
                        using var ms = new MemoryStream(byteArray);
                        SetIcon(new Icon(ms));
                    }
                    else if (value is Stream stream)
                    {
                        SetIcon(new Icon(stream));
                    }
                    else if (value is string fileName)
                    {
                        SetIcon(new Icon(fileName));
                    }
                    else if (value is FileInfo fileInfo)
                    {
                        SetIcon(new Icon(fileInfo.FullName));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
#else
                    throw new NotSupportedException();
#endif
                }
                UpdateIcon();
            }
        }

        string? _text;
        public override string? Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    UpdateIcon();
                }
            }
        }

        bool _visible = true;
        public override bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                    UpdateIcon();
                }
            }
        }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        sealed class NOTIFYICONDATA
        {
            public int cbSize =
#if (NETFRAMEWORK && NET451_OR_GREATER) || NET5_0_OR_GREATER2
                Marshal.SizeOf<NOTIFYICONDATA>();
#else
                Marshal.SizeOf(typeof(NOTIFYICONDATA));
#endif
            public IntPtr hWnd;
            public int uID;
            public NIF uFlags;
            public int uCallbackMessage;
            public IntPtr hIcon;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string? szTip;
            public int dwState;
            public int dwStateMask;
            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters.
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szInfo;
            public int uTimeoutOrVersion;
            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface
            /// above the text. It can have a maximum of 63 characters.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string szInfoTitle;
            public NIIF dwInfoFlags;

            ///// <summary>
            ///// Windows Vista (Shell32.dll version 6.0.6) and later. The handle of a customized
            ///// balloon icon provided by the application that should be used independently
            ///// of the tray icon. If this member is non-NULL and the <see cref="NIIF.USER"/>
            ///// flag is set, this icon is used as the balloon icon.<br/>
            ///// If this member is NULL, the legacy behavior is carried out.
            ///// </summary>
            //public IntPtr CustomBalloonIconHandle;
        }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        [Flags]
        enum NIF : uint
        {
            MESSAGE = 0x00000001,
            ICON = 0x00000002,
            TIP = 0x00000004,
            STATE = 0x00000008,
            INFO = 0x00000010,
            GUID = 0x00000020,
            REALTIME = 0x00000040,
            SHOWTIP = 0x00000080
        }

        [Flags]
        enum NIIF : uint
        {
            NONE = 0x00000000,
            INFO = 0x00000001,
            WARNING = 0x00000002,
            ERROR = 0x00000003,
            USER = 0x00000004,
            ICON_MASK = 0x0000000F,
            NOSOUND = 0x00000010,
            LARGE_ICON = 0x00000020,
            RESPECT_QUIET_TIME = 0x00000080
        }

        /// <summary>
        /// Custom Win32 window messages for the NotifyIcon
        /// </summary>
        enum CustomWindowsMessage
        {
            WM_TRAYICON = 0x8000 + 1024,
            WM_TRAYMOUSE = 0x0400 + 1024
        }

        enum NIM : uint
        {
            ADD = 0x00000000,
            MODIFY = 0x00000001,
            DELETE = 0x00000002,
            SETFOCUS = 0x00000003,
            SETVERSION = 0x00000004
        }

        /// <summary>
        /// Creates, updates or deletes the taskbar icon.
        /// </summary>
        [DllImport("shell32", CharSet = CharSet.Unicode)]
        static extern bool Shell_NotifyIcon(NIM cmd, NOTIFYICONDATA data);

        /// <summary>
        /// Shows, hides or removes the notify icon based on the set properties and parameters.
        /// </summary>
        /// <param name="remove">If set to true, the notify icon will be removed.</param>
        void UpdateIcon(bool remove = false)
        {
            if (!remove && _iconHandle != default && _visible)
            {
                iconData.uFlags |= NIF.ICON;
                iconData.hIcon = _iconHandle;
                iconData.szTip = _text;

                if (!_iconAdded)
                {
                    Shell_NotifyIcon(NIM.ADD, iconData);
                    _iconAdded = true;
                }
                else
                {
                    Shell_NotifyIcon(NIM.MODIFY, iconData);
                }
            }
            else
            {
                iconData.szInfoTitle = string.Empty;
                iconData.szInfo = string.Empty;
                Shell_NotifyIcon(NIM.DELETE, iconData);
                _iconAdded = false;
            }
        }

        void ShowBalloonTip(string? tipTitle, string? tipText, NIIF flags = NIIF.NONE/*, IntPtr balloonIconHandle = default*/)
        {
            if (iconData != null)
            {
                iconData.szInfoTitle = tipTitle ?? string.Empty;
                iconData.szInfo = tipText ?? string.Empty;
                iconData.uFlags |= NIF.INFO;
                //iconData.CustomBalloonIconHandle = balloonIconHandle;
                iconData.dwInfoFlags = flags;
                Shell_NotifyIcon(NIM.MODIFY, iconData);
            }
        }

        //internal void ShowBalloonTip(string tipTitle, string tipText, IntPtr balloonIcon, bool largeIcon)
        //{
        //    NIIF flags;
        //    if (balloonIcon == default)
        //    {
        //        flags = NIIF.NONE;
        //    }
        //    else
        //    {
        //        flags = NIIF.USER;
        //        if (largeIcon)
        //        {
        //            flags |= NIIF.LARGE_ICON;
        //        }
        //    }
        //    ShowBalloonTip(tipTitle, tipText, flags, balloonIcon);
        //}

        public override void ShowBalloonTip(string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            var flags = tipIcon switch
            {
                ToolTipIcon.None => NIIF.NONE,
                ToolTipIcon.Info => NIIF.INFO,
                ToolTipIcon.Warning => NIIF.WARNING,
                ToolTipIcon.Error => NIIF.ERROR,
                _ => throw new ArgumentOutOfRangeException(nameof(tipIcon), tipIcon, null),
            };
            ShowBalloonTip(tipTitle, tipText, flags/*, default*/);
        }

        public override void HideBalloonTip()
        {
            if (iconData != null)
            {
                // reset balloon by just setting the info to an empty string
                iconData.szInfo = iconData.szInfoTitle = string.Empty;
                Shell_NotifyIcon(NIM.MODIFY, iconData);
            }
        }

        sealed class NativeWindow : IDisposable
        {
            readonly WndProc wndProc;
            readonly WndProc _wndProc;
            readonly string _className = "NativeHelperWindow" + Guid.NewGuid();
            bool disposedValue;

            /// <summary>
            /// The window handle of the underlying native window.
            /// </summary>
            public IntPtr Handle { get; set; }

            /// <summary>
            /// Creates a new native (Win32) helper window for receiving window messages.
            /// </summary>
            public NativeWindow(WndProc wndProc)
            {
                this.wndProc = wndProc;

                // We need to store the window proc as a field so that
                // it doesn't get garbage collected away.
                _wndProc = new WndProc(WndProc);

                WNDCLASSEX wndClassEx = new()
                {
                    cbSize =
#if NETFRAMEWORK && NET451_OR_GREATER
                        Marshal.SizeOf<WNDCLASSEX>(),
#else
                        Marshal.SizeOf(typeof(WNDCLASSEX)),
#endif
                    lpfnWndProc = _wndProc,
                    hInstance = GetModuleHandle(null),
                    lpszClassName = _className
                };

                ushort atom = RegisterClassEx(ref wndClassEx);

                if (atom == 0)
                {
                    throw new Win32Exception();
                }

                Handle = CreateWindowEx(0, atom, null, WS_POPUP, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

                if (Handle == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }

            /// <summary>
            /// This function will receive all the system window messages relevant to our window.
            /// </summary>
            IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
            {
                var result = wndProc(hWnd, msg, wParam, lParam);
                if (result != default) return result;
                switch (msg)
                {
                    case (uint)WindowsMessage.WM_CLOSE:
                        DestroyWindow(hWnd);
                        break;
                    case (uint)WindowsMessage.WM_DESTROY:
                        PostQuitMessage(0);
                        break;
                    default:
                        return DefWindowProc(hWnd, msg, wParam, lParam);
                }
                return default;
            }

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
            /// <summary>
            /// Destructs the object and destroys the native window.
            /// </summary>
            ~NativeWindow()
            {
                if (Handle != default)
                {
                    PostMessage(Handle, (uint)WindowsMessage.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
                }
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: false);
            }

            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct WNDCLASSEX
        {
            public int cbSize;
            public int style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW")]
        static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr CreateWindowEx(
            int dwExStyle,
            uint lpClassName,
            string? lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        const uint WS_POPUP = 0x80000000;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "PostMessageW")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        enum WindowsMessage : uint
        {
            WM_DESTROY = 0x0002,
            WM_CLOSE = 0x0010,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONUP = 0x0205,
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern void PostQuitMessage(int nExitCode);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        bool _doubleClick;
        /// <summary>
        /// Handles the NotifyIcon-specific window messages sent by the notification icon.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        IntPtr HandleWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == (uint)CustomWindowsMessage.WM_TRAYMOUSE)
            {
                // Determine the type of message and call the matching event handlers
                switch (lParam.ToInt32())
                {
                    case (int)WindowsMessage.WM_LBUTTONUP:
                        if (!_doubleClick)
                        {
                            Click?.Invoke(this, EventArgs.Empty);
                        }
                        _doubleClick = false;
                        break;

                    case (int)WindowsMessage.WM_LBUTTONDBLCLK:
                        DoubleClick?.Invoke(this, EventArgs.Empty);
                        _doubleClick = true;
                        break;

                    case (int)WindowsMessage.WM_RBUTTONUP:
                        GetCursorPos(out var point);
                        RightClick?.Invoke(this, new(point.X, point.Y));
                        ShowContextMenu();
                        break;
                }
            }
            if (msg == WM_TASKBARCREATED && Visible)
            {
                UpdateIcon(true);
                UpdateIcon();
            }
            return default;
        }

        public override event EventHandler? Click;

        public override event EventHandler? DoubleClick;

        public override event EventHandler<MouseEventArgs>? RightClick;

        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        struct PointInt32
        {
            public int X;
            public int Y;

            public PointInt32(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetCursorPos")]
        static extern bool GetCursorPos(out PointInt32 pt);

        void DestroyMenu()
        {
            if (popupMenu != default)
            {
                DestroyMenu(popupMenu);
                popupMenu = default;
            }
        }

        void ShowContextMenu()
        {
            DestroyMenu();

            if (!ContextMenuStrip.Items.Any()) return;

            // Since we can't use the Avalonia ContextMenu directly due to shortcomings
            // regrading its positioning, we'll create a native context menu instead.
            // This dictionary will map the menu item IDs which we'll need for the native
            // menu to the MenuItems of the provided Avalonia ContextMenu.
            Dictionary<uint, Action> contextItemLookup = new();

            // Create a native (Win32) popup menu as the notify icon's context menu.
            popupMenu = CreatePopupMenu();

            uint i = 1;
            foreach (var item in ContextMenuStrip.Items)
            {
                // Add items to the native context menu by simply reusing
                // the information provided within the Avalonia ContextMenu.
                AppendMenu(popupMenu, MenuFlags.MF_STRING, i, item.Text ?? string.Empty);

                // Add the mapping so that we can find the selected item later
                contextItemLookup.Add(i,
                    () => OnContextMenuItemClick(item, this, EventArgs.Empty));
                i++;
            }

            // To display a context menu for a notification icon, the current window
            // must be the foreground window before the application calls TrackPopupMenu
            // or TrackPopupMenuEx.Otherwise, the menu will not disappear when the user
            // clicks outside of the menu or the window that created the menu (if it is
            // visible). If the current window is a child window, you must set the
            // (top-level) parent window as the foreground window.
            SetForegroundWindow(_window.Handle);

            // Get the mouse cursor position
            GetCursorPos(out var pt);

            OnContextMenuPopup(this, EventArgs.Empty);

            // Now display the context menu and block until we get a result
            uint commandId = TrackPopupMenuEx(
                popupMenu,
                UFLAGS.TPM_BOTTOMALIGN |
                UFLAGS.TPM_RIGHTALIGN |
                UFLAGS.TPM_NONOTIFY |
                UFLAGS.TPM_RETURNCMD,
                pt.X, pt.Y, _window.Handle, IntPtr.Zero);

            OnContextMenuCollapse(this, EventArgs.Empty);

            // If we have a result, execute the corresponding command
            if (commandId != 0)
            {
                if (contextItemLookup.ContainsKey(commandId))
                {
                    contextItemLookup[commandId]();
                }
            }
        }

        [DllImport("user32.dll")]
        static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool AppendMenu(IntPtr hMenu, MenuFlags uFlags, uint uIDNewItem, string lpNewItem);

        [Flags]
        enum MenuFlags : uint
        {
            MF_STRING = 0,
            MF_BYPOSITION = 0x400,
            MF_SEPARATOR = 0x800,
            MF_REMOVE = 0x1000,
        }

        [DllImport("user32.dll", ExactSpelling = true)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern uint TrackPopupMenuEx(IntPtr hmenu, UFLAGS uFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("user32.dll")]
        static extern bool DestroyMenu(IntPtr hmenu);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);

        [Flags]
        enum UFLAGS : uint
        {
            TPM_LEFTALIGN = 0x0000,
            TPM_CENTERALIGN = 0x0004,
            TPM_RIGHTALIGN = 0x0008,
            TPM_TOPALIGN = 0x0000,
            TPM_VCENTERALIGN = 0x0010,
            TPM_BOTTOMALIGN = 0x0020,
            TPM_HORIZONTAL = 0x0000,
            TPM_VERTICAL = 0x0040,
            TPM_NONOTIFY = 0x0080,
            TPM_RETURNCMD = 0x0100
        }

        bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _iconHandle = default;
                    if (_iconBitmap != null)
                    {
                        _iconBitmap.Dispose();
                        _iconBitmap = null;
                    }
                    if (_icon != null)
                    {
                        _icon.Dispose();
                        _icon = null;
                    }
                    DestroyMenu();
                    _window.Dispose();
                    UpdateIcon(remove: true);
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
    }
}
#endif
