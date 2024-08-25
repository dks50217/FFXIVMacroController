using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Seer.Events
{
    public sealed class EnsembleNone : SeerEvent
    {
        public EnsembleNone(EventSource readerBackendType, string[] chatLog) : base(readerBackendType)
        {
            EventType = GetType();
            ChatLog = chatLog;
        }

        public string[] ChatLog { get; }

        public override bool IsValid() => ChatLog.Count() > 0;

    }
}
