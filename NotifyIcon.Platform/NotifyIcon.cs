#if NET5_0_OR_GREATER
using System.Runtime.Versioning;
#endif
using System.Collections.Specialized;

namespace System.Windows
{
    /// <summary>
    /// 指定可在通知区域创建图标的组件。
    /// </summary>
    public abstract partial class NotifyIcon : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public NotifyIcon()
        {
            ContextMenuStrip = new();
            ContextMenuStrip.Items.CollectionChanged += Items_CollectionChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
        }

        /// <summary>
        /// 获取或设置与 NotifyIcon 关联的快捷菜单。
        /// </summary>
        public ContextMenuStrip ContextMenuStrip { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnContextMenuCollapse(object? sender, EventArgs e)
            => ContextMenuStrip.OnCollapse(sender, e);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnContextMenuPopup(object? sender, EventArgs e)
            => ContextMenuStrip.OnPopup(sender, e);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="menuItem"></param>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected static void OnContextMenuItemClick(ContextMenuStrip.MenuItem menuItem, object? sender, EventArgs e) => menuItem.OnClick(sender, e);

        /// <summary>
        /// 获取或设置当前图标。
        /// </summary>
        public abstract object? Icon { set; }

        /// <summary>
        /// 获取或设置当鼠标指针停留在通知区域图标上时显示的工具提示文本。
        /// </summary>
        public abstract string? Text { get; set; }

        /// <summary>
        /// 获取或设置一个值，该值指示任务栏的通知区域是否会显示图标。
        /// </summary>
        public abstract bool Visible { get; set; }

        /// <summary>
        /// 当用户单击通知区域中的图标时发生。
        /// </summary>
        public abstract event EventHandler? Click;

        /// <summary>
        /// 当用户双击任务栏的通知区域中的图标时发生。
        /// </summary>
        public abstract event EventHandler? DoubleClick;

        /// <summary>
        /// 当用户右键单击通知区域中的图标时发生。
        /// </summary>
        public abstract event EventHandler<MouseEventArgs>? RightClick;

        /// <summary>
        /// 在任务栏中显示具有指定标题、文本和图标的气球状提示（仅支持 Windows）。
        /// </summary>
        /// <param name="tipTitle">提示标题。</param>
        /// <param name="tipText">提示文本。</param>
        /// <param name="tipIcon">提示图标。</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public virtual void ShowBalloonTip(string tipTitle, string tipText, ToolTipIcon tipIcon)
            => throw new PlatformNotSupportedException();

        /// <summary>
        /// 隐藏在任务栏中的气球提示（仅支持 Windows）。
        /// </summary>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public virtual void HideBalloonTip()
            => throw new PlatformNotSupportedException();

        bool disposedValue;
        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    ContextMenuStrip.Items.CollectionChanged -= Items_CollectionChanged;
                    ContextMenuStrip.Items.Clear();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~NotifyIcon()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public class MouseEventArgs : EventArgs
        {
            /// <summary>
            /// 提供要用于没有事件数据的事件的值。
            /// </summary>
            public static new readonly MouseEventArgs Empty = new();

            /// <summary>
            /// 获取鼠标在产生鼠标事件时的 x 坐标。
            /// </summary>
            public int X { get; }

            /// <summary>
            /// 获取鼠标在产生鼠标事件时的 y 坐标。
            /// </summary>
            public int Y { get; }

            /// <summary>
            /// 初始化 <see cref="MouseEventArgs"/> 类的新实例。
            /// </summary>
            public MouseEventArgs()
            {

            }

            /// <summary>
            /// 初始化 <see cref="MouseEventArgs"/> 类的新实例。
            /// </summary>
            /// <param name="x">鼠标单击的 x 坐标（以像素为单位）。</param>
            /// <param name="y">鼠标单击的 y 坐标（以像素为单位）。</param>
            public MouseEventArgs(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}