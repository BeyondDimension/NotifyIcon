#if !__LIB_SYS_OS2__ && !XAMARIN_MAC
using OperatingSystem2 = System.OperatingSystem;
#endif
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
using System.Windows;

#if LINUX || MAC || XAMARIN_MAC || NET6_0_MACOS10_14 || WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS
[assembly: NotifyIcon.Dependency(typeof(NotifyIconImpl))]
#else
[assembly: NotifyIcon.Dependency(typeof(NotifyIconExtensions.RuntimeImplType))]
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

#if LINUX || MAC || XAMARIN_MAC || NET6_0_MACOS10_14 || WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS
#else
        internal sealed class RuntimeImplType : NotifyIcon.IRuntimeImplType
        {
            Type NotifyIcon.IRuntimeImplType.Type => GetNotifyIconImpl();
        }
#endif

        static Type GetNotifyIconImpl()
        {
#if LINUX || MAC || XAMARIN_MAC || NET6_0_MACOS10_14 || WINDOWS || NET5_0_WINDOWS || NET6_0_WINDOWS
            return typeof(NotifyIconImpl);
#else
#if !NETSTANDARD1_1
            if (OperatingSystem2.IsWindows
#if !__LIB_SYS_OS2__
            ()
#endif
            )
            {
                return typeof(WindowsNotifyIcon);
            }
#if !NET45 && !NET40 && !NET35
            if (OperatingSystem2.IsLinux
#if !__LIB_SYS_OS2__
            ()
#endif
)
            {
                return typeof(LinuxNotifyIcon);
            }
            if (OperatingSystem2.IsMacOS
#if !__LIB_SYS_OS2__
            ()
#endif
)
            {
                return typeof(MacNotifyIcon);
            }
#endif
#endif
            throw new PlatformNotSupportedException();
#endif
        }

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
            services.AddSingleton(typeof(NotifyIcon), GetNotifyIconImpl());
            return services;
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
}