﻿/*
 * Copyright(c) 2023 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

namespace FFXIVMacroController.Seer.Reader.Backend.Sharlayan;

public class BmpSeerSharlayanSigException : BmpSeerException
{
    public BmpSeerSharlayanSigException(string message) : base("Unable to find memory signature for: " + message)
    {
    }
}