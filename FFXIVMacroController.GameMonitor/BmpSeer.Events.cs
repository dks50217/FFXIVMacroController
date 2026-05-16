/*
 * Copyright(c) 2022 MoogleTroupe, trotlinebeercan, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FFXIVMacroController.GameMonitor.Events;

namespace FFXIVMacroController.GameMonitor;

public partial class BmpSeer
{
    public delegate void SeerExceptionEventHandler(SeerExceptionEvent seerExceptionEvent);
    public event SeerExceptionEventHandler SeerExceptionEvent;
    private void OnSeerExceptionEvent(SeerExceptionEvent seerExceptionEvent) =>
        SeerExceptionEvent?.Invoke(seerExceptionEvent);

    public delegate void BackendExceptionEventHandler(BackendExceptionEvent seerExceptionEvent);
    public event BackendExceptionEventHandler BackendExceptionEvent;
    private void OnBackendExceptionEvent(BackendExceptionEvent seerExceptionEvent) =>
        BackendExceptionEvent?.Invoke(seerExceptionEvent);

    public delegate void GameExceptionEventHandler(GameExceptionEvent seerExceptionEvent);
    public event GameExceptionEventHandler GameExceptionEvent;
    private void OnGameExceptionEvent(GameExceptionEvent seerExceptionEvent) =>
        GameExceptionEvent?.Invoke(seerExceptionEvent);

    public delegate void GameStartedHandler(GameStarted seerEvent);
    public event GameStartedHandler GameStarted;
    private void OnGameStarted(GameStarted seerEvent) => GameStarted?.Invoke(seerEvent);

    public delegate void GameStoppedHandler(GameStopped seerEvent);
    public event GameStoppedHandler GameStopped;
    private void OnGameStopped(GameStopped seerEvent) => GameStopped?.Invoke(seerEvent);

    public delegate void ChatStatusChangedHandler(ChatStatusChanged seerEvent);
    public event ChatStatusChangedHandler ChatStatusChanged;
    private void OnChatStatusChanged(ChatStatusChanged seerEvent) => ChatStatusChanged?.Invoke(seerEvent);

    public delegate void IsLoggedInChangedHandler(IsLoggedInChanged seerEvent);
    public event IsLoggedInChangedHandler IsLoggedInChanged;
    private void OnIsLoggedInChanged(IsLoggedInChanged seerEvent) => IsLoggedInChanged?.Invoke(seerEvent);

    public delegate void PlayerNameChangedHandler(PlayerNameChanged seerEvent);
    public event PlayerNameChangedHandler PlayerNameChanged;
    private void OnPlayerNameChanged(PlayerNameChanged seerEvent) => PlayerNameChanged?.Invoke(seerEvent);

    public delegate void IsGotChatLogHandler(EnsembleNone seerEvent);
    public event IsGotChatLogHandler IsGotChatLog;
    private void OnIsGotChatLog(EnsembleNone seerEvent) => IsGotChatLog?.Invoke(seerEvent);

    private async Task RunEventsHandler(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (_eventQueue.TryDequeue(out var seerEvent))
            {
                if (token.IsCancellationRequested)
                    break;

                switch (seerEvent)
                {
                    case BackendExceptionEvent backendExceptionEvent:
                        OnBackendExceptionEvent(backendExceptionEvent);
                        break;
                    case GameExceptionEvent gameExceptionEvent:
                        OnGameExceptionEvent(gameExceptionEvent);
                        break;
                    case SeerExceptionEvent seerExceptionEvent:
                        OnSeerExceptionEvent(seerExceptionEvent);
                        break;
                    case GameStarted gameStarted:
                        OnGameStarted(gameStarted);
                        break;
                    case GameStopped gameStopped:
                        OnGameStopped(gameStopped);
                        break;
                    case ChatStatusChanged chatStatusChanged:
                        OnChatStatusChanged(chatStatusChanged);
                        break;
                    case IsLoggedInChanged isLoggedInChanged:
                        OnIsLoggedInChanged(isLoggedInChanged);
                        break;
                    case PlayerNameChanged playerNameChanged:
                        OnPlayerNameChanged(playerNameChanged);
                        break;
                    case EnsembleNone isGotChatLog:
                        OnIsGotChatLog(isGotChatLog);
                        break;
                }
            }

            await Task.Delay(1, token).ContinueWith(tsk=> { }, token);
        }
    }

    internal void PublishEvent(SeerEvent seerEvent)
    {
        if (!_eventQueueOpen)
            return;

        _eventQueue.Enqueue(seerEvent);
    }

    private ConcurrentQueue<SeerEvent> _eventQueue;
    private bool _eventQueueOpen;
    private CancellationTokenSource _eventsTokenSource;

    private void StartEventsHandler()
    {
        _eventQueue = new ConcurrentQueue<SeerEvent>();

        _eventsTokenSource = new CancellationTokenSource();
        Task.Factory.StartNew(() => RunEventsHandler(_eventsTokenSource.Token), TaskCreationOptions.LongRunning);

        _eventQueueOpen = true;
    }

    private void StopEventsHandler()
    {
        _eventQueueOpen = false;
        _eventsTokenSource.Cancel();
        while (_eventQueue.TryDequeue(out _))
        {
        }
    }
}
