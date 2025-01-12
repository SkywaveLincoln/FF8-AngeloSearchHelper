using System;
using System.Windows.Forms; // for Keys enum
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FF8AngeloSearch
{
    public static class KeyboardHook
    {
        // Public event so the UI can listen for key presses
        public static event Action<Keys> KeyPressed;

        // 1. Define the callback delegate
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        // 2. Keep references to delegates & hook handles to avoid GC
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // 3. Constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN     = 0x0100;
        private const int WM_KEYUP       = 0x0101;

        // 4. Import WinAPI functions
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// 5. Public method to install the hook
        /// </summary>
        public static void InstallHook()
        {
            if (_hookID == IntPtr.Zero)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    _hookID = SetWindowsHookEx(
                        WH_KEYBOARD_LL,
                        _proc,
                        GetModuleHandle(curModule.ModuleName),
                        0);
                }
            }
        }

        /// <summary>
        /// 6. Public method to remove the hook
        /// </summary>
        public static void UninstallHook()
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 7. The actual callback function
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msgType = wParam.ToInt32();
                if (msgType == WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    KeyPressed?.Invoke((Keys)vkCode);
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
    }
}
