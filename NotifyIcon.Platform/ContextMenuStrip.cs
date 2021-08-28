using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace System.Windows
{
    /// <summary>
    /// 表示快捷菜单。
    /// </summary>
    public sealed class ContextMenuStrip
    {
        internal ContextMenuStrip()
        {
            Items = new();
        }

        /// <summary>
        /// 获取或设置菜单项。
        /// </summary>
        public ObservableCollection<MenuItem> Items { get; }

        /// <summary>
        /// 表示 Menu 内某个可选择的项。
        /// </summary>
        public sealed class MenuItem : INotifyPropertyChanged
        {
            /// <summary>
            /// 获取或设置与菜单项关联的命令。
            /// </summary>
            public ICommand? Command { get; set; }

            /// <summary>
            /// 获取或设置要传递给 <see cref="Command"/> 的 <see cref="MenuItem"/> 属性的参数。
            /// </summary>
            public object? CommandParameter { get; set; }

            /// <summary>
            /// 获取或设置显示在 MenuItem 中的图标。
            /// </summary>
            public object? Icon
            {
                get => _Icon;
                set
                {
                    if (_Icon != value)
                    {
                        _Icon = value;
                        NotifyPropertyChanged(nameof(Icon));
                    }
                }
            }
            object? _Icon;

            /// <summary>
            /// 获取或设置一个值，通过该值指示菜单项标题。
            /// </summary>
            public string? Text
            {
                get => _Text;
                set
                {
                    if (_Text != value)
                    {
                        _Text = value;
                        NotifyPropertyChanged(nameof(Text));
                    }
                }
            }
            string? _Text;

            /// <summary>
            /// 获取或设置一个值，通过该值指示菜单项是否可见。
            /// </summary>
            public bool Visible
            {
                get => _Visible;
                set
                {
                    if (_Visible != value)
                    {
                        _Visible = value;
                        NotifyPropertyChanged(nameof(Visible));
                    }
                }
            }
            bool _Visible = true;

            /// <summary>
            /// 当单击菜单项或使用为该菜单项定义的快捷键或访问键选择菜单项时发生。
            /// </summary>
            public event EventHandler? Click;

            /// <summary>
            ///
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            void NotifyPropertyChanged(string propertyName)
                => PropertyChanged?.Invoke(this, new(propertyName));

            internal void OnClick(object? sender, EventArgs e)
            {
                if (Command != null)
                {
                    if (Command.CanExecute(CommandParameter))
                    {
                        Command.Execute(CommandParameter);
                    }
                }
                Click?.Invoke(sender, e);
            }
        }

        /// <summary>
        /// 当快捷菜单折叠时发生。
        /// </summary>
        public event EventHandler? Collapse;

        /// <summary>
        /// 在快捷菜单显示之前发生。
        /// </summary>
        public event EventHandler? Popup;

        internal void OnCollapse(object? sender, EventArgs e)
            => Collapse?.Invoke(sender, e);

        internal void OnPopup(object? sender, EventArgs e)
            => Popup?.Invoke(sender, e);
    }
}