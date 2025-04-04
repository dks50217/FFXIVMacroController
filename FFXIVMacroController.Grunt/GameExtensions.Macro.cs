/*
 * Copyright(c) 2021 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FFXIVMacroController.Grunt.Helper.Utilities;
using FFXIVMacroController.Quotidian.Enums;
using FFXIVMacroController.Seer;

namespace FFXIVMacroController.Grunt;

public static partial class GameExtensions
{
    private static readonly SemaphoreSlim LyricSemaphoreSlim = new (1,1);

    /// <summary>
    /// 送按鍵到遊戲中
    /// </summary>
    /// <param name="game"></param>
    /// <param name="keyArray"></param>
    /// <returns></returns>
    public static async Task<bool> SendKeyArray(this Game game, Keys key)
    {
        var sent = false;

        await LyricSemaphoreSlim.WaitAsync();

        try
        {
            var tcs = new TaskCompletionSource<bool>();
            var clipboardThread = new Thread(() => SendKeyInputTask(tcs, game, key));

#pragma warning disable CA1416
            clipboardThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416
            clipboardThread.Start();
            sent = await tcs.Task;
        }
        catch (Exception)
        {
            // TODO: Log errors
        }
        finally
        {
            LyricSemaphoreSlim.Release();
        }

        return sent;
    }


    /// <summary>
    /// 送到對話框
    /// </summary>
    /// <param name="game"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static async Task<bool> SendLyricLine(this Game game, string text)
    {
        if (!BmpGrunt.Instance.Started) throw new BmpGruntException("Grunt not started.");

        var sent = false;

        await LyricSemaphoreSlim.WaitAsync();

        try
        {
            var tcs = new TaskCompletionSource<bool>();
            var clipboardThread = new Thread(() => SendLyricLineClipBoardTask(tcs, game, text));
#pragma warning disable CA1416
            clipboardThread.SetApartmentState(ApartmentState.STA);
#pragma warning restore CA1416
            clipboardThread.Start();
            sent = await tcs.Task;
        }
        catch (Exception)
        {
            // TODO: Log errors
        }
        finally
        {
            LyricSemaphoreSlim.Release();
        }

        return sent;
    }

    private static void SendKeyInputTask(TaskCompletionSource<bool> tcs, Game game, Keys key)
    {
        try
        {
            var result = true;

            if (!SyncTapKey(game, key).GetAwaiter().GetResult())
            {
                result = false;
            }

            tcs.SetResult(result);
        }
        catch (Exception)
        {
            // TODO: Log errors
            tcs.SetResult(false);
        }
    }

    private static void SendLyricLineClipBoardTask(TaskCompletionSource<bool> tcs, Game game, string text)
    {
        try {
            
            if (!game.ChatStatus && !SyncTapKey(game, Keys.Enter).GetAwaiter().GetResult())
            {
                tcs.SetResult(false);
                return;
            }

            text.CopyToClipboard();

            var result = true;

            if (!SyncTapKey(game, (int)Keys.Control + Keys.V).GetAwaiter().GetResult())
            {
                result = false;
            }

            else if (!SyncTapKey(game, Keys.Enter).GetAwaiter().GetResult())
            {
                result = false;
            }

            tcs.SetResult(result);
        }
        catch(Exception) {
            // TODO: Log errors
            tcs.SetResult(false);
        }
    }
}