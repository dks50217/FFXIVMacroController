/*
 * Copyright(c) 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using FFXIVMacroController.Quotidian;

namespace FFXIVMacroController.Grunt;

public class BmpGruntException : BmpException
{
    internal BmpGruntException()
    {
    }
    internal BmpGruntException(string message) : base(message)
    {
    }
}