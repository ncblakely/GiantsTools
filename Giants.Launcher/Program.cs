using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Giants.Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Mutex mutex = new Mutex(true, "GiantsLauncherMutex"))
            {
                if (!mutex.WaitOne(TimeSpan.Zero, true))
                {
                    // Another instance must be running, switch the first process we find with the same name to the foreground:
                    string appName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appName));

                    Process otherProcess = processes.FirstOrDefault(p => p.Id != Process.GetCurrentProcess().Id);
                    if (otherProcess != null)
                    {
                        NativeMethods.SetForegroundWindow(otherProcess.MainWindowHandle);
                    }

                    Application.Exit();

                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new LauncherForm());
            }
        }
    }
}
