#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
#if DI
using Microsoft.Extensions.DependencyInjection;
#endif
#if LINUX
using NotifyIconImpl = System.Windows.LinuxNotifyIcon;
#endif
#if MAC || XAMARIN_MAC || NET6_0_MACOS10_14
using NotifyIconImpl = System.Windows.MacNotifyIcon;
#endif
#if WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS || NET35 || NET40 || NET45
using NotifyIconImpl = System.Windows.WindowsNotifyIcon;
#endif
#if DRAWING || NETFRAMEWORK
using System.Drawing;
#endif

namespace System.Windows
{
    /// <summary>
    /// 对 <see cref="NotifyIcon"/> 类的扩展函数。
    /// </summary>
    public static class NotifyIconExtensions
    {
#if NET5_0_OR_GREATER && (LINUX || MAC || WINDOWS)
        internal const string SupportedOSPlatformName =
#if LINUX
            "linux";
#endif
#if MAC
            "macos";
#endif
#if WINDOWS
            "windows";
#endif
#endif

#if DI
        /// <summary>
        /// 添加 <see cref="NotifyIcon"/> 到 <see cref="IServiceCollection"/> 中。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
#if NET5_0_OR_GREATER && (LINUX || MAC || WINDOWS)
        [SupportedOSPlatform(SupportedOSPlatformName)]
#endif
        public static IServiceCollection AddNotifyIcon(this IServiceCollection services)
        {
#if LINUX || MAC || XAMARIN_MAC || NET6_0_MACOS10_14 || WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS
            services.AddSingleton<NotifyIcon, NotifyIconImpl>();
            return services;
#else
#if !NETSTANDARD1_1
            if (OperatingSystem2.IsWindows)
            {
                services.AddSingleton<NotifyIcon, WindowsNotifyIcon>();
                return services;
            }
#if !NET45
            if (OperatingSystem2.IsLinux)
            {
                services.AddSingleton<NotifyIcon, LinuxNotifyIcon>();
                return services;
            }
            if (OperatingSystem2.IsMacOS)
            {
                services.AddSingleton<NotifyIcon, MacNotifyIcon>();
                return services;
            }
#endif
#endif
            throw new PlatformNotSupportedException();
#endif
        }
#endif

        //#if WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS
        //#if DRAWING || NETFRAMEWORK
        //        /// <summary>
        //        /// 在任务栏中显示具有指定标题、文本和图标的气球状提示（仅支持 Windows）。
        //        /// </summary>
        //        /// <param name="notifyIcon"></param>
        //        /// <param name="tipTitle">提示标题。</param>
        //        /// <param name="tipText">提示文本。</param>
        //        /// <param name="balloonIcon">提示图标。</param>
        //        /// <param name="largeIcon"><see langword="true"/> to allow large icons (Windows Vista and later).</param>
        //#if NET5_0_OR_GREATER
        //        [SupportedOSPlatform("windows")]
        //#endif
        //        public static void ShowBalloonTip(this NotifyIcon notifyIcon, string tipTitle, string tipText, Icon balloonIcon, bool largeIcon = false)
        //        {
        //            if (notifyIcon is NotifyIconImpl notifyIcon1)
        //            {
        //                notifyIcon1.ShowBalloonTip(tipTitle, tipText, balloonIcon.Handle, largeIcon);
        //            }
        //            else
        //            {
        //                throw new PlatformNotSupportedException();
        //            }
        //        }
        //#endif

        //        /// <summary>
        //        /// 在任务栏中显示具有指定标题、文本和图标的气球状提示（仅支持 Windows）。
        //        /// </summary>
        //        /// <param name="notifyIcon"></param>
        //        /// <param name="tipTitle">提示标题。</param>
        //        /// <param name="tipText">提示文本。</param>
        //        /// <param name="balloonIcon">提示图标指针。</param>
        //        /// <param name="largeIcon"><see langword="true"/> to allow large icons (Windows Vista and later).</param>
        //#if NET5_0_OR_GREATER
        //        [SupportedOSPlatform("windows")]
        //#endif
        //        public static void ShowBalloonTip(this NotifyIcon notifyIcon, string tipTitle, string tipText, IntPtr balloonIcon, bool largeIcon = false)
        //        {
        //            if (notifyIcon is NotifyIconImpl notifyIcon1)
        //            {
        //                notifyIcon1.ShowBalloonTip(tipTitle, tipText, balloonIcon, largeIcon);
        //            }
        //            else
        //            {
        //                throw new PlatformNotSupportedException();
        //            }
        //        }
        //#endif
    }

#if !DI
    /// <summary>
    ///
    /// </summary>
#if NET5_0_OR_GREATER && (LINUX || MAC || WINDOWS)
    [SupportedOSPlatform(SupportedOSPlatformName)]
#endif
    public static class NotifyIconFactory
    {
        /// <summary>
        /// <see cref="NotifyIcon"/> 的实现类型。
        /// </summary>
        public static Type ImplType =>
#if NETSTANDARD1_0
            throw new NotSupportedException();
#else
            typeof(NotifyIconImpl);
#endif

        /// <summary>
        /// 创建 <see cref="NotifyIcon"/> 实例。
        /// </summary>
        /// <returns></returns>
        public static NotifyIcon Create() =>
#if NETSTANDARD1_0
            throw new NotSupportedException();
#else
            new NotifyIconImpl();
#endif
    }
#endif
}