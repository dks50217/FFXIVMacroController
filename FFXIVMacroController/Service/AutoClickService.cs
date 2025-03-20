using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroControllerApp.Service
{
    public interface IAutoClickService
    {
        void MoveAndClick(IntPtr hWnd, int x, int y);
    }

    public class AutoClickService : IAutoClickService
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public void MoveAndClick(IntPtr hWnd, int x, int y)
        {
            // 將視窗恢復，如果視窗最小化
            ShowWindow(hWnd, SW_RESTORE);

            // 將視窗內的相對座標轉換為螢幕絕對座標
            Point point = new Point(x, y);
            if (!ClientToScreen(hWnd, ref point))
            {
                throw new InvalidOperationException("無法將座標轉換為螢幕座標");
            }

            // 移動滑鼠到目標位置
            if (!SetCursorPos(point.X, point.Y))
            {
                throw new InvalidOperationException("無法移動滑鼠到指定位置");
            }

            // 模擬滑鼠左鍵按下和抬起
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
    }
}
