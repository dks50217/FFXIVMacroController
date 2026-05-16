/*
 * Copyright(c) 2022 MoogleTroupe, trotlinebeercan, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using FFXIVMacroController.GameMonitor.Events;

namespace FFXIVMacroController.GameMonitor;

public partial class Game
{
    private void OnEventReceived(SeerEvent seerEvent)
    {
        try
        {
            // make sure it's valid to begin with and from a backend.
            if (!seerEvent.IsValid() || 10 > (int) seerEvent.EventSource) return;

            seerEvent.Game = this;

            // pass exceptions up immediately
            if (seerEvent is BackendExceptionEvent backendExceptionEvent)
            {
                BmpSeer.Instance.PublishEvent(backendExceptionEvent);
                return;
            }

            // deduplicate if needed
            if (seerEvent.DedupeThreshold > 0)
            {
                if (_eventDedupeHistory.ContainsKey(seerEvent.EventType) &&
                    _eventDedupeHistory[seerEvent.EventType] + seerEvent.DedupeThreshold >=
                    seerEvent.TimeStamp) return;

                _eventDedupeHistory[seerEvent.EventType] = seerEvent.TimeStamp;
            }

            switch (seerEvent)
            {
                case ChatStatusChanged chatStatus:
                    if (ChatStatus != chatStatus.ChatStatus)
                    {
                        ChatStatus = chatStatus.ChatStatus;
                        BmpSeer.Instance.PublishEvent(chatStatus);
                    }
                    break;

                case IsLoggedInChanged isLoggedIn:
                    if (IsLoggedIn != isLoggedIn.IsLoggedIn)
                    {
                        IsLoggedIn = isLoggedIn.IsLoggedIn;
                        BmpSeer.Instance.PublishEvent(isLoggedIn);
                    }
                    break;

                case PlayerNameChanged playerName:
                    if (!PlayerName.Equals(playerName.PlayerName))
                    {
                        PlayerName = playerName.PlayerName;
                        BmpSeer.Instance.PublishEvent(playerName);
                    }
                    break;

                case EnsembleNone chatLog:
                    BmpSeer.Instance.PublishEvent(chatLog);
                    break;
            }
        }
        catch (Exception ex)
        {
            BmpSeer.Instance.PublishEvent(new GameExceptionEvent(this, Pid, ex));
        }
    }
}
