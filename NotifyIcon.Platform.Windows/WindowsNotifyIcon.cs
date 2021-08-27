#if !XAMARIN_MAC && !__MACOS__ && !NET6_0_MACOS10_14
#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
#if DRAWING || NETFRAMEWORK
using System.Drawing;
#endif
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;

namespace System.Windows
{
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    internal sealed class WindowsNotifyIcon : NotifyIcon
    {
        readonly int _uID = 0;
        readonly NativeWindow _window;
        static int _nextUID = 0;
        bool _iconAdded;

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
            };
        }

        IntPtr _iconHandle;
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
                    _iconHandle = icon.Handle;
                }
                else if (value is Bitmap bitmap)
                {
                    _iconHandle = bitmap.GetHicon();
                }
#endif
                else
                {
#if DRAWING || NETFRAMEWORK
                    if (value is byte[] byteArray)
                    {
                        _iconHandle = new Icon(new MemoryStream(byteArray)).Handle;
                    }
                    if (value is Stream stream)
                    {
                        _iconHandle = new Icon(stream).Handle;
                    }
                    else if (value is string fileName)
                    {
                        _iconHandle = new Icon(fileName).Handle;
                    }
                    else if (value is FileInfo fileInfo)
                    {
                        _iconHandle = new Icon(fileInfo.FullName).Handle;
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

        bool _visible;
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
#if NETFRAMEWORK && NET451_OR_GREATER
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
            public int dwState = 0;
            public int dwStateMask = 0;
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

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The handle of a customized
            /// balloon icon provided by the application that should be used independently
            /// of the tray icon. If this member is non-NULL and the <see cref="NIIF.USER"/>
            /// flag is set, this icon is used as the balloon icon.<br/>
            /// If this member is NULL, the legacy behavior is carried out.
            /// </summary>
            public IntPtr CustomBalloonIconHandle;
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
                Shell_NotifyIcon(NIM.DELETE, iconData);
                _iconAdded = false;
            }
        }

        void ShowBalloonTip(string? tipTitle, string? tipText, NIIF flags = NIIF.NONE, IntPtr balloonIconHandle = default)
        {
            if (iconData != null)
            {
                iconData.szInfo = tipTitle ?? string.Empty;
                iconData.szInfoTitle = tipText ?? string.Empty;
                iconData.uFlags |= NIF.INFO;
                iconData.CustomBalloonIconHandle = balloonIconHandle;
                iconData.dwInfoFlags = flags;
                Shell_NotifyIcon(NIM.MODIFY, iconData);
            }
        }

        internal void ShowBalloonTip(string tipTitle, string tipText, IntPtr balloonIcon, bool largeIcon)
        {
            NIIF flags;
            if (balloonIcon == default)
            {
                flags = NIIF.NONE;
            }
            else
            {
                flags = NIIF.USER;
                if (largeIcon)
                {
                    flags |= NIIF.LARGE_ICON;
                }
            }
            ShowBalloonTip(tipTitle, tipText, flags, balloonIcon);
        }

        public override void ShowBalloonTip(string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            NIIF flags = tipIcon switch
            {
                ToolTipIcon.None => NIIF.NONE,
                ToolTipIcon.Info => NIIF.INFO,
                ToolTipIcon.Warning => NIIF.WARNING,
                ToolTipIcon.Error => NIIF.ERROR,
                _ => throw new ArgumentOutOfRangeException(nameof(tipIcon), tipIcon, null),
            };
            ShowBalloonTip(tipTitle, tipText, flags, default);
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

        sealed class NativeWindow
        {
            readonly WndProc wndProc;
            readonly WndProc _wndProc;
            readonly string _className = "NativeHelperWindow" + Guid.NewGuid();

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
            /// Destructs the object and destroys the native window.
            /// </summary>
            ~NativeWindow()
            {
                if (Handle != IntPtr.Zero)
                {
                    PostMessage(Handle, (uint)WindowsMessage.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
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
            switch (msg)
            {
                case (uint)CustomWindowsMessage.WM_TRAYMOUSE:
                    // Forward WM_TRAYMOUSE messages to the tray icon's window procedure

                    // We only care about tray icon messages
                    if (msg == (uint)CustomWindowsMessage.WM_TRAYMOUSE)
                    {
                        // Determine the type of message and call the matching event handlers
                        switch (lParam.ToInt32())
                        {
                            case (int)WindowsMessage.WM_LBUTTONUP:
                                if (!_doubleClick)
                                {
                                    Click?.Invoke(this, new());
                                }
                                _doubleClick = false;
                                break;

                            case (int)WindowsMessage.WM_LBUTTONDBLCLK:
                                DoubleClick?.Invoke(this, new());
                                _doubleClick = true;
                                break;

                            case (int)WindowsMessage.WM_RBUTTONUP:
                                GetCursorPos(out var point);
                                RightClick?.Invoke(this, new(point.X, point.Y));
                                ShowContextMenu();
                                break;
                        }
                    }
                    break;
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

        void ShowContextMenu()
        {
            if (!ContextMenuStrip.Items.Any()) return;

            Dictionary<uint, Action> contextItemLookup = new();

            // Create a native (Win32) popup menu as the notify icon's context menu.
            var popupMenu = CreatePopupMenu();

            uint i = 1;
            foreach (var item in ContextMenuStrip.Items)
            {
                AppendMenu(popupMenu, MenuFlags.MF_STRING, i, item.Text ?? string.Empty);
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

            // Now display the context menu and block until we get a result
            uint commandId = TrackPopupMenuEx(
                popupMenu,
                UFLAGS.TPM_BOTTOMALIGN |
                UFLAGS.TPM_RIGHTALIGN |
                UFLAGS.TPM_NONOTIFY |
                UFLAGS.TPM_RETURNCMD,
                pt.X, pt.Y, _window.Handle, IntPtr.Zero);

            // If we have a result, execute the corresponding command
            if (commandId != 0)
            {
                contextItemLookup[commandId]();
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

        [Flags]
        public enum UFLAGS : uint
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
    }
}
#endif