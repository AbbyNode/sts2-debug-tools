# STS2 Debug Tools

A **Slay the Spire 2** mod that adds debugging utilities to any run:

| Feature | When available | What it does |
|---|---|---|
| **Win Battle** (`F5`) | During combat | Instantly kills all primary enemies and triggers the normal end-of-combat flow (rewards, map unlock, etc.). |
| **Right-click Kill** | During combat | Right-click an enemy to kill it instantly. *(Requires `NRun.Instance` — may not work on all versions.)* |
| **Map Teleport** | On the map (between rooms) | Enables debug travel mode on the map screen — click any room to teleport there directly, bypassing normal travel restrictions. |

**Win Battle** is triggered by pressing **F5** during combat. **Map Teleport** is always active whenever the map screen is open (no button required).

---

## Installation

Drop the `Sts2DebugTools/` folder (containing `Sts2DebugTools.dll` and `Sts2DebugTools.json`) into your Slay the Spire 2 `mods/` directory.

**Dependency:** [BaseLib](https://github.com/Alchyr/sts2-base-lib) must be installed.

---

## Building from source

Prerequisites: **.NET 9 SDK** and a local install of **Slay the Spire 2** (Godot is only needed if you want to export a `.pck` via `dotnet publish`).

1. Edit `Directory.Build.props` to verify the paths match your local setup.
2. `dotnet build -c Release`

The compiled DLL and manifest are copied to your mods folder automatically.

---

## Implementation notes

* **Win Battle** — press **F5** during combat. Uses `AccessTools.Field` (Harmony) to reflectively set each primary enemy's `_currentHp` field to 0, fires `InvokeDiedEvent()`, then calls `CombatManager.Instance.CheckWinCondition()` to let the game run its normal victory sequence. The key listener is attached to the `SceneTree` root viewport (via `Engine.GetMainLoop()`), so it works even when `NRun.Instance` is unavailable.
* **Right-click Kill** — right-click a hovered enemy during combat to kill it. Attaches an `InputListenerNode` to `NRun.Instance` (may be unavailable in some game versions; the **Win Battle** shortcut is a reliable alternative).
* **Map Teleport** — a Harmony postfix on `NMapScreen.Open` calls `SetDebugTravelEnabled(true)` each time the map screen opens, enabling the game's built-in debug travel mode so the player can click any room directly.
