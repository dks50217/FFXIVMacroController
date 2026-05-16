/*
 * Copyright(c) 2022 MoogleTroupe, trotlinebeercan, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FFXIVMacroController.GameMonitor.Events;

namespace FFXIVMacroController.GameMonitor;

public partial class BmpSeer : IDisposable
{
    private static readonly Lazy<BmpSeer> LazyInstance = new(() => new BmpSeer());

    /// <summary>
    ///
    /// </summary>
    public bool Started { get; private set; }

    private readonly ConcurrentDictionary<int, Game> _games;

    private BmpSeer() { _games = new ConcurrentDictionary<int, Game>(); }

    public static BmpSeer Instance => LazyInstance.Value;

    /// <summary>
    /// Current active games
    /// </summary>
    public IReadOnlyDictionary<int, Game> Games => new ReadOnlyDictionary<int, Game>(_games);

    /// <summary>
    /// Start Seer monitoring.
    /// </summary>
    public void Start()
    {
        if (Started) return;
        StartEventsHandler();
        StartProcessWatcher();
        Started = true;
    }

    /// <summary>
    /// Stop Seer monitoring.
    /// </summary>
    public void Stop()
    {
        if (!Started) return;

        StopProcessWatcher();
        StopEventsHandler();

        foreach (var game in _games.Values)
        {
            game?.Dispose();
        }

        _games.Clear();

        Started = false;
    }

    ~BmpSeer() { Dispose(); }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}
