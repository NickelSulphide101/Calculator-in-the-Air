using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace CalculatorInAir
{
    public static class Program
    {
        private static Mutex? _mutex;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const string MutexName = "Local\\CalculatorInTheAirMutex-984F-4B8A-A2E4";
        private const int WM_USER_WAKEUP = 0x0400 + 101;

        [STAThread]
        public static void Main()
        {
            // Register global exception logger
            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                if (args.ExceptionObject is Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Fatal Application Exception:\n\n{ex.Message}\n\n{ex.StackTrace}",
                        "Calculator in the Air - Fatal Error",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error
                    );
                }
            };

            // Try to acquire the system mutex
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // If an instance already exists, wake it up by posting WM_USER_WAKEUP to all windows of the existing process
                int currentPid = Environment.ProcessId;
                var processes = Process.GetProcessesByName("CalculatorInAir");
                bool awakened = false;

                foreach (var p in processes)
                {
                    if (p.Id != currentPid)
                    {
                        EnumWindows((hWnd, lParam) =>
                        {
                            GetWindowThreadProcessId(hWnd, out uint pid);
                            if (pid == p.Id)
                            {
                                PostMessage(hWnd, WM_USER_WAKEUP, IntPtr.Zero, IntPtr.Zero);
                                awakened = true;
                            }
                            return true;
                        }, IntPtr.Zero);
                    }
                }

                if (!awakened)
                {
                    IntPtr hWnd = FindWindow(null, "Calculator in the Air");
                    if (hWnd != IntPtr.Zero)
                    {
                        PostMessage(hWnd, WM_USER_WAKEUP, IntPtr.Zero, IntPtr.Zero);
                    }
                }
                
                // Release mutex handle and exit second process
                return;
            }

            try
            {
                var app = new App();
                app.Run();
            }
            finally
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
    }
}
