using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Models
{
    
    
    internal class ChatLogItem
    {
        public byte[] Bytes { get; set; }

        public string Code { get; set; }

        public string Combined { get; set; }

        public bool JP { get; set; }

        public string Line { get; set; }

        public string Raw { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}
