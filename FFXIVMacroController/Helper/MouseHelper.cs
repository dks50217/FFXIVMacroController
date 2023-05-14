using Machina.FFXIV;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace FFXIVMacroController.Helper
{
    public class MouseHelper
    {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        private const int VK_MENU = 0x12; // Alt key
        private const int L_Ctrl = 0xA2;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(
        int hWnd,               // handle to destination window
        int Msg,                // message
        int wParam,             // first message parameter
        int lParam);            // second message parameter


        public static (int, int) LocateMouse(nint targetProcessId)
        {
            // Wait for the Alt key press
            Console.WriteLine("Press Alt to get the mouse position.");
            while (true)
            {
                if (IsKeyPressed(L_Ctrl))
                    break;
            }

            // Retrieve the current mouse position
            POINT mousePosition;
            GetCursorPos(out mousePosition);
            ScreenToClient(targetProcessId, ref mousePosition);

            RECT AppRECT = new RECT();

            GetWindowRect(targetProcessId, ref AppRECT);


            // Display the mouse position
            Console.WriteLine($"Mouse position: X={mousePosition.X}, Y={mousePosition.Y}");

            return (mousePosition.X, mousePosition.Y);
        }

        private static bool IsKeyPressed(int keyCode)
        {
            const int KEY_PRESSED = 0x8000;
            return (GetAsyncKeyState(keyCode) & KEY_PRESSED) != 0;
        }

        static IntPtr MakeLParam(int x, int y) => (IntPtr)((y << 16) | (x & 0xFFFF));

        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_MOUSEMOVE = 0x0200;
        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;
        const int WM_ACTIVATE = 0x0006;

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void RunMouseClick(nint targetProcessId, int x, int y)
        {
            var pointPtr = MakeLParam(x, y);
            //SendMessage(targetProcessId, WM_MOUSEMOVE, 0, MakeLParam(X, Y));
            //SendMessage(targetProcessId, WM_ACTIVATE, 1, 0);
            SendMessage(targetProcessId, WM_LBUTTONDOWN, 1, pointPtr);
            SendMessage(targetProcessId, WM_LBUTTONUP,0, pointPtr);
        }

        static void SetCursorPositionByProcess(int processId, int x, int y)
        {
            Process process = Process.GetProcessById(processId);
            IntPtr mainWindowHandle = process.MainWindowHandle;

            if (mainWindowHandle != IntPtr.Zero)
            {
                //// Bring the target process window to the foreground
                //SetForegroundWindow(mainWindowHandle);

                //// Calculate the coordinates relative to the target window
                //RECT windowRect;
                //GetWindowRect(mainWindowHandle, out windowRect);
                //int targetX = windowRect.Left + x;
                //int targetY = windowRect.Top + y;

                //// Set the cursor position
                //SetCursorPos(targetX, targetY);
            }
            else
            {
                Console.WriteLine("Failed to find the target process or its main window.");
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
