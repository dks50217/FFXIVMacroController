# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Publish

```shell
# Build (Debug)
dotnet build

# Build (Release)
dotnet build -c Release

# Publish as single self-contained exe
dotnet publish -c Release --self-contained -r win-x64 -p:PublishSingleFile=true
```

The solution targets Windows only. The main app requires `net8.0-windows`; the library projects target `net7.0`.

## Solution Structure

Four projects in `FFXIVMacroController.sln`:

| Project | Framework | Role |
|---|---|---|
| `FFXIVMacroController` | net8.0-windows | Main WPF app — hosts Blazor WebView UI |
| `FFXIVMacroController.InputSender` | net7.0 | Sends key/text input to the FFXIV game window (uses H.Pipes) |
| `FFXIVMacroController.GameMonitor` | net7.0 | Monitors FFXIV processes; reads game memory via Sharlayan |
| `FFXIVMacroController.Common` | net7.0 | Shared enums, structs, and types |

> Previously named: Grunt → InputSender, Seer → GameMonitor, Quotidian → Common.

## Architecture

### UI Layer
`MainWindow.xaml.cs` is a WPF window that hosts a `BlazorWebView`. The UI is built entirely with Razor components (`Components/Pages/`) using [Radzen Blazor](https://blazor.radzen.com/) components. Services are registered via standard `IServiceCollection` DI in `MainWindow` constructor:
- `IUpdateService` / `UpdateService`
- `IAutoClickService` / `AutoClickService`

### Singleton Services (BmpSeer / BmpGrunt)
- **`BmpSeer.Instance`** — starts a process watcher that detects running FFXIV instances (stored as `Game` objects keyed by PID in a `ConcurrentDictionary`). Uses Sharlayan for memory reading.
- **`BmpGrunt.Instance`** — requires Seer to be running. Provides the actual input sending capability. Must be started before sending any input.

Both are lazy singletons started in `MainWindow` on `Loaded`, and can be restarted mid-session from `Home.razor` when the user clicks Play.

### Data Model
Config is loaded from/saved to `config.json` in the app's base directory.

```
MacroRootModel
  └─ List<CategoryModel>   (a "script" the user can run)
        ├─ repeat: int      (how many times to loop)
        └─ List<MacroModel>
              ├─ type: Types       (button | text | mouse)
              ├─ keyNumber: int    (maps to Keys enum)
              ├─ sleep: int        (seconds to wait after action)
              ├─ inputText: string (for Types.text)
              ├─ coordinateX: int  (for Types.mouse, not yet enabled in UI)
              └─ coordinateY: int  (for Types.mouse, not yet enabled in UI)
```

`EventHelper.ConvertJsonToList()` parses the JSON manually via `JsonDocument` (not via `JsonSerializer.Deserialize`).

### Input Execution
`EventHelper.SendInput_Token(game, macro, cancellationToken)` is the main per-step execution method called from `Home.razor`'s `handlePlay()`:
- `Types.button` → `game.SendKeyArray(key)` — sends a Win32 key message to the game window
- `Types.text` → `game.SendLyricLine(text)` — opens chat (sends Enter), pastes via clipboard (Ctrl+V), confirms (Enter)

Both methods in `GameExtensions.Macro.cs` (inside InputSender) use a `SemaphoreSlim` + dedicated STA thread to handle clipboard operations safely.

`handlePlay()` also supports an optional random delay (100–300ms) between steps when the user enables the "開啟隨機間隔秒數" setting in the UI.

### Auto-Update
`UpdateService` checks GitHub releases via Octokit on startup, downloads the zip, then launches `update.ps1` and shuts down the app.
