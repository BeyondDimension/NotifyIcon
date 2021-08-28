# NotifyIcon
在通知区域创建图标的组件。

## 包引用方式（二选一）
1. 运行时自动匹配平台实现
    - ```Install-Package NotifyIcon```
2. 仅单个平台实现
    - Windows
        - ```Install-Package NotifyIcon.Windows```
    - Linux
        - ```Install-Package NotifyIcon.Linux```
    - macOS
        - ```Install-Package NotifyIcon.Mac```

## 平台支持与目标框架
- Windows
    - .NET Framework 3.5 ~ 4.8+
    - .NET Core 2.0+
    - .NET 5+
    - .NET Standard 2.0+
- Linux
    - .NET Framework 4.6.1+ / Mono 5.4+
    - .NET Core 2.0+
    - .NET 5+
    - .NET Standard 2.0+
- macOS
    - .NET Framework 4.6.1+ / Mono 5.4+
    - .NET Core 2.0+
    - .NET 5+
    - .NET Standard 2.0+

## 使用方式
1. 在以下目标框架仅支持通过 **Microsoft.Extensions.DependencyInjection** 使用
    - 目标框架支持
        - .NET Framework 4.5+ / Mono 4.6+
        - .NET Core 1.0+
        - .NET 5+
        - .NET Standard 1.1+
    - 添加到依赖注入服务中
        - ```services.AddNotifyIcon();```
2. 在不支持 **Microsoft.Extensions.DependencyInjection** 的目标框架上使用
    - ```NotifyIcon notifyIcon = NotifyIconFactory.Create();```

## 添加右键菜单项
```
notifyIcon.ContextMenuStrip.Items.Add(new ContextMenuStrip.MenuItem() { Text = "Menu Text 01", Command = ReactiveCommand.Create(() =>
{
    // ...
})});
```

## 示例项目
| Platform                    | ProjectFileName                     |
| --------------------------- | ----------------------------------- |
| NotifyIcon for WPF          | Samples.WpfApp1.csproj              |
| NotifyIcon for Console      | Samples.ConsoleApp1.csproj          |
| NotifyIcon for Avalonia     | Samples.AvaloniaApp1.csproj         |