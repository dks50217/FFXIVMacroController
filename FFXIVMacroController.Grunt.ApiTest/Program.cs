/*
 * Copyright(c) 2023 MoogleTroupe
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayerApi/blob/develop/LICENSE for full license information.
 */

using System;
using System.Threading.Tasks;
using FFXIVMacroController.Pigeonhole;
using FFXIVMacroController.Quotidian.Structs;
using FFXIVMacroController.Seer;
using FFXIVMacroController.Seer.Events;

namespace FFXIVMacroController.Grunt.ApiTest;

internal class Program
{
    private static void Main()
    {
        BmpPigeonhole.Initialize(AppContext.BaseDirectory + @"\Grunt.ApiTest.json");

        BmpSeer.Instance.GameStarted += SendTest;

        BmpSeer.Instance.SetupFirewall("BardMusicPlayer.Grunt.ApiTest");

        Console.WriteLine("Hit enter to start Grunt");

        Console.ReadLine();

        while (true)
        {
            BmpSeer.Instance.Start();

            BmpGrunt.Instance.Start();

            Console.ReadLine();

            BmpGrunt.Instance.Stop();

            BmpSeer.Instance.Stop();

            Console.WriteLine("Grunt stopped. Hit enter to start it again.");

            Console.ReadLine();
        }
    }

    private static void SendTest(GameStarted seerEvent)
    {
        var game = seerEvent.Game;

        Console.WriteLine("Detected game pid " + game.Pid + ", sleep a thread for 3000ms to allow Seer to parse the dat files.");

        Task.Run(async () =>
        {
            await Task.Delay(3000);
            Console.WriteLine("Trying to doot on game pid " + game.Pid + ".");
            if (game != null && !await game.SendLyricLine("" + game.PlayerName + " is singing" + (game.IsDalamudHooked() ? " with Dalamud!" : " with copy paste!"))) Console.WriteLine("Failed to tell game pid " + game.Pid + " to sing a line :(");
            //if (game != null && !await game.SendLyricLine("/echo " + game.PlayerName + " is singing" + (game.IsDalamudHooked() ? " with Dalamud!" : " with copy paste!"))) Console.WriteLine("Failed to tell game pid " + game.Pid + " to sing a line :(");
        });
    }
}