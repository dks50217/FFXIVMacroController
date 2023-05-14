using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace FFXIVMacroController.Helper
{
    public class ClickOnPointTool
    {
        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

#pragma warning disable 649
        internal struct INPUT
        {
            public UInt32 Type;
            public MOUSEKEYBDHARDWAREINPUT Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }

        internal struct MOUSEINPUT
        {
            public Int32 X;
            public Int32 Y;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr ExtraInfo;
        }

#pragma warning restore 649

        static IntPtr MakeLParam(int x, int y) => (IntPtr)((y << 16) | (x & 0xFFFF));

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        const int WM_LBUTTONDOWN = 0x201;
        const int WM_LBUTTONUP = 0x202;

        public static void ClickOnPoint(IntPtr wndHandle, int x, int y)
        {
            POINT lpPoint;
           
            GetCursorPos(out lpPoint);

            var oldHandle = GetForegroundWindow();

            Point clientPoint = new Point(x, y);

            ClientToScreen(wndHandle, ref clientPoint);

            SetCursorPos(clientPoint.X, clientPoint.Y);

            SetForegroundWindow(wndHandle);

            // 切换窗口后，等待窗口切换完成
            while (GetForegroundWindow() != wndHandle)
            {
                Thread.Sleep(20);
            }

            var pointPtr = MakeLParam(x, y);

            Thread.Sleep(300);

            SendMessage(wndHandle, WM_LBUTTONDOWN, 1, pointPtr);

            Thread.Sleep(300);

            SendMessage(wndHandle, WM_LBUTTONUP, 0, pointPtr);

            Thread.Sleep(300);

            SetForegroundWindow(oldHandle);

            SetCursorPos(lpPoint.X, lpPoint.Y);
        }
        public static bool SendMessage(nint wndHandle, nint pointPtr)
        {
            SendMessage(wndHandle, WM_LBUTTONDOWN, 1, pointPtr);

            SendMessage(wndHandle, WM_LBUTTONUP, 0, pointPtr);




            return true;
        }
    }
}
