using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Model
{
    public class CategoryModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
        public int repeat { get; set; }
        public List<MacroModel> macroList { get; set; }
    }
}
