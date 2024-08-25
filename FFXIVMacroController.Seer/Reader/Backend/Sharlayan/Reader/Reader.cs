/*
 * Copyright(c) 2007-2020 Ryan Wilson syndicated.life@gmail.com (http://syndicated.life/)
 * Licensed under the MIT license. See https://github.com/FFXIVAPP/sharlayan/blob/master/LICENSE.md for full license information.
 */

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Models;
using System.Linq;
using FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Core;

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Reader;

internal partial class Reader
{
    public Reader(MemoryHandler memoryHandler)
    {
        Scanner              = memoryHandler.Scanner;
        MemoryHandler        = memoryHandler;
        MemoryHandler.Reader = this;
        _chatLogReader = new ChatLogReader(memoryHandler);
    }

    public Scanner Scanner { get; set; }

    public MemoryHandler MemoryHandler { get; set; }

    private readonly ChatLogReader _chatLogReader;

    private bool _chatLogFirstRun = true;

    public bool CanGetChatLog() => Scanner.Locations.ContainsKey(Signatures.ChatLogKey);

    public ChatLogResult GetChatLog(int previousArrayIndex = 0, int previousOffset = 0)
    {
        var result = new ChatLogResult();

        if (!CanGetChatLog() || !MemoryHandler.IsAttached) return result;

        _chatLogReader.PreviousArrayIndex = previousArrayIndex;
        _chatLogReader.PreviousOffset = previousOffset;

        var chatPointerMap = (IntPtr)Scanner.Locations[Signatures.ChatLogKey];

        if (chatPointerMap.ToInt64() <= 20)
        {
            return result;
        }

        var buffered = new List<List<byte>>();

        try
        {
            _chatLogReader.ChatLogPointers = new ChatLogPointers
            {
                LineCount = (uint)MemoryHandler.GetPlatformUInt(chatPointerMap),
                OffsetArrayStart = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.OffsetArrayStart),
                OffsetArrayPos = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.OffsetArrayPos),
                OffsetArrayEnd = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.OffsetArrayEnd),
                LogStart = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.LogStart),
                LogNext = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.LogNext),
                LogEnd = MemoryHandler.GetPlatformUInt(chatPointerMap, MemoryHandler.Structures.ChatLogPointers.LogEnd)
            };

            var currentArrayIndex = (_chatLogReader.ChatLogPointers.OffsetArrayPos -
                                     _chatLogReader.ChatLogPointers.OffsetArrayStart) / 4;
            if (_chatLogFirstRun)
            {
                _chatLogFirstRun = false;
                _chatLogReader.EnsureArrayIndexes();

                if (currentArrayIndex - 1 > 0 && _chatLogReader.Indexes.Count >= currentArrayIndex - 1) _chatLogReader.PreviousOffset = _chatLogReader.Indexes[(int)currentArrayIndex - 1];
                else
                {
                    _chatLogReader.PreviousOffset = 0;
                    _chatLogReader.PreviousArrayIndex = 0;
                    result.PreviousArrayIndex = _chatLogReader.PreviousArrayIndex;
                    result.PreviousOffset = _chatLogReader.PreviousOffset;
                    return result; // The player is logged out.
                }

                _chatLogReader.PreviousArrayIndex = (int)currentArrayIndex - 1;
            }
            else
            {
                if (currentArrayIndex < _chatLogReader.PreviousArrayIndex)
                {
                    _chatLogReader.PreviousOffset = 0;
                    _chatLogReader.PreviousArrayIndex = 0;
                    result.PreviousArrayIndex = _chatLogReader.PreviousArrayIndex;
                    result.PreviousOffset = _chatLogReader.PreviousOffset;
                    return result; // The player logged out.
                }

                if (_chatLogReader.PreviousArrayIndex < currentArrayIndex)
                {
                    buffered.AddRange(_chatLogReader.ResolveEntries(_chatLogReader.PreviousArrayIndex, (int)currentArrayIndex));
                }

                _chatLogReader.PreviousArrayIndex = (int)currentArrayIndex;
            }
        }
        catch (Exception ex)
        {
            MemoryHandler?.RaiseException(ex);
        }

        foreach (var bytes in buffered.Where(b => b.Count > 0))
        {
            try
            {
                var chatLogEntry = ChatEntry.Process(MemoryHandler, bytes.ToArray());
                if (Regex.IsMatch(chatLogEntry.Combined, @"[\w\d]{4}::?.+"))
                {
                    result.ChatLogItems.Add(chatLogEntry);
                }
            }
            catch (Exception ex)
            {
                MemoryHandler?.RaiseException(ex);
            }
        }

        result.PreviousArrayIndex = _chatLogReader.PreviousArrayIndex;
        result.PreviousOffset = _chatLogReader.PreviousOffset;

        return result;
    }
}