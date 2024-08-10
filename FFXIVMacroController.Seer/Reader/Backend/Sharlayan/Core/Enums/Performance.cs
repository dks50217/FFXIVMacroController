/*
 * Copyright(c) 2023 MoogleTroupe, 2018-2020 parulina
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan.Core.Enums;

internal class Performance
{
    public enum Status : byte
    {
        Closed,
        Loading,
        Opened,
        SwitchingNote,
        HoldingNote
    }
}