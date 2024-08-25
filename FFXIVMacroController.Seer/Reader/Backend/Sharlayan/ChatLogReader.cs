using FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan
{
    internal class ChatLogReader
    {
        public readonly List<int> Indexes = new();

        public ChatLogPointers ChatLogPointers;

        public int PreviousArrayIndex;

        public int PreviousOffset;

        private readonly MemoryHandler _memoryHandler;
        public ChatLogReader(MemoryHandler memoryHandler)
        {
            _memoryHandler = memoryHandler;
        }

        private const int BUFFER_SIZE = 4000;

        public void EnsureArrayIndexes()
        {
            Indexes.Clear();

            var indexes = _memoryHandler.GetByteArray(new IntPtr(ChatLogPointers.OffsetArrayStart), BUFFER_SIZE);

            for (var i = 0; i < BUFFER_SIZE; i += 4)
            {
                Indexes.Add(BitConverter.ToInt32(indexes, i));
            }
        }

        public IEnumerable<List<byte>> ResolveEntries(int offset, int length)
        {
            var entries = new List<List<byte>>();
            for (var i = offset; i < length; i++)
            {
                EnsureArrayIndexes();
                var currentOffset = Indexes[i];
                entries.Add(ResolveEntry(PreviousOffset, currentOffset));
                PreviousOffset = currentOffset;
            }
            return entries;
        }

        private List<byte> ResolveEntry(int offset, int length) => new(_memoryHandler.GetByteArray(new IntPtr(ChatLogPointers.LogStart + offset), length - offset));
    }
}
