using FFXIVMacroController.Quotidian.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Model
{
    public class MacroModel
    {
        public Keys key { get; set; }
        public int sleep { get; set; }
        public Types type { get; set; }
        public int coordinateX { get; set; }
        public int coordinateY { get; set; }
        public string? coordinate => $"({coordinateX},{coordinateY})";
        public string? keyName => Enum.GetName(key);
    }

    public enum Types
    {
        button = 1,
        mouse = 2
    }
}
