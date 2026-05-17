using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using OpenCvSharp;

namespace FFXIVMacroController.InputSender;

public static partial class GameExtensions
{
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN  = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP    = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP   = 0x0010;

    /// <summary>
    /// 截圖整個螢幕，用 OpenCV 模板比對找到圖片位置後點擊
    /// </summary>
    public static async Task<bool> LocateAndClick(string imagePath, double confidence = 0.8, bool rightClick = false)
    {
        if (!System.IO.File.Exists(imagePath))
        {
            Console.WriteLine($"[Mouse] 圖片不存在: {imagePath}");
            return false;
        }

        try
        {
            // 截圖
            var screenBounds = GetScreenBounds();
            using var screenshot = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size);
            }

            // 轉成 OpenCV Mat
            using var screenMat = BitmapToMat(screenshot);
            using var templateMat = Cv2.ImRead(imagePath, ImreadModes.Color);

            if (templateMat.Empty())
            {
                Console.WriteLine($"[Mouse] 無法讀取模板圖片: {imagePath}");
                return false;
            }

            // 模板比對
            using var result = new Mat();
            Cv2.MatchTemplate(screenMat, templateMat, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            Console.WriteLine($"[Mouse] 比對信度: {maxVal:F3} (需要 {confidence})");

            if (maxVal < confidence)
            {
                Console.WriteLine($"[Mouse] 找不到圖片（信度不足）: {imagePath}");
                return false;
            }

            // 計算中心點
            int cx = maxLoc.X + templateMat.Width / 2;
            int cy = maxLoc.Y + templateMat.Height / 2;

            Console.WriteLine($"[Mouse] 找到位置: ({cx}, {cy})");

            // 移動並點擊
            SetCursorPos(cx, cy);
            await Task.Delay(50);
            if (rightClick)
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, cx, cy, 0, IntPtr.Zero);
                await Task.Delay(100);
                mouse_event(MOUSEEVENTF_RIGHTUP, cx, cy, 0, IntPtr.Zero);
            }
            else
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, cx, cy, 0, IntPtr.Zero);
                await Task.Delay(100);
                mouse_event(MOUSEEVENTF_LEFTUP, cx, cy, 0, IntPtr.Zero);
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Mouse] 錯誤: {ex.Message}");
            return false;
        }
    }

    private static System.Drawing.Rectangle GetScreenBounds()
    {
        int width = 0, height = 0;
        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            width = Math.Max(width, screen.Bounds.Right);
            height = Math.Max(height, screen.Bounds.Bottom);
        }
        return new System.Drawing.Rectangle(0, 0, width, height);
    }

    private static Mat BitmapToMat(Bitmap bitmap)
    {
        var bmpData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try
        {
            var mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC4, bmpData.Scan0, bmpData.Stride);
            var bgr = new Mat();
            Cv2.CvtColor(mat, bgr, ColorConversionCodes.BGRA2BGR);
            return bgr;
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }
    }
}
