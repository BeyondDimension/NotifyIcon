# NotifyIcon
在通知区域创建图标的组件。

## 包引用方式（二选一）
1. 运行时自动匹配平台实现 [![NuGet version (NotifyIcon)](https://img.shields.io/nuget/v/NotifyIcon.svg)](https://www.nuget.org/packages/NotifyIcon/)
    - ```Install-Package NotifyIcon```
2. 仅单个平台实现
    - Windows [![NuGet version (NotifyIcon.Windows)](https://img.shields.io/nuget/v/NotifyIcon.Windows.svg)](https://www.nuget.org/packages/NotifyIcon.Windows/)
        - ```Install-Package NotifyIcon.Windows```
    - Linux [![NuGet version (NotifyIcon.Linux)](https://img.shields.io/nuget/v/NotifyIcon.Linux.svg)](https://www.nuget.org/packages/NotifyIcon.Linux/)
        - ```Install-Package NotifyIcon.Linux```
    - macOS [![NuGet version (NotifyIcon.Mac)](https://img.shields.io/nuget/v/NotifyIcon.Mac.svg)](https://www.nuget.org/packages/NotifyIcon.Mac/)
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
1. 直接使用
    - ```NotifyIcon notifyIcon = NotifyIcon.Create();```
2. 通过 **Microsoft.Extensions.DependencyInjection** 使用
    - 添加到依赖注入服务中
        - ```services.AddSingleton(typeof(NotifyIcon), NotifyIcon.ImplType);```

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
