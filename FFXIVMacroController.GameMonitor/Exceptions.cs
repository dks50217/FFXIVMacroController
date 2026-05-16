/*
 * Copyright(c) 2022 MoogleTroupe, trotlinebeercan, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using FFXIVMacroController.Common;
using FFXIVMacroController.GameMonitor.Events;

namespace FFXIVMacroController.GameMonitor;

public class BmpSeerException : BmpException
{
    internal BmpSeerException() { }

    internal BmpSeerException(string message) : base(message) { }
}

public class BmpSeerGamePathException : BmpSeerException
{
    internal BmpSeerGamePathException(string message) : base(message) { }
}

public class BmpSeerEnvironmentTypeException : BmpSeerException
{
    internal BmpSeerEnvironmentTypeException(string message) : base(message) { }
}

public class BmpSeerGameRegionException : BmpSeerException
{
    internal BmpSeerGameRegionException(string message) : base(message) { }
}

public class BmpSeerConfigPathException : BmpSeerException
{
    internal BmpSeerConfigPathException(string message) : base(message) { }
}

public class BmpSeerBackendAlreadyRunningException : BmpSeerException
{
    internal BmpSeerBackendAlreadyRunningException(int pid, EventSource readerBackendType) : base("Backend " +
                                                                                                  readerBackendType + " already running for pid " + pid)
    {
    }
}
