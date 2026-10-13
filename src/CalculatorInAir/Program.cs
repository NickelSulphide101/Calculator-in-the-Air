using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace CalculatorInAir
{
    public static class Program
    {
        private static Mutex? _mutex;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int dwProcessId);

        public static uint WakeupMessage { get; private set; } = 0x0400 + 101;

        private static string GetMutexName()
        {
            string user = Environment.UserName;
            return $"Local\\CalculatorInTheAir_{user}_984F-4B8A-A2E4";
        }

        [STAThread]
        public static void Main()
        {
            // Register collision-free unique window message for single-instance IPC
            uint registeredMsg = RegisterWindowMessage("CalculatorInAir_Wakeup_984F4B8AA2E4");
            if (registeredMsg != 0)
            {
                WakeupMessage = registeredMsg;
            }

            // Register global exception logger
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    SettingsManager.LogException(ex);
                    System.Windows.MessageBox.Show(
                        $"A fatal error occurred:\n\n{ex.Message}\n\nDetails have been logged to the application crash log.",
                        "Calculator in the Air - Fatal Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            };

            // Try to acquire the system mutex
            bool isNewInstance = false;
            try
            {
                _mutex = new Mutex(true, GetMutexName(), out isNewInstance);
            }
            catch (AbandonedMutexException)
            {
                isNewInstance = true;
            }

            if (!isNewInstance)
            {
                bool instanceFound = false;
                try
                {
                    // If an instance already exists, wake it up by posting WakeupMessage to windows of verified process
                    int currentPid = Environment.ProcessId;
                    string currentExePath = Environment.ProcessPath ?? "";
                    var processes = Process.GetProcessesByName("CalculatorInAir");

                    try
                    {
                        foreach (var p in processes)
                        {
                            if (p.Id != currentPid)
                            {
                                try
                                {
                                    // Strictly validate that the target process is running from the exact same executable path (fail-closed)
                                    string? targetPath = p.MainModule?.FileName;
                                    if (!string.IsNullOrEmpty(currentExePath) && !string.IsNullOrEmpty(targetPath) &&
                                        string.Equals(Path.GetFullPath(currentExePath), Path.GetFullPath(targetPath), StringComparison.OrdinalIgnoreCase))
                                    {
                                        instanceFound = true;
                                        AllowSetForegroundWindow(p.Id);
                                        EnumWindows((hWnd, lParam) =>
                                        {
                                            GetWindowThreadProcessId(hWnd, out uint pid);
                                            if (pid == p.Id)
                                            {
                                                PostMessage(hWnd, WakeupMessage, IntPtr.Zero, IntPtr.Zero);
                                            }
                                            return true;
                                        }, IntPtr.Zero);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                    finally
                    {
                        // Clean up all process handles
                        foreach (var p in processes)
                        {
                            try { p.Dispose(); } catch { }
                        }
                    }
                }
                catch { }
                finally
                {
                    if (instanceFound)
                    {
                        // Release mutex handle and exit second process only if genuine instance was verified and messaged
                        _mutex?.Dispose();
                    }
                }

                if (instanceFound)
                {
                    return;
                }
                // If mutex was held but no genuine instance was running (e.g. mutex squatting / orphaned handle), continue launch
            }

            try
            {
                var app = new App();
                app.Run();
            }
            finally
            {
                if (_mutex != null)
                {
                    try { _mutex.ReleaseMutex(); } catch { }
                    _mutex.Dispose();
                }
            }
        }
    }
}
