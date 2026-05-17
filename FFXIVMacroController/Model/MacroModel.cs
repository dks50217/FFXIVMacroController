using FFXIVMacroController.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroControllerApp.Model
{
    public class MacroModel
    {
        public Keys key { get; set; }
        public int keyNumber { get; set; }
        public int sleep { get; set; }
        public Types type { get; set; }
        public int typeNumber { get; set; }
        public string inputText { get; set; }
        public int coordinateX { get; set; }
        public int coordinateY { get; set; }
        public string? coordinate => $"({coordinateX},{coordinateY})";
        public string? keyName => Enum.GetName(key);
        public string? group { get; set; }
        public string? imagePath { get; set; }
        public decimal confidence { get; set; } = 0.8m;
        public MouseButton mouseButton { get; set; } = MouseButton.Left;
    }

    public enum Types
    {
        button = 1,
        mouse = 2,
        text = 3
    }

    public enum MouseButton
    {
        Left = 0,
        Right = 1
    }
}
