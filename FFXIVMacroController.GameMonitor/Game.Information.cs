/*
 * Copyright(c) 2022 MoogleTroupe, trotlinebeercan, GiR-Zippo
 * Licensed under the GPL v3 license. See https://github.com/BardMusicPlayer/BardMusicPlayer/blob/develop/LICENSE for full license information.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FFXIVMacroController.Common.Enums;
using FFXIVMacroController.GameMonitor.Utilities;
using FFXIVMacroController.GameMonitor.Utilities.KnownFolder;

namespace FFXIVMacroController.GameMonitor;

public partial class Game
{
    private void InitInformation()
    {
        GamePath        = GetGamePath();
        EnvironmentType = GetEnvironmentType();
        GameRegion      = GetGameRegion();
        ConfigPath      = GetConfigPath();
    }

    /// <summary>
    /// Contains the Process object for this Game. Set on creation of this Game.
    /// </summary>
    public Process Process { get; }

    /// <summary>
    /// Contains the Process Id for this Game. Set on creation of this Game.
    /// </summary>
    public int Pid { get; private set; }

    /// <summary>
    /// Contains the region of this Game. Set on creation of this Game.
    /// </summary>
    public GameRegion GameRegion { get; private set; }

    /// <summary>
    /// Contains the environment of this Game. Set on creation of this Game.
    /// </summary>
    public EnvironmentType EnvironmentType { get; private set; }

    /// <summary>
    /// Contains the path to the boot/game folders for this Game. Set on creation of this Game.
    /// </summary>
    public string GamePath { get; private set; }

    /// <summary>
    /// Contains the path to the game configuration files like ffxiv.cfg and ConfigId folders. Set on creation of this Game.
    /// </summary>
    public string ConfigPath { get; private set; }

    /// <summary>
    /// Shows the player's name. Updated by Sharlayan.
    /// </summary>
    public string PlayerName { get; private set; } = "Unknown";

    /// <summary>
    /// Shows if the player is logged in.
    /// </summary>
    public bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Shows if the chatbox is open for input. Updated by Sharlayan.
    /// </summary>
    public bool ChatStatus { get; private set; }

    /// <summary>
    ///     Indicates if gfx set to low
    /// </summary>
    public bool GfxSettingsLow { get; set; }

    private string GetGamePath()
    {
        try
        {
            var gamePath = Process.Modules.Cast<ProcessModule>()
                .Aggregate("",
                    (current, module) =>
                        module.ModuleName.ToLower() switch
                        {
                            "ffxiv_dx11.exe" => System.IO.Directory
                                .GetParent(System.IO.Path.GetDirectoryName(module.FileName) ?? string.Empty)
                                ?.FullName,
                            _ => current
                        }
                );

            if (string.IsNullOrEmpty(gamePath))
            {
                throw new BmpSeerGamePathException(
                    "Cannot locate the running directory of this game's ffxiv_dx11.exe");
            }

            return gamePath + @"\";
        }
        catch (Exception ex)
        {
            throw new BmpSeerGamePathException(
                "Unexpected error while trying to locate the path to ffxiv_dx11.exe: " + Environment.NewLine +
                ex.Message);
        }
    }

    private EnvironmentType GetEnvironmentType()
    {
        try
        {
            var modules = Process.Modules;
            var environmentType = modules.Cast<ProcessModule>()
                .Aggregate(EnvironmentType.Normal, (current, module) => module.ModuleName.ToLower() switch
                {
                    "sbiedll.dll"    => EnvironmentType.Sandboxie,
                    "innerspace.dll" => EnvironmentType.InnerSpace,
                    _                => current
                });
            return environmentType;
        }
        catch (Exception ex)
        {
            throw new BmpSeerEnvironmentTypeException(
                "Unexpected error while trying to detect the environment of a running game: " +
                Environment.NewLine + ex.Message);
        }
    }

    private GameRegion GetGameRegion()
    {
        try
        {
            var gameRegion = GameRegion.Global;

            if (File.Exists(GamePath + @"boot\locales\ko.pak")) gameRegion = GameRegion.Korea;
            else if (Directory.Exists(GamePath + @"sdo")) gameRegion       = GameRegion.China;

            return gameRegion;
        }
        catch (Exception ex)
        {
            throw new BmpSeerGameRegionException(
                "Unexpected error while trying to detect the region of a running game: " + Environment.NewLine +
                ex.Message);
        }
    }

    private string GetConfigPath()
    {
        var partialConfigPath = GameRegion == GameRegion.Korea
            ? @"My Games\FINAL FANTASY XIV - KOREA\"
            : @"My Games\FINAL FANTASY XIV - A Realm Reborn\";
        var configPath = "";

        try
        {
            if (EnvironmentType == EnvironmentType.Sandboxie)
            {
                var sandboxieConfigFilePath = "";

                try
                {
                    if (File.Exists(
                            Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\Sandboxie.ini"))
                    {
                        sandboxieConfigFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) +
                                                  @"\Sandboxie.ini";
                    }
                    else
                    {
                        sandboxieConfigFilePath = System.Diagnostics.Process.GetProcesses()
                            .Where(process => process.ProcessName.ToLower().Equals("sbiectrl"))
                            .Select(sandboxieProcess => sandboxieProcess.Modules)
                            .Aggregate(sandboxieConfigFilePath, (current1, sandboxieModules) => sandboxieModules
                                .Cast<ProcessModule>()
                                .Aggregate(current1, (current, sandboxieModule) =>
                                    sandboxieModule.ModuleName.ToLower() switch
                                    {
                                        "sbiectrl.exe" => System.IO.Path.GetDirectoryName(sandboxieModule.FileName) +
                                                          @"\Sandboxie.ini",
                                        _ => current
                                    }));
                    }
                }
                finally
                {
                    if (string.IsNullOrEmpty(sandboxieConfigFilePath))
                    {
                        throw new BmpSeerConfigPathException(
                            "This game is running in Sandboxie, however the Sandboxie.ini configuration file could not be found.");
                    }
                }

                var boxName = Process.MainWindowTitle.Split('[')[2].Split(']')[0];

                var boxRoot = File.ReadLines(sandboxieConfigFilePath, Encoding.Unicode)
                    .First(line => line.StartsWith("BoxRootFolder"))
                    .Split('=').Last() + @"\Sandbox\" + boxName + @"\";

                if (Directory.Exists(boxRoot))
                {
                    if (GameRegion == GameRegion.China)
                    {
                        configPath = boxRoot + GamePath.Substring(0, 1) + @"\" +
                                     GamePath.Substring(2, GamePath.Length) + @"game\" + partialConfigPath;
                    }
                    else configPath = boxRoot + @"user\current\Documents\" + partialConfigPath;
                }
                else
                {
                    throw new BmpSeerConfigPathException(
                        "This game is running in Sandboxie, however the sandbox could not be found.");
                }
            }
            else
            {
                if (GameRegion == GameRegion.China) configPath = GamePath + @"game\" + partialConfigPath;
                else
                {
                    configPath = new KnownFolder(KnownFolderType.Documents, Process.WindowsIdentity()).Path + @"\" +
                                 partialConfigPath;
                }
            }

            if (string.IsNullOrEmpty(configPath) || !Directory.Exists(configPath))
            {
                throw new BmpSeerConfigPathException(
                    "Invalid config path for a running game: " + Environment.NewLine + configPath);
            }

            return configPath;
        }
        catch (Exception ex)
        {
            throw new BmpSeerConfigPathException(
                "Unexpected error while trying to locate the config path for a running game: "
                + Environment.NewLine + "PartialConfigPath: " + partialConfigPath
                + Environment.NewLine + "ConfigPath: " + configPath
                + Environment.NewLine + ex.Message);
        }
    }
}
