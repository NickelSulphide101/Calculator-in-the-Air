using System;
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

        private const string MutexName = "Local\\CalculatorInTheAirMutex-984F-4B8A-A2E4";
        private const int WM_USER_WAKEUP = 0x0400 + 101;

        [STAThread]
        public static void Main()
        {
            // Try to acquire the system mutex
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);

            if (!isNewInstance)
            {
                // If it already exists, look for the running window and wake it up
                IntPtr hWnd = FindWindow(null, "Calculator in the Air");
                if (hWnd != IntPtr.Zero)
                {
                    PostMessage(hWnd, WM_USER_WAKEUP, IntPtr.Zero, IntPtr.Zero);
                }
                
                // Release mutex and exit the second process immediately
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
