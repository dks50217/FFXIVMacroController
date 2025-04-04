/*
 * Copyright(c) 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using FFXIVMacroController.Seer;

namespace FFXIVMacroController.Grunt;

public class BmpGrunt
{
    private static readonly Lazy<BmpGrunt> LazyInstance = new(() => new BmpGrunt());

    /// <summary>
    /// 
    /// </summary>
    public bool Started { get; private set; }

    private BmpGrunt()
    {
    }

    public static BmpGrunt Instance => LazyInstance.Value;

    /// <summary>
    /// Start Grunt.
    /// </summary>
    public void Start()
    {
        if (Started) return;
        if (!BmpSeer.Instance.Started) throw new BmpGruntException("Grunt requires Seer to be running.");
        Started       = true;
    }

    /// <summary>
    /// Stop Grunt.
    /// </summary>
    public void Stop()
    {
        if (!Started) return;
        Started       = false;
    }

    ~BmpGrunt() => Dispose();
    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}