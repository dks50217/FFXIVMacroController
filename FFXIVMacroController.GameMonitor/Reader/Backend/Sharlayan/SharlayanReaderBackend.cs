/*
 * Copyright(c) 2023 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using FFXIVMacroController.GameMonitor.Events;
using FFXIVMacroController.GameMonitor.Reader.Backend.Sharlayan.Events;
using FFXIVMacroController.GameMonitor.Reader.Backend.Sharlayan.Models;
using FFXIVMacroController.GameMonitor.Reader.Backend.Sharlayan.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FFXIVMacroController.GameMonitor.Reader.Backend.Sharlayan;

internal class SharlayanReaderBackend : IReaderBackend
{
    public EventSource ReaderBackendType { get; }

    public ReaderHandler ReaderHandler { get; set; }

    public int SleepTimeInMs { get; set; }

    public SharlayanReaderBackend(int sleepTimeInMs)
    {
        ReaderBackendType = EventSource.Sharlayan;
        SleepTimeInMs     = sleepTimeInMs;
        _signaturesFound  = false;
    }

    private Reader.Reader _reader;
    private bool _signaturesFound;

    private void InitializeSharlayan()
    {
        _lastScan = new ScanItems
        {
            FirstScan          = true,
            PreviousArrayIndex = 0,
            PreviousOffset     = 0,
            PlayerName         = "Unknown",
            IsLoggedIn         = false,
            ChatOpen           = false
        };
        _reader ??= new Reader.Reader(new MemoryHandler(new Scanner(), ReaderHandler.Game.GameRegion));
        _reader.MemoryHandler.SetProcess(new ProcessModel { Process = ReaderHandler.Game.Process });
        _reader.MemoryHandler.SignaturesFoundEvent += SignaturesFound;
        _reader.MemoryHandler.ExceptionEvent       += ExceptionEvent;
    }

    private void DestroySharlayan()
    {
        try
        {
            if (_reader != null) _reader.MemoryHandler.SignaturesFoundEvent -= SignaturesFound;
        }
        catch { }

        try
        {
            if (_reader != null) _reader.MemoryHandler.ExceptionEvent -= ExceptionEvent;
        }
        catch { }

        try
        {
            _reader?.MemoryHandler.UnsetProcess();
        }
        catch { }

        try
        {
            if (_reader != null) _reader.Scanner = null;
        }
        catch { }

        try
        {
            if (_reader != null) _reader.MemoryHandler = null;
        }
        catch { }

        _reader          = null;
        _signaturesFound = false;
        _lastScan        = default;
    }

    private void SignaturesFound(object sender, SignaturesFoundEvent signaturesFoundEvent)
    {
        if (!signaturesFoundEvent.Signatures.Keys.Contains("CHATINPUT"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("CHATINPUT")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("WORLD"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("WORLD")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("CHARID"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("CHARID")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("PERFSTATUS"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("PERFSTATUS")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("PLAYERINFO"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("PLAYERINFO")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("PARTYMAP"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("PARTYMAP")));

        if (!signaturesFoundEvent.Signatures.Keys.Contains("PARTYCOUNT"))
            ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, new BmpSeerSharlayanSigException("PARTYCOUNT")));

        _signaturesFound                           =  true;
        _reader.MemoryHandler.SignaturesFoundEvent -= SignaturesFound;
    }

    private void ExceptionEvent(object sender, ExceptionEvent exceptionEvent) =>
        ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, exceptionEvent.Exception));

    private ScanItems _lastScan;

    private struct ScanItems
    {
        public bool FirstScan;
        public int PreviousArrayIndex;
        public int PreviousOffset;
        public string PlayerName;
        public bool IsLoggedIn;
        public bool ChatOpen;
    }

    public async Task Loop(CancellationToken token)
    {
        InitializeSharlayan();

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (!_signaturesFound || _reader.Scanner.IsScanning)
                {
                    await Task.Delay(SleepTimeInMs, token);
                    continue;
                }

                GetPlayerInfo(token);
                GetChatInputOpen(token);
                GetChatLog(token);

                _lastScan.FirstScan = false;
            }
            catch (Exception ex)
            {
                ReaderHandler.Game.PublishEvent(new BackendExceptionEvent(EventSource.Sharlayan, ex));
            }

            await Task.Delay(SleepTimeInMs, token);
        }

        DestroySharlayan();
    }

    public void Dispose()
    {
        DestroySharlayan();
        GC.SuppressFinalize(this);
    }

    private void GetChatLog(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (!_reader.CanGetChatLog()) return;

        var result = _reader.GetChatLog(_lastScan.PreviousArrayIndex, _lastScan.PreviousOffset);
        _lastScan.PreviousArrayIndex = result.PreviousArrayIndex;
        _lastScan.PreviousOffset     = result.PreviousOffset;

        if (!result.ChatLogItems.Any()) return;

        ReaderHandler.Game.PublishEvent(new EnsembleNone(EventSource.Sharlayan,
            result.ChatLogItems.Select(c => $"{c.Line} ({c.Code})").ToArray()));
    }

    private void GetPlayerInfo(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (!_reader.CanGetPlayerInfo()) return;

        var kvp = _reader.GetCurrentPlayer();
        if (!_lastScan.FirstScan && _lastScan.PlayerName.Equals(kvp.Value.Item1) &&
            _lastScan.IsLoggedIn.Equals(kvp.Value.Item3)) return;

        _lastScan.PlayerName = kvp.Value.Item1;
        _lastScan.IsLoggedIn = kvp.Value.Item3;
        ReaderHandler.Game.PublishEvent(new PlayerNameChanged(EventSource.Sharlayan, _lastScan.PlayerName));
        ReaderHandler.Game.PublishEvent(new IsLoggedInChanged(EventSource.Sharlayan, _lastScan.IsLoggedIn));
    }

    private void GetChatInputOpen(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;
        if (!_reader.CanGetChatInput()) return;

        var result = _reader.IsChatInputOpen();
        if (!_lastScan.FirstScan && _lastScan.ChatOpen.Equals(result)) return;

        _lastScan.ChatOpen = result;
        ReaderHandler.Game.PublishEvent(new ChatStatusChanged(EventSource.Sharlayan, result));
    }
}
