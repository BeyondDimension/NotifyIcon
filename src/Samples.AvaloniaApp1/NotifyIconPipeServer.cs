using System.Diagnostics;
using System.IO.Pipes;

namespace Samples.AvaloniaApp1;

sealed class NotifyIconPipeServer : NotifyIconPipeCore
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="handle"></param>
    /// <returns></returns>
    static string GetArguments(string handle)
        => $"{Program.ArgsNotifyIcon} {handle} {Environment.ProcessId}";

    bool HandlerCommand(string command)
    {
        switch (command)
        {
            case CommandExit:
                Program.Shutdown();
                return true;
            case CommandNotifyIconClick:
                App.Current?.OnNotifyIconClick(this, EventArgs.Empty);
                break;
        }
        return false;
    }

    protected override void OnStartCore()
    {
        Process pipeClient = new();

        var mainModule = Process.GetCurrentProcess().MainModule;
        if (mainModule == null)
            throw new ArgumentNullException(nameof(mainModule));
        pipeClient.StartInfo.FileName = mainModule.FileName;

        using (var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
        {
            // Pass the client process a handle to the server.
            pipeClient.StartInfo.Arguments =
                GetArguments(pipeServer.GetClientHandleAsString());
            pipeClient.StartInfo.UseShellExecute = false;
            pipeClient.Start();

            pipeServer.DisposeLocalCopyOfClientHandle();

            try
            {
                // Read user input and send that to the client process.
                using var sr = new StreamReader(pipeServer);

                // Display the read text to the console
                string? temp;

                // Read the server data and echo to the console.
                while ((temp = sr.ReadLine()) != null)
                {
                    try
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        if (HandlerCommand(temp)) break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch (IOException)
            {
            }
        }

        pipeClient.WaitForExit();
        pipeClient.Close();
    }
}