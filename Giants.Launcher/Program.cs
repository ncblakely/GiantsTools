using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

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
                    string appName = Process.GetCurrentProcess().MainModule.FileName;
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


                Form form;
                try
                {
                    form = new LauncherForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        text: string.Format(Resources.LauncherFatalError, ex.Message),
                        caption: Resources.Error,
                        buttons: MessageBoxButtons.OK,
                        icon: MessageBoxIcon.Error);

                    Application.Exit();
                    return;
                }

                Application.Run(form);

            }
        }
    }
}
