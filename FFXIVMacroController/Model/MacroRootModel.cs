﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Model
{
    public class MacroRootModel
    {
        public List<MacroModel> macroList { get; set; }
        public int repeat { get; set; }
    }
}