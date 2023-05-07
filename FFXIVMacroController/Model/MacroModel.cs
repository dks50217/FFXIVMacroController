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
    }

    public enum Types
    {
        button = 1,
        mouse = 2
    }
}
