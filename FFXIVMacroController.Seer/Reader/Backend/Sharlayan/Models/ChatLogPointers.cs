using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Models
{
    internal class ChatLogPointers
    {
        public uint LineCount { get; set; }

        public long LogEnd { get; set; }

        public long LogNext { get; set; }

        public long LogStart { get; set; }

        public long OffsetArrayEnd { get; set; }

        public long OffsetArrayPos { get; set; }

        public long OffsetArrayStart { get; set; }
    }
}
