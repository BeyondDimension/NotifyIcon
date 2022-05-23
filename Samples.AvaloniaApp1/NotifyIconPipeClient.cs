using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Windows;
using GtkApplication = Gtk.Application;

namespace Samples.AvaloniaApp1
{
    sealed class NotifyIconPipeClient : NotifyIconPipeCore
    {
        Process? mainProcess;
        readonly string pipeHandleAsString;
        readonly ConcurrentQueue<string> queue = new();

        public NotifyIconPipeClient(string pipeHandleAsString, string mainProcessIdStr)
        {
            this.pipeHandleAsString = pipeHandleAsString;
            if (int.TryParse(mainProcessIdStr, out var mainProcessId))
            {
                mainProcess = Process.GetProcessById(mainProcessId);
                mainProcess.Exited += MainProcess_Exited;
            }
        }

        private void MainProcess_Exited(object? sender, EventArgs e) => Dispose();

        protected override void OnStartCore()
        {
            try
            {
                using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandleAsString);
                using var sw = new StreamWriter(pipeClient);
                sw.AutoFlush = true;

                while (true)
                {
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        if (mainProcess == null || mainProcess.HasExited) break;

                        if (queue.TryDequeue(out var result))
                        {
                            sw.WriteLine(result);
                            if (result == CommandExit) break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        sw.WriteLine(CommandExit);
                        break;
                    }
                }
            }
            catch (Exception)
            {

            }

            Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void Append(string value)
        {
            if (!disposedValue && !cts.IsCancellationRequested)
            {
                queue.Enqueue(value);
            }
        }

        bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (mainProcess != null)
                    {
                        mainProcess.Exited -= MainProcess_Exited;
                        mainProcess = null;
                    }

                    Program.Services.GetRequiredService<NotifyIcon>().Dispose();

                    if (OperatingSystem2.IsLinux())
                    {
                        GtkApplication.Quit();
                    }
                    if (OperatingSystem2.IsWindows())
                    {
                        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                desktop.Shutdown();
                            }, DispatcherPriority.MaxValue);
                        }
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
            base.Dispose(disposing);
        }
    }
}