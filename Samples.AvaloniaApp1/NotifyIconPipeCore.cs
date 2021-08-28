using System;
using System.Threading;
using System.Threading.Tasks;

namespace Samples.AvaloniaApp1
{
    abstract class NotifyIconPipeCore : IDisposable
    {
        public const string CommandExit = "Exit";
        public const string CommandNotifyIconClick = "NotifyIconClick";

        protected readonly CancellationTokenSource cts = new();

        bool isStarted;

        /// <summary>
        ///
        /// </summary>
        public void OnStart()
        {
            if (isStarted) return;
            isStarted = true;
            Task.Run(OnStartCore);
        }

        protected abstract void OnStartCore();

        bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    cts.Cancel();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~NotifyIconPipeCore()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}