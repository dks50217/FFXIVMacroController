using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Models
{
    internal class ChatLogResult
    {
        public List<ChatLogItem> ChatLogItems { get; } = new();

        public int PreviousArrayIndex { get; set; }

        public int PreviousOffset { get; set; }
    }
}
