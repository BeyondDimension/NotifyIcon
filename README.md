# NotifyIcon

## 包引用方式
1. 运行时自动匹配平台实现
    - ```Install-Package NotifyIcon```
2. 单平台
    - Windows
        - ```Install-Package NotifyIcon.Windows```
    - Linux
        - ```Install-Package NotifyIcon.Linux```
    - macOS
        - ```Install-Package NotifyIcon.Mac```

## 使用方式
1. 通过 Microsoft.Extensions.DependencyInjection 使用
    - ```services.AddNotifyIcon();```
2. 直接使用
    - ```NotifyIcon notifyIcon = NotifyIconFactory.Create();```

## 添加右键菜单项
```
notifyIcon.ContextMenuStrip.Items.Add(new ContextMenuStrip.MenuItem() { Text = "Menu Text 01", Command = ReactiveCommand.Create(() =>
{
    // ...
})});
```